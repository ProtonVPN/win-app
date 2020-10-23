namespace ProtonVPN.Service.Firewall
{
    public class FirewallParams
    {
        public FirewallParams(string serverIp, bool dnsLeakOnly)
        {
            ServerIp = serverIp;
            DnsLeakOnly = dnsLeakOnly;
        }

        public string ServerIp { get; }

        public bool DnsLeakOnly { get; }
    }
}
