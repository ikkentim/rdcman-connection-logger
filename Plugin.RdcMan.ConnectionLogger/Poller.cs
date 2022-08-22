using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RdcMan;

namespace RdcPlgTest
{
    public static class Poller
    {
        private static readonly Dictionary<string, ServerState> _servers =
            new Dictionary<string, ServerState>(StringComparer.InvariantCultureIgnoreCase);

        private static bool _pollingRunning;
        private static DateTime? _lastProcessed;
        private static readonly SemaphoreSlim _pollLock = new SemaphoreSlim(1, 1);

        public static void StartPoller()
        {
            if (!_pollingRunning)
            {
                _pollingRunning = true;
                _ = Task.Run(PollingLoop);
            }
        }

        public static async Task PollNow()
        {
            try
            {
                if (_pollLock.CurrentCount == 0)
                {
                    return;
                }

                await _pollLock.WaitAsync();
                await Poll();
            }
            finally
            {
                _pollLock.Release();
            }
        }

        private static async Task PollingLoop()
        {
            for (;;)
            {
                if (LoggerClient.IsConfigured)
                {
                    try
                    {
                        await PollNow();
                    }
                    catch
                    {
                        // void
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        }

        public static ServerState GetServerState(string remoteAddress) =>
            _servers.TryGetValue(remoteAddress, out var state)
                ? state
                : _servers[remoteAddress] = new ServerState { RemoteAddress = remoteAddress };

        public static event EventHandler<ServerState> ServerStateChanged;
        private static async Task Poll()
        {
            var entries = await LoggerClient.GetLog();
            
            foreach (var entry in entries
                         .Where(x => x.RemoteAddress != null && x.Date != null)
                         .Where(x => _lastProcessed == null || x.Date >= _lastProcessed)
                         .OrderBy(x => x.Date))
            {
                var state = GetServerState(entry.RemoteAddress);

                if (!ContainsActivity(state, entry))
                {
                    state.Activity.Add(new ServerActivity
                    {
                        Action = entry.Action, 
                        Date = entry.Date.Value, 
                        UserName = entry.UserName
                    });

                    _lastProcessed = entry.Date.Value;
                    
                    // TODO: batching
                    var change = false;
                    switch (entry.Action)
                    {
                        case nameof(RdpClient.ConnectionState.Connected):
                            state.ConnectedUser = entry.UserName;
                            state.LastUserConnected = entry.Date;
                            state.LastUserIsMe = entry.UserName == Environment.UserName;
                            change = true;
                            break;
                        case nameof(RdpClient.ConnectionState.Disconnected):
                            state.ConnectedUser = null;
                            state.LastUserConnected = null;
                            change = true;
                            break;
                    }
                    
                    try
                    {
                        if (change)
                        {
                            OnServerStateChanged(state);
                        }
                    }
                    catch
                    {
                        // void
                    }
                }
            }
        }

        private static bool ContainsActivity(ServerState state, LoggerEntry entry) =>
            state.Activity.Any(x =>
                x.Date == entry.Date && x.UserName == entry.UserName && x.Action == entry.Action);

        private static void OnServerStateChanged(ServerState e)
        {
            ServerStateChanged?.Invoke(null, e);
        }
    }
}