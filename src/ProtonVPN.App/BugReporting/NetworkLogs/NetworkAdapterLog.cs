using System.IO;
using ProtonVPN.Common.OS.Net.NetworkInterface;

namespace ProtonVPN.BugReporting.NetworkLogs
{
    internal class NetworkAdapterLog : BaseLog
    {
        private readonly INetworkInterfaces _networkInterfaces;

        public NetworkAdapterLog(INetworkInterfaces networkInterfaces, Common.Configuration.Config config) : base(
            config.AppLogFolder, "NetworkAdapters.txt")
        {
            _networkInterfaces = networkInterfaces;
        }

        public override void Write()
        {
            File.WriteAllText(Path, Content);
        }

        private string Content
        {
            get
            {
                var str = string.Empty;
                var interfaces = _networkInterfaces.Interfaces();
                foreach (var networkInterface in interfaces)
                {
                    str += GetInterfaceDetails(networkInterface);
                }

                return str;
            }
        }

        private string GetInterfaceDetails(INetworkInterface networkInterface)
        {
            var active = networkInterface.IsActive ? "true" : "false";

            return $"Name: {networkInterface.Name}\n" +
                   $"Description: {networkInterface.Description}\n" +
                   $"Active: {active}\n\n";
        }
    }
}
