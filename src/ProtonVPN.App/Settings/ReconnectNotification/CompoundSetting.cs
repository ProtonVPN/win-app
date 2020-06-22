using System.Collections.Generic;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Settings.ReconnectNotification
{
    public class CompoundSetting : Setting
    {
        private readonly List<Setting> _settings = new List<Setting>();

        public CompoundSetting(string name, Setting parent, IAppSettings settings) : base(name, parent, settings)
        {
        }

        public override void Add(Setting s)
        {
            _settings.Add(s);
        }

        public override List<Setting> GetChildren()
        {
            return _settings;
        }
    }
}
