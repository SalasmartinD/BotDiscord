using Discord;
using Discord.Commands;
using System.Threading.Tasks;

public class HelpModule : ModuleBase<SocketCommandContext>
{
    private readonly CommandService _service;

    // Inyección de dependencias: El sistema nos da el servicio de comandos automáticamente
    public HelpModule(CommandService service)
    {
        _service = service;
    }

    [Command("help")]
    [Alias("ayuda")] // Permite usar >ayuda también
    [Summary("Muestra este mensaje de ayuda.")]
    public async Task HelpAsync()
    {
        var builder = new EmbedBuilder()
        {
            Title = "📜 Panel de Ayuda",
            Description = "Aquí tienes la lista de comandos disponibles:",
            Color = Color.Blue
        };

        // Recorremos todos los Módulos (Archivos de comandos)
        foreach (var module in _service.Modules)
        {
            string? description = null;

            // Recorremos todos los comandos dentro de ese módulo
            foreach (var cmd in module.Commands)
            {
                // Ignoramos el comando help para que no sea redundante (opcional)
                var result = await cmd.CheckPreconditionsAsync(Context);
                
                // Si el usuario tiene permisos para usarlo, lo mostramos
                if (result.IsSuccess) 
                {
                    // Formato: >comando - Descripción
                    description += $"`>{cmd.Name}`: {cmd.Summary ?? "Sin descripción"}\n";
                }
            }

            // Si el módulo tiene comandos disponibles, lo agregamos al Embed
            if (!string.IsNullOrWhiteSpace(description))
            {
                // Usamos el nombre del módulo (o el nombre de la clase si no tiene nombre)
                builder.AddField(x =>
                {
                    x.Name = module.Name ?? "Otros";
                    x.Value = description;
                    x.IsInline = false;
                });
            }
        }

        builder.WithFooter($"Solicitado por {Context.User.Username}");

        await ReplyAsync("", false, builder.Build());
    }
}