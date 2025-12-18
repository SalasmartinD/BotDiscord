using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;

    // El constructor recibe las "piezas" necesarias
    public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services)
    {
        _client = client;
        _commands = commands;
        _services = services;
    }

    public async Task InstallCommandsAsync()
    {
        // Conectamos el evento de mensajes
        _client.MessageReceived += HandleCommandAsync;

        // Esto busca AUTOMÁTICAMENTE todas las clases que sean "Modules" en el proyecto
        // y las registra como comandos.
        await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: _services);
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        var message = messageParam as SocketUserMessage;
        if (message == null) return;

        int argPos = 0;

        // DEBUG: Ver si llega el mensaje
        // Console.WriteLine($"Mensaje recibido: {message.Content}"); 

        if (!(message.HasCharPrefix('>', ref argPos) || 
              message.HasMentionPrefix(_client.CurrentUser, ref argPos)) || 
              message.Author.IsBot)
            return;

        // DEBUG: Ver si detectó el prefijo
        Console.WriteLine($"Comando detectado: {message.Content}");

        var context = new SocketCommandContext(_client, message);

        var result = await _commands.ExecuteAsync(
            context: context, 
            argPos: argPos, 
            services: _services);

        // DEBUG: Ver si falló la ejecución interna
        if (!result.IsSuccess)
        {
            Console.WriteLine($"Error ejecutando comando: {result.ErrorReason}");
            await context.Channel.SendMessageAsync($"⚠ Error: {result.ErrorReason}");
        }
    }
}