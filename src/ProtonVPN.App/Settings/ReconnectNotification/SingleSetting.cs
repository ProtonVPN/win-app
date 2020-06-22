using System.Collections.Generic;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Settings.ReconnectNotification
{
    public class SingleSetting : Setting
    {
        public SingleSetting(string name, Setting parent, IAppSettings settings) : base(name, parent, settings)
        {
        }

        public override void Add(Setting s)
        {
            throw new System.NotImplementedException();
        }

        public override List<Setting> GetChildren()
        {
            return new List<Setting>();
        }
    }
}
