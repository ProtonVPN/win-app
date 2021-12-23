using System;
using System.Net.Http;
using System.Threading.Tasks;
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
            return await GetConnectionInfo("ip");
        }

        public async Task<string> GetCountry()
        {
            return await GetConnectionInfo("country");
        }

        private async Task<string> GetConnectionInfo(string jsonKey)
        {
            HttpResponseMessage response = await _client.GetAsync("/json/");
            response.EnsureSuccessStatusCode();
            string responseBody = response.Content.ReadAsStringAsync().Result;
            JObject json = JObject.Parse(responseBody);
            return json[jsonKey].ToString();
        }
    }
}
