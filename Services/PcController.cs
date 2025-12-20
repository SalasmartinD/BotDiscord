using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Renci.SshNet; // La librería que acabamos de instalar
using System.Net.NetworkInformation;
using System.Text.Json;


namespace DiscordBot.Services
{
    public class PcController
    {
        // ==========================================
        // ⚙️ CONFIGURACIÓN
        // ==========================================
        private string PcIp;
        private string PcMac;
        private string SshUser;
        private string SshPass;
        // ==========================================
        public PcController()
        {
            // 1. Leemos el archivo JSON
            string textoJson = File.ReadAllText("appsettings.json");
            
            // 2. Lo convertimos a objeto
            var config = JsonSerializer.Deserialize<BotConfig>(textoJson);

            // 🔴 VALIDACIÓN DE SEGURIDAD (Esto es lo que arregla el error)
            // Le decimos: "Si config es nulo O la parte de PcConfig es nula, lanza error"
            if (config == null || config.PcConfig == null)
            {
                throw new Exception("ERROR CRÍTICO: El archivo appsettings.json está vacío, mal formado o le falta la sección PcConfig.");
            }

            // 3. Cargamos las variables (Ahora usamos el operador '??' por si acaso vienen vacías poner un texto vacío)
            PcIp = config.PcConfig.Ip ?? "";
            PcMac = config.PcConfig.Mac ?? "";
            SshUser = config.PcConfig.User ?? "";
            SshPass = config.PcConfig.Pass ?? "";
        }
        // COMANDO 1: PRENDER (Wake on LAN)
        public async Task EncenderPc()
        {
            // Limpio la MAC para que quede solo en hex
            string macLimpia = PcMac.Replace(":", "").Replace("-", "");
            
            // Convierto la MAC a bytes
            byte[] macBytes = new byte[6];
            for (int i = 0; i < 6; i++)
            {
                macBytes[i] = Convert.ToByte(macLimpia.Substring(i * 2, 2), 16);
            }

            // Creo el Paquete Mágico (6 veces FF + 16 veces la MAC)
            List<byte> packet = new List<byte>();
            for (int i = 0; i < 6; i++) packet.Add(0xFF);
            for (int i = 0; i < 16; i++) packet.AddRange(macBytes);

            // Envio a toda la red (Broadcast)
            using (UdpClient client = new UdpClient())
            {
                client.Connect(IPAddress.Broadcast, 9);
                await client.SendAsync(packet.ToArray(), packet.Count);
            }
        }

        // COMANDO 2: APAGAR (SSH Shutdown)
        public void ApagarPc()
        {
            using (var client = new SshClient(PcIp, SshUser, SshPass))
            {
                client.Connect();
                // Ejecuto el apagado inmediato
                client.RunCommand("shutdown /s /t 0");
                client.Disconnect();
            }
        }

        // COMANDO 3: ABRIR PROGRAMA (SSH Start)
        public void AbrirPrograma(string nombreTarea)
        {
            using (var client = new SshClient(PcIp, SshUser, SshPass))
            {
                client.Connect();

                var cmd = client.CreateCommand($"schtasks /run /tn \"{nombreTarea}\"");
                
                cmd.Execute(); 
                
                client.Disconnect();
            }
        }

        public async Task<bool> EstaPrendida()
        {
            Ping ping = new Ping();
            try
            {
                // Enviamos un ping y esperamos máximo 2 segundos
                PingReply reply = await ping.SendPingAsync(PcIp, 2000);
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        // CHECK 2: ¿ESTÁ EL SERVER CORRIENDO? (Busca java.exe)
        public bool EstaProcesoCorriendo(string nombreProceso)
        {
            try
            {
                using (var client = new SshClient(PcIp, SshUser, SshPass))
                {
                    client.Connect();
                    
                    // Busco el proceso específico pasado por parámetro
                    // /NH = No Header (para limpiar la salida)
                    var cmd = client.CreateCommand($"tasklist /FI \"IMAGENAME eq {nombreProceso}\" /NH");
                    string resultado = cmd.Execute();
                    
                    client.Disconnect();

                    // Si el resultado contiene el nombre, es que está vivo
                    // Usamos .ToLower() para evitar problemas de mayúsculas
                    return resultado.ToLower().Contains(nombreProceso.ToLower());
                }
            }
            catch
            {
                return false;
            }
        }
    }
}