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
        [Summary("Prende la PC mediante Wake on LAN.")]
        [RequireRole("Admin-MC")] 

        public async Task Encender()
        {
            await ReplyAsync("💀 Reviviendo la PC");
            await ReplyAsync("Espera 1 o 2 minutos para prender el server gordito");
            await _pcController.EncenderPc();
        }

        // COMANDO: APAGAR
        [Command("pcoff")]
        [Summary("Apaga la PC.")]
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
                    await ReplyAsync("En aproximadamente 2 o 3 minutos vas a poder entrar a viciar gordito");
                    break;
                
                case "ip":
                    ruta = "RunIP";
                    break;
                
                default:
                    await ReplyAsync("No se que decis tontito. Intenta con '!ejecutar server' o '!ejecutar ip'");
                    return;
            }

            await ReplyAsync($"🎮 Intentando abrir {programa}...");
            _pcController.AbrirPrograma(ruta);
        }

        [Command("estado")]
        [Summary("Verifica estado. Uso: !estado pc | !estado server | !estado ip")]
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

                case "ip":
                    string nombreExe = "playit.exe"; 
                    
                    estaOn = _pcController.EstaProcesoCorriendo(nombreExe);
                    
                    if (estaOn) await ReplyAsync("🌐 **Túnel Playit.gg: ONLINE** (IP pública activa)");
                    else await ReplyAsync("🔌 **Túnel Playit.gg: APAGADO** (Nadie puede entrar)");
                    break;

                default:
                    await ReplyAsync("❓ No se q decis bobi, proba con: pc, server, o playit.");
                    break;
            }
        }

        [Command("kill")]
        [Summary("Cierra programas a la fuerza. Uso: !kill server | !kill ip")]
        [RequireRole("Admin-MC")] // ¡Importante para que nadie te apague cosas por error!
        public async Task Kill([Remainder] string objetivo)
        {
            string nombreProceso = "";
            string nombreAmigable = "";

            switch (objetivo.ToLower())
            {
                case "server":
                case "minecraft": // Alias extra
                    nombreProceso = "java.exe"; 
                    nombreAmigable = "⛏️ Servidor de Minecraft";
                    break;

                case "ip":
                case "playitgg": // Alias extra
                    // ⚠️ ASEGURATE que en tu Administrador de Tareas se llame así.
                    // A veces es "playit-amd64.exe" o similar.
                    nombreProceso = "playit.exe"; 
                    nombreAmigable = "🌐 Túnel Playit.gg";
                    break;

                default:
                    await ReplyAsync("⚠️ Objetivo no reconocido. Intenta: `!kill server` o `!kill ip`");
                    return;
            }

            await ReplyAsync($"🔫 Apuntando a: **{nombreAmigable}** ({nombreProceso})...");

            try 
            {
                // Llamamos a tu función genérica que ya creaste antes
                _pcController.MatarProceso(nombreProceso);
                
                // Esperamos un segundo y confirmamos (Opcional)
                await Task.Delay(1000); 
                await ReplyAsync($"💀 **{nombreAmigable}** fue sido eliminado.");
            }
            catch (Exception ex)
            {
                await ReplyAsync($"❌ Error al intentar matar el proceso: {ex.Message}");
            }
        }
    }
}