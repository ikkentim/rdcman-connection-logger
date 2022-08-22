using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace RdcPlgTest
{
    [DataContract]
    public class LoggerEntry
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
        public DateTime? Date { get; set; }

        protected bool Equals(LoggerEntry other) => UserName == other.UserName && RemoteName == other.RemoteName && Action == other.Action && RemoteAddress == other.RemoteAddress && Nullable.Equals(Date, other.Date);

        public override bool Equals(object obj) =>
            !ReferenceEquals(null, obj) &&
            (ReferenceEquals(this, obj) || (obj.GetType() == GetType() && Equals((LoggerEntry)obj)));
        
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (UserName != null ? UserName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RemoteName != null ? RemoteName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Action != null ? Action.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RemoteAddress != null ? RemoteAddress.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Date.GetHashCode();
                return hashCode;
            }
        }
    }
}
