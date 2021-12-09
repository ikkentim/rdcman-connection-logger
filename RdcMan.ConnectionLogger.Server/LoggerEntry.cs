namespace RdcMan.ConnectionLogger.Server;

public class LoggerEntry
{
    public string? UserName { get; set; }
    public string? RemoteName { get; set; }
    public string? Action { get; set; }
    public string? RemoteAddress { get; set; }
    public DateTime Date { get; set; }
}