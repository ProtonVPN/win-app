/*
 * Copyright (c) 2023 Proton AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using ProtonVPN.Common.Networking;
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
            string newValue = GetSettingValueSerialized();
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

        public virtual bool Changed(VpnProtocol vpnProtocol)
        {
            bool childChanged = false;

            foreach (Setting child in GetChildren())
            {
                if (child.Changed(vpnProtocol))
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
            object val = _appSettings.GetType().GetProperty(Name)?.GetValue(_appSettings, null);
            return JsonConvert.SerializeObject(val);
        }

        private void ClearState()
        {
            _reverted = false;
        }
    }
}