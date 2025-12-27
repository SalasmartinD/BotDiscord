using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;
    
    // Inyecto el servicio de Gemini (puede ser nulo si no pusiste la key)
    private readonly GeminiService? _gemini;

    public CommandHandler(IServiceProvider services)
    {
        _services = services;
        _client = services.GetRequiredService<DiscordSocketClient>();
        _commands = services.GetRequiredService<CommandService>();
        
        // Intento obtener el servicio de IA
        _gemini = services.GetService<GeminiService>();
    }

    public async Task InstallCommandsAsync()
    {
        _client.MessageReceived += HandleCommandAsync;
        await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: _services);
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        var message = messageParam as SocketUserMessage;
        if (message == null || message.Author.IsBot) return;

        int argPos = 0;

        // ---------------------------------------------------------
        // EL PREFIJO ES '>'
        // ---------------------------------------------------------
        if (message.HasCharPrefix('>', ref argPos))
        {
            var context = new SocketCommandContext(_client, message);
            await _commands.ExecuteAsync(context, argPos, _services);
            return; 
        }

        // ---------------------------------------------------------
        // LÓGICA DE ASTOLFO (IA)
        // ---------------------------------------------------------
        string contenido = message.Content.Trim();
        var triggers = new List<string> { "astolfo", "rider", "bot" };
        
        // 1. DETECCIÓN FLEXIBLE (IndexOf)
        bool porNombre = triggers.Any(t => contenido.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0);
        bool porMencion = message.MentionedUsers.Any(u => u.Id == _client.CurrentUser.Id);
        bool porReplica = message.ReferencedMessage != null && 
                          message.ReferencedMessage.Author.Id == _client.CurrentUser.Id;

        if (porNombre || porMencion || porReplica)
        {
            if (_gemini == null) 
            {
                await message.Channel.SendMessageAsync("🚫 Falta API Key.");
                return;
            }

            using (message.Channel.EnterTypingState())
            {
                // -------------------------------------------------------
                // 2. RECUPERAR HISTORIAL
                // -------------------------------------------------------
                var mensajesAnteriores = await message.Channel.GetMessagesAsync(6).FlattenAsync();
                string historialChat = "";
                
                foreach (var msg in mensajesAnteriores.Reverse())
                {
                    if (msg.Id == message.Id) continue; // Salto el actual
                    if (msg.Content.StartsWith(">")) continue; // Salto comandos

                    historialChat += $"{msg.Author.Username}: {msg.Content}\n";
                }
                // -------------------------------------------------------

                // 3. LIMPIEZA INTELIGENTE
                string preguntaLimpia = contenido;

                foreach (var trigger in triggers)
                {
                    // Solo quito el nombre si está al principio
                    if (preguntaLimpia.StartsWith(trigger, StringComparison.OrdinalIgnoreCase))
                    {
                        preguntaLimpia = preguntaLimpia.Substring(trigger.Length).Trim(',', ' ', ':');
                        break;
                    }
                }

                if (string.IsNullOrEmpty(preguntaLimpia)) preguntaLimpia = "Hola, ¿estás ahí?";

                // 4. IA
                string respuesta = await _gemini.ResponderComoAstolfo(
                    historialChat, 
                    message.Author.Username, 
                    preguntaLimpia
                );
                
                // 5. ENVIAR (Chunking + Reply)
                if (respuesta.Length <= 1900)
                {
                    await message.Channel.SendMessageAsync(
                        respuesta, 
                        messageReference: new MessageReference(message.Id),
                        allowedMentions: AllowedMentions.All);
                }
                else
                {
                    int tamanoChunk = 1900;
                    for (int i = 0; i < respuesta.Length; i += tamanoChunk)
                    {
                        int largoRestante = Math.Min(tamanoChunk, respuesta.Length - i);
                        await message.Channel.SendMessageAsync(respuesta.Substring(i, largoRestante));
                        await Task.Delay(500); 
                    }
                }
            }
        }
    }
}