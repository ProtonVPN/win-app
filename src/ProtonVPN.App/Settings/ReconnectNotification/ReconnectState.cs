using System.Collections.Generic;
using System.Threading.Tasks;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Settings.ReconnectNotification
{
    public class ReconnectState : IVpnStateAware
    {
        private VpnStatus _vpnStatus;
        private readonly SettingsBuilder _settingsBuilder;
        private List<Setting> _reconnectRequiredSettings = new List<Setting>();

        public ReconnectState(SettingsBuilder settingsBuilder)
        {
            _settingsBuilder = settingsBuilder;
        }

        public bool Required(string settingChanged)
        {
            if (_vpnStatus == VpnStatus.Disconnecting ||
                _vpnStatus == VpnStatus.Disconnected)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(settingChanged))
            {
                OnSettingChanged(settingChanged);
            }

            foreach (var setting in _reconnectRequiredSettings)
            {
                if (setting.Changed())
                {
                    return true;
                }
            }

            return false;
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _vpnStatus = e.State.Status;

            if (_vpnStatus == VpnStatus.Connected)
            {
                _reconnectRequiredSettings = _settingsBuilder.Build();
            }

            return Task.CompletedTask;
        }

        private void RevertChanges(Setting setting, string name)
        {
            if (setting.Name == name)
            {
                setting.SetChangesReverted();
                return;
            }

            foreach (var s in setting.GetChildren())
            {
                if (s.Name == name)
                {
                    s.SetChangesReverted();
                    return;
                }

                RevertChanges(s, name);
            }
        }

        private void OnSettingChanged(string settingName)
        {
            foreach (var setting in _reconnectRequiredSettings)
            {
                RevertChanges(setting, settingName);
            }
        }
    }
}
