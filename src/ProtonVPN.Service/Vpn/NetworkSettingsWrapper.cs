using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Os.Net;
using ProtonVPN.Common.OS.Net.NetworkInterface;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Service.Vpn
{
    public class NetworkSettings : IVpnStateAware
    {
        private readonly INetworkInterfaces _networkInterfaces;
        private readonly ILogger _logger;
        private readonly Common.Configuration.Config _config;

        public NetworkSettings(
            ILogger logger,
            INetworkInterfaces networkInterfaces,
            Common.Configuration.Config config)
        {
            _config = config;
            _logger = logger;
            _networkInterfaces = networkInterfaces;
        }

        public bool ApplyNetworkSettings()
        {
            var tapInterfaceIndex =
                _networkInterfaces.InterfaceIndex(_config.OpenVpn.TapAdapterDescription, _config.OpenVpn.TapAdapterId);
            if (tapInterfaceIndex == 0)
            {
                return false;
            }

            try
            {
                var localInterfaceIp = NetworkUtil.GetBestInterfaceIp(_config.OpenVpn.TapAdapterId).ToString();

                NetworkUtil.DeleteDefaultGatewayForIface(tapInterfaceIndex, localInterfaceIp);
                NetworkUtil.AddDefaultGatewayForIface(tapInterfaceIndex, localInterfaceIp);
                NetworkUtil.SetLowestTapMetric(tapInterfaceIndex);
            }
            catch (NetworkUtilException e)
            {
                _logger.Error("Failed to apply network settings. Error code: " + e.Code);
                return false;
            }

            return true;
        }

        private void RestoreNetworkSettings()
        {
            var tapInterfaceIndex =
                _networkInterfaces.InterfaceIndex(_config.OpenVpn.TapAdapterDescription, _config.OpenVpn.TapAdapterId);
            if (tapInterfaceIndex == 0)
            {
                return;
            }

            try
            {
                NetworkUtil.RestoreDefaultTapMetric(tapInterfaceIndex);
            }
            catch (NetworkUtilException e)
            {
                _logger.Error("Failed restore network settings. Error code: " + e.Code);
            }
        }

        public void OnVpnDisconnected(VpnState state)
        {
            RestoreNetworkSettings();
        }

        public void OnVpnConnected(VpnState state)
        {
        }

        public void OnVpnConnecting(VpnState state)
        {
        }
    }
}