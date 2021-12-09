using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace RdcPlgTest
{
    public static class LoggerClient
    {
		private static HttpClient _client = new HttpClient();

		public static void Initialize(string server)
        {
            _client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15),
                BaseAddress = new Uri(server)
            };
        }

		public static async Task<bool> GetAvailable()
        {
            if (_client.BaseAddress == null)
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
            if (_client.BaseAddress == null)
            {
				return;
            }

			using (var memoryStream = new MemoryStream())
			{
				var x = new DataContractJsonSerializer(typeof(LoggerEntry));
				x.WriteObject(memoryStream, entry);

                memoryStream.Seek(0, SeekOrigin.Begin);

                var str = Encoding.UTF8.GetString(memoryStream.ToArray());
				var content = new StringContent(str, Encoding.UTF8, "application/json");
                			
				try
				{
					var result = await _client.PostAsync("/api/logger", content);
				}
                catch
                {
					// void
                }
			}
        }
    }
}
