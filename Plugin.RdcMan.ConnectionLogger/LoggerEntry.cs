using System.Runtime.Serialization;

namespace RdcPlgTest
{
    [DataContract]
    public class LoggerEntry
    {
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string RemoteName { get; set; }
        [DataMember]
        public string Action { get; set; }
        [DataMember]
        public string RemoteAddress { get; set;}
    }
}
