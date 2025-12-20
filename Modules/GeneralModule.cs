using Discord;
using Discord.Commands;

// Todos los módulos deben heredar de ModuleBase
public class GeneralModule : ModuleBase<SocketCommandContext>
{
    [Command("hola")]
    [Summary("Te saludo")]
    public async Task Hola()
    {
        // "Context.User" es quien envió el mensaje automáticamente
        await ReplyAsync($"Hola gordito");
    }

    [Command("info")]
    public async Task Info()
    {
        await ReplyAsync("Soy un bot modular hecho en C# 🤖, pero tambien me podes decir mi amor");
    }
}