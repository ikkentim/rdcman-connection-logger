namespace RdcMan.ConnectionLogger.Server
{
    public static class LoggerStorage
    {
        private static List<LoggerEntry> _entries = new List<LoggerEntry>();

        public static void AddEntry(LoggerEntry entry)
        {
            entry.Date = DateTime.Now;

            _entries.Add(entry);
        }

        public static IEnumerable<LoggerEntry> GetEntries()
        {
            return _entries.Reverse<LoggerEntry>();
        }
    }
}
