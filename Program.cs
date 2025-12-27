using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.IO;

public class Program
{
    // Actualizo la clase para leer la Key de Google
    public class Config 
    { 
        public string? Token { get; set; } 
        public string? GoogleApiKey { get; set; }
        public PcConfigData? PcConfig { get; set; } 
    }
    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
    {
        // 1. CARGA DE CONFIGURACIÓN
        if (!File.Exists("appsettings.json"))
        {
            Console.WriteLine("ERROR FATAL: Falta appsettings.json");
            return;
        }

        string textoJson = File.ReadAllText("appsettings.json");
        var config = JsonSerializer.Deserialize<Config>(textoJson);

        if (string.IsNullOrEmpty(config?.Token))
        {
            Console.WriteLine("ERROR: Token vacío.");
            return;
        }

        // 2. CONFIGURACIÓN DE SERVICIOS
        // Pasamos la API Key al método configurador
        using var services = ConfigureServices(config.GoogleApiKey); 
        
        var client = services.GetRequiredService<DiscordSocketClient>();
        var commands = services.GetRequiredService<CommandService>();
        var handler = services.GetRequiredService<CommandHandler>();
    
        client.Log += LogAsync;
        commands.Log += LogAsync;

        // Iniciar el manejador
        await handler.InstallCommandsAsync();

        // 3. LOGIN
        await client.LoginAsync(TokenType.Bot, config.Token);
        await client.StartAsync();

        await client.SetGameAsync("Jugando al teto", type: ActivityType.Playing);

        await Task.Delay(-1);
    }

    // Modifico para recibir la apiKey
    private ServiceProvider ConfigureServices(string? googleApiKey)
    {
        var services = new ServiceCollection()
            .AddSingleton(new DiscordSocketConfig 
            { 
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers 
            })
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandler>();

        // REGISTRAMOS EL SERVICIO DE IA (GEMINI)
        if (!string.IsNullOrEmpty(googleApiKey))
        {
            services.AddSingleton(new GeminiService(googleApiKey));
        }
        else
        {
            Console.WriteLine("⚠️ ADVERTENCIA: No se encontró GoogleApiKey en el JSON. La IA no funcionará.");
        }

        return services.BuildServiceProvider();
    }

    private Task LogAsync(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}