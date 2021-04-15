using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ProtonVPN.UI.Test.ApiClient
{
    class CommonAPI
    {
        private readonly HttpClient _client;

        public CommonAPI(string baseAddress)
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };
        }

        public async Task<string> GetIpAddress()
        {
            return await GetConnectionInfo("query");
        }

        public async Task<string> GetCountry()
        {
            return await GetConnectionInfo("country");
        }

        private async Task<string> GetConnectionInfo(string jsonKey)
        {
            string result;

            try
            {
                HttpResponseMessage response = await _client.GetAsync("/json");
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                JObject json = JObject.Parse(responseBody);
                result = json[jsonKey].ToString();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("\nFailed to get connection info!");
                Console.WriteLine(ex.Message);
                result = string.Empty;
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine("\nFailed to parse JSON file!");
                Console.WriteLine(ex.Message);
                result = string.Empty;
            }

            return result;
        }
    }
}
