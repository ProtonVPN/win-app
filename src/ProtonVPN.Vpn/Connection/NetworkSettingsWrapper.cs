using System;
using System.Collections.Generic;
using ProtonVPN.Common;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Os.Net;
using ProtonVPN.Common.OS.Net.NetworkInterface;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Vpn.Connection
{
    internal class NetworkSettingsWrapper : IVpnConnection
    {
        private readonly IVpnConnection _origin;
        private readonly INetworkInterfaces _networkInterfaces;
        private readonly string _tapAdapterDescription;
        private readonly string _tapAdapterId;
        private readonly ILogger _logger;

        public NetworkSettingsWrapper(
            ILogger logger,
            string tapAdapterId,
            string tapAdapterDescription,
            INetworkInterfaces networkInterfaces,
            IVpnConnection origin)
        {
            _logger = logger;
            _tapAdapterDescription = tapAdapterDescription;
            _tapAdapterId = tapAdapterId;
            _networkInterfaces = networkInterfaces;
            _origin = origin;
            _origin.StateChanged += Origin_StateChanged;
        }

        public event EventHandler<EventArgs<VpnState>> StateChanged;
        public InOutBytes Total => _origin.Total;

        public void Connect(IReadOnlyList<VpnHost> servers, VpnConfig config, VpnProtocol protocol,
            VpnCredentials credentials)
        {
            ApplyNetworkSettings();

            _origin.Connect(servers, config, protocol, credentials);
        }

        public void Disconnect(VpnError error = VpnError.None)
        {
            _origin.Disconnect(error);
            RestoreNetworkSettings();
        }

        public void UpdateServers(IReadOnlyList<VpnHost> servers, VpnConfig config)
        {
            _origin.UpdateServers(servers, config);
        }

        private void Origin_StateChanged(object sender, EventArgs<VpnState> e)
        {
            OnStateChanged(e.Data);
        }

        private void OnStateChanged(VpnState state)
        {
            StateChanged?.Invoke(this, new EventArgs<VpnState>(state));
        }

        private void ApplyNetworkSettings()
        {
            var tapGuid = GetTapGuid();
            if (tapGuid == null)
            {
                return;
            }

            try
            {
                var localInterfaceIp = NetworkUtil.GetBestInterfaceIp(_tapAdapterId).ToString();

                NetworkUtil.DeleteDefaultGatewayForIface(tapGuid.Value, localInterfaceIp);
                NetworkUtil.AddDefaultGatewayForIface(tapGuid.Value, localInterfaceIp);
                NetworkUtil.SetLowestTapMetric(tapGuid.Value);
            }
            catch (NetworkUtilException e)
            {
                _logger.Error("Failed to apply network settings. Error code: " + e.Code);
            }
        }

        private void RestoreNetworkSettings()
        {
            var tapGuid = GetTapGuid();
            if (tapGuid == null)
            {
                return;
            }

            try
            {
                NetworkUtil.RestoreDefaultTapMetric(tapGuid.Value);
            }
            catch (NetworkUtilException e)
            {
                _logger.Error("Failed restore network settings. Error code: " + e.Code);
            }
        }

        private Guid? GetTapGuid()
        {
            var tapInterface = _networkInterfaces.Interface(_tapAdapterDescription);
            var parseResult = Guid.TryParse(tapInterface.Id, out var guid);

            if (!parseResult)
            {
                return null;
            }

            return guid;
        }
    }
}
