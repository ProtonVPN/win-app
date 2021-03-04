using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.OS.Net.NetworkInterface;

namespace ProtonVPN.Common.OS.Net
{
    public class NetworkInterfaceLoader : INetworkInterfaceLoader
    {
        private readonly Config _config;
        private readonly INetworkInterfaces _networkInterfaces;

        public NetworkInterfaceLoader(Config config, INetworkInterfaces networkInterfaces)
        {
            _networkInterfaces = networkInterfaces;
            _config = config;
        }

        public INetworkInterface GetTapInterface()
        {
            return _networkInterfaces.GetByDescription(_config.OpenVpn.TapAdapterDescription);
        }

        public INetworkInterface GetTunInterface()
        {
            return _networkInterfaces.GetByName(_config.OpenVpn.TunAdapterName);
        }
    }
}