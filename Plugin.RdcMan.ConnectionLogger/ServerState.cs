using System;
using System.Collections.Generic;

namespace RdcPlgTest
{
    public class ServerState
    {
        public string RemoteAddress { get; set; }
        public string ConnectedUser { get; set; }
        public DateTime? LastUserConnected { get; set; }
        public bool LastUserIsMe { get; set; }
        public List<ServerActivity> Activity { get; } = new List<ServerActivity>();
    }
}