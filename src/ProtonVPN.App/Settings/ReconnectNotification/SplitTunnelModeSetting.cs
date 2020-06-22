using System;
using System.Collections.Generic;
using ProtonVPN.Common;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Settings.ReconnectNotification
{
    public class SplitTunnelModeSetting : CompoundSetting
    {
        private readonly IAppSettings _appSettings;

        private readonly List<Setting> _includeModeSettings = new List<Setting>();
        private readonly List<Setting> _excludeModeSettings = new List<Setting>();

        public SplitTunnelModeSetting(string name, Setting parent, IAppSettings appSettings) : base(name, parent, appSettings)
        {
            _appSettings = appSettings;
            _includeModeSettings.Add(new SingleSetting(nameof(IAppSettings.SplitTunnelIncludeIps), this, _appSettings));
            _includeModeSettings.Add(new SingleSetting(nameof(IAppSettings.SplitTunnelingAllowApps), this, _appSettings));

            _excludeModeSettings.Add(new SingleSetting(nameof(IAppSettings.SplitTunnelExcludeIps), this, _appSettings));
            _excludeModeSettings.Add(new SingleSetting(nameof(IAppSettings.SplitTunnelingBlockApps), this, _appSettings));
        }

        public override void Add(Setting s) => throw new NotImplementedException();

        public override List<Setting> GetChildren()
        {
            if (_appSettings.SplitTunnelMode == SplitTunnelMode.Permit)
            {
                return _includeModeSettings;
            }

            return _excludeModeSettings;
        }
    }
}
