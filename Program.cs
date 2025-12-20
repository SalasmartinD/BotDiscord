using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.IO; // Necesario para File.ReadAllText
using Renci.SshNet;

public class Program
{
    // Clase simple para mapear el JSON
    public class Config 
    { 
        public string? Token { get; set; } 
    }

    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
    {
        // ---------------------------------------------------------
        // 1. CARGA DE CONFIGURACIÓN (Token)
        // ---------------------------------------------------------
        // Primero verifico si existe el archivo para evitar crashes feos
        if (!File.Exists("appsettings.json"))
        {
            Console.WriteLine("ERROR FATAL: No se encontró el archivo 'appsettings.json'.");
            Console.WriteLine("Asegúrate de crearlo en la misma carpeta del ejecutable con tu Token.");
            return; // Detiene el programa
        }

        string textoJson = File.ReadAllText("appsettings.json");
        var config = JsonSerializer.Deserialize<Config>(textoJson);

        // Verifico que el token no haya venido vacío
        if (string.IsNullOrEmpty(config?.Token))
        {
            Console.WriteLine("ERROR: El archivo json existe, pero el Token está vacío o mal escrito.");
            return;
        }

        // ---------------------------------------------------------
        // 2. CONFIGURACIÓN DE SERVICIOS
        // ---------------------------------------------------------
        using var services = ConfigureServices();
        
        var client = services.GetRequiredService<DiscordSocketClient>();
        var commands = services.GetRequiredService<CommandService>();
        var handler = services.GetRequiredService<CommandHandler>();

        // Logueo rápido para consola
        client.Log += LogAsync;
        commands.Log += LogAsync;

        // Iniciar el manejador de comandos
        await handler.InstallCommandsAsync();

        // ---------------------------------------------------------
        // 3. LOGIN Y ARRANQUE
        // ---------------------------------------------------------
        // Usamos el token que leímos del archivo json
        await client.LoginAsync(TokenType.Bot, config.Token);
        await client.StartAsync();

        // Poner estado
        await client.SetGameAsync("Jugando al teto", type: ActivityType.Playing);

        // Mantiene el bot vivo infinitamente
        await Task.Delay(-1);
    }

    // Configura la inyección de dependencias
    private ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddSingleton(new DiscordSocketConfig 
            { 
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers 
            })
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandler>()
            .BuildServiceProvider();
    }

    private Task LogAsync(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}