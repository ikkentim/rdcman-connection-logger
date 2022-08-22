using System.Runtime.Serialization;

namespace RdcPlgTest
{
    [DataContract]
    public class LoggerEntryLngDate
    {
        [DataMember(Name = "userName")]
        public string UserName { get; set; }
        [DataMember(Name = "remoteName")]
        public string RemoteName { get; set; }
        [DataMember(Name = "action")]
        public string Action { get; set; }
        [DataMember(Name = "remoteAddress")]
        public string RemoteAddress { get; set; }
        /// read-only in API.
        [DataMember(Name = "date", EmitDefaultValue = false)]
        public long? Date { get; set; }
    }
}