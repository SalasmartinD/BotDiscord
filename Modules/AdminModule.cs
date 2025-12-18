using Discord;
using Discord.Commands;
using Discord.WebSocket;

public class AdminModule : ModuleBase<SocketCommandContext>
{
    [Command("kick")]
    [Summary("Expulsa a un usuario.")]
    [RequireUserPermission(GuildPermission.KickMembers)] // ¡Seguridad automática!
    public async Task Kick(SocketGuildUser usuario, [Remainder] string razon = "Sin razón")
    {
        await usuario.KickAsync(razon);
        await ReplyAsync($"👢 {usuario.Username} ha sido expulsado. Razón: {razon}");
    }

    [Command("limpiar")]
    [Summary("Borra mensajes.")]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    public async Task Limpiar(int cantidad)
    {
        // Lógica de borrado
        var mensajes = await Context.Channel.GetMessagesAsync(cantidad + 1).FlattenAsync();
        await ((ITextChannel)Context.Channel).DeleteMessagesAsync(mensajes);

        var msg = await ReplyAsync($"✅ Borrados {cantidad} mensajes.");
        await Task.Delay(3000);
        await msg.DeleteAsync();
    }
}