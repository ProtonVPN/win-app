using System.Collections.Generic;
using Newtonsoft.Json;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Settings.ReconnectNotification
{
    public abstract class Setting
    {
        private bool _reverted;
        public string Name;
        public string Value;
        private readonly IAppSettings _appSettings;

        protected Setting(string name, Setting parent, IAppSettings appSettings)
        {
            _appSettings = appSettings;
            Name = name;
            Parent = parent;
            Value = GetSettingValueSerialized();
        }

        public Setting Parent { get; }

        public abstract void Add(Setting s);

        public void SetChangesReverted()
        {
            var newValue = GetSettingValueSerialized();
            if (_appSettings.GetType().GetProperty(Name)?.PropertyType == typeof(bool))
            {
                if (newValue == "true" && !_reverted)
                {
                    return;
                }
            }

            _reverted = Value == newValue;
            ResetParentChanges(Parent);
        }

        private void ResetParentChanges(Setting setting)
        {
            if (setting != null)
            {
                setting.ClearState();
                ResetParentChanges(setting.Parent);
            }
        }

        public abstract List<Setting> GetChildren();

        public virtual bool Changed()
        {
            var childChanged = false;

            foreach (var child in GetChildren())
            {
                if (child.Changed())
                {
                    childChanged = true;
                    break;
                }
            }

            if (childChanged)
            {
                if (_appSettings.GetType().GetProperty(Name)?.PropertyType == typeof(bool))
                {
                    return !_reverted;
                }

                return true;
            }

            return Value != GetSettingValueSerialized();
        }

        public string GetSettingValueSerialized()
        {
            var val = _appSettings.GetType().GetProperty(Name)?.GetValue(_appSettings, null);
            return JsonConvert.SerializeObject(val);
        }

        private void ClearState()
        {
            _reverted = false;
        }
    }
}
