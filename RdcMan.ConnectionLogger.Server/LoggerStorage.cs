namespace RdcMan.ConnectionLogger.Server;

public static class LoggerStorage
{
    private static readonly List<LoggerEntry> _entries = new();

    public static void AddEntry(LoggerEntry entry)
    {
        entry.Date = DateTime.UtcNow;

        _entries.Add(entry);
    }

    public static IEnumerable<LoggerEntry> GetEntries() => _entries.Reverse<LoggerEntry>();
}