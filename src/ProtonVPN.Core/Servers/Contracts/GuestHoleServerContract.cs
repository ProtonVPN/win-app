using Newtonsoft.Json;

namespace ProtonVPN.Core.Servers.Contracts
{
    public class GuestHoleServerContract
    {
        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }
    }
}