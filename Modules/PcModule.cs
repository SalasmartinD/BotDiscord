using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using DiscordBot.Services;

namespace DiscordBot.Modules
{
    [Name("Control de PC")]
    public class PcModule : ModuleBase<SocketCommandContext>
    {
        // Instancio el controlador
        private readonly PcController _pcController = new PcController();
        // COMANDO: PRENDER
        [Command("pcon")]
        [Summary("Prende la PC Gamer mediante Wake on LAN.")]
        [RequireRole("Admin-MC")] 

        public async Task Encender()
        {
            await ReplyAsync("⚡ Enviando señal mágica a la PC...");
            await _pcController.EncenderPc();
        }

        // COMANDO: APAGAR
        [Command("pcoff")]
        [Summary("Apaga la PC Gamer mediante SSH.")]
        [RequireRole("Admin-MC")] // Solo gente con este rol puede apagarla
        public async Task Apagar()
        {
            await ReplyAsync("💀 Iniciando secuencia de apagado remoto...");
            try
            {
                _pcController.ApagarPc();
                await ReplyAsync("✅ Comando de apagado enviado con éxito.");
            }
            catch (System.Exception ex)
            {
                await ReplyAsync($"❌ Error crítico: {ex.Message}");
            }
        }

        // COMANDO: ABRIR PROGRAMA
        [Command("ejecutar")]
        [Summary("Abre un programa específico.")]
        [RequireRole("Admin-MC")]
        public async Task Ejecutar([Remainder] string programa)
        {
            // Lógica simple para elegir qué abrir según lo que se escriba
            string ruta = "";

            switch (programa.ToLower())
            {

                case "server":
                    ruta = "RunServer"; 
                    break;
                
                case "ip":
                    ruta = "RunIP";
                    break;
                
                default:
                    await ReplyAsync("🤷‍♂️ No conozco ese programa. Prueba con '!ejecutar server'");
                    return;
            }

            await ReplyAsync($"🎮 Intentando abrir {programa}...");
            _pcController.AbrirPrograma(ruta);
        }

        [Command("estado")]
        [Summary("Verifica estado. Uso: !estado pc | !estado server | !estado playit")]
        public async Task Estado([Remainder] string objetivo = "pc")
        {
            bool estaOn = false;

            switch (objetivo.ToLower())
            {
                case "pc":
                    await ReplyAsync("📡 Pingueando PC Gamer...");
                    estaOn = await _pcController.EstaPrendida();
                    if (estaOn) await ReplyAsync("🟢 **PC ONLINE**");
                    else await ReplyAsync("🔴 **PC OFFLINE**");
                    break;

                case "server":
                    // Aca uso la nueva función buscando java.exe
                    estaOn = _pcController.EstaProcesoCorriendo("java.exe");
                    if (estaOn) await ReplyAsync("⛏️ **Minecraft Server: ONLINE**");
                    else await ReplyAsync("❌ **Minecraft Server: APAGADO**");
                    break;

                case "playit":
                    string nombreExe = "playit.exe"; 
                    
                    estaOn = _pcController.EstaProcesoCorriendo(nombreExe);
                    
                    if (estaOn) await ReplyAsync("🌐 **Túnel Playit.gg: ONLINE** (IP pública activa)");
                    else await ReplyAsync("🔌 **Túnel Playit.gg: APAGADO** (Nadie puede entrar)");
                    break;

                default:
                    await ReplyAsync("❓ Opción no válida. Usa: pc, server, o playit.");
                    break;
            }
        }
    }
}