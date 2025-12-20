public class BotConfig
{
    public string? Token { get; set; }
    public PcConfigData? PcConfig { get; set; }
    }

public class PcConfigData
{
    public string? Ip { get; set; }
    public string? Mac { get; set; }
    public string? User { get; set; }
    public string? Pass { get; set; }
}