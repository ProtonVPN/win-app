using System.Collections.Generic;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Settings.ReconnectNotification
{
    public class CustomDnsSetting : CompoundSetting
    {
        private readonly SingleSetting _customDnsIpSetting;
        private readonly IAppSettings _appSettings;

        public CustomDnsSetting(string name, Setting parent, IAppSettings appSettings) : base(name, parent, appSettings)
        {
            _appSettings = appSettings;
            _customDnsIpSetting = new SingleSetting(nameof(IAppSettings.CustomDnsIps), this, appSettings);
        }

        public override List<Setting> GetChildren()
        {
            return new List<Setting> {_customDnsIpSetting};
        }

        public override bool Changed()
        {
            var changed = base.Changed();

            if (_customDnsIpSetting.GetSettingValueSerialized() != _customDnsIpSetting.Value)
            {
                if (_appSettings.CustomDnsIps.Length == 0)
                {
                    return true;
                }
            }

            return _appSettings.CustomDnsIps.Length != 0 && changed;
        }
    }
}
