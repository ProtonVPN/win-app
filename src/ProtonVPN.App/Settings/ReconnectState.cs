using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Settings
{
    public class ReconnectState : IVpnStateAware
    {
        private readonly Dictionary<string, string> _initialSettings = new Dictionary<string, string>();
        private VpnStatus _vpnStatus;
        private readonly IAppSettings _appSettings;

        public ReconnectState(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        private readonly List<string> _reconnectRequiredSettings = new List<string>
        {
            nameof(IAppSettings.SplitTunnelingEnabled),
            nameof(IAppSettings.SplitTunnelMode),
            nameof(IAppSettings.SplitTunnelingAllowApps),
            nameof(IAppSettings.SplitTunnelingBlockApps),
            nameof(IAppSettings.SplitTunnelIncludeIps),
            nameof(IAppSettings.SplitTunnelExcludeIps),
            nameof(IAppSettings.KillSwitch),
            nameof(IAppSettings.OvpnProtocol),
            nameof(IAppSettings.CustomDnsEnabled),
            nameof(IAppSettings.CustomDnsIps),
            nameof(IAppSettings.NetShieldEnabled),
            nameof(IAppSettings.NetShieldMode),
        };

        public bool Required()
        {
            if (_vpnStatus == VpnStatus.Disconnecting ||
                _vpnStatus == VpnStatus.Disconnected)
            {
                return false;
            }

            return _reconnectRequiredSettings.Any(setting =>
                _initialSettings.ContainsKey(setting) &&
                GetSettingValueSerialized(setting) != _initialSettings[setting]);
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnStatus = e.State.Status;

            if (_vpnStatus == VpnStatus.Connected)
            {
                SaveInitialSettings();
            }

            return Task.CompletedTask;
        }

        private void SaveInitialSettings()
        {
            _initialSettings.Clear();

            foreach (var setting in _reconnectRequiredSettings)
            {
                _initialSettings.Add(setting, GetSettingValueSerialized(setting));
            }
        }

        private string GetSettingValueSerialized(string setting)
        {
            var val = _appSettings.GetType().GetProperty(setting)?.GetValue(_appSettings, null);
            return JsonConvert.SerializeObject(val);
        }
    }
}
