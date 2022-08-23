using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using MemoryStream = System.IO.MemoryStream;

namespace RdcPlgTest
{
    public static class LoggerClient
    {
        private static HttpClient _client = new HttpClient();

        public static void Initialize(string server)
        {
            _client = new HttpClient { Timeout = TimeSpan.FromSeconds(15), BaseAddress = new Uri(server) };
        }

        public static bool IsConfigured => _client.BaseAddress != null;

        public static async Task<LoggerEntry[]> GetLog()
        {
            
            var result = await _client.GetAsync("/api/logger");

            result.EnsureSuccessStatusCode();
            
            var serializer = new DataContractJsonSerializer(typeof(LoggerEntryLngDate[]));
            
            if (!(serializer.ReadObject(await result.Content.ReadAsStreamAsync()) is LoggerEntryLngDate[] entries))
            {
                throw new InvalidDataException("Serialization failure");
            }

            return entries.Select(x => new LoggerEntry
            {
                Action = x.Action,
                Date = x.Date == null ? null : (DateTime?)DateTimeOffset.FromUnixTimeSeconds(x.Date.Value).DateTime.ToLocalTime(),
                RemoteAddress = x.RemoteAddress,
                RemoteName = x.RemoteName,
                UserName = x.UserName
            }).ToArray();
        }

        public static async Task<bool> GetAvailable()
        {
            if (!IsConfigured)
            {
                return false;
            }

            try
            {
                var result = await _client.GetAsync("/api/logger");

                return result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static async void Send(LoggerEntry entry)
        {
            if (!IsConfigured)
            {
                return;
            }

            using (var memoryStream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(LoggerEntry));
                serializer.WriteObject(memoryStream, entry);

                memoryStream.Seek(0, SeekOrigin.Begin);

                var str = Encoding.UTF8.GetString(memoryStream.ToArray());
                var content = new StringContent(str, Encoding.UTF8, "application/json");
                            
                try
                {
                    var resp = await _client.PostAsync("/api/logger", content);
                }
                catch
                {
                    // void
                }
            }
        }
    }
}
