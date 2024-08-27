using Newtonsoft.Json;

namespace ProtonVPN.Core.Servers.Contracts
{
    public class GuestHoleServerContract
    {
        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("publicKey")]
        public string X25519PublicKey { get; set; }
    }
}