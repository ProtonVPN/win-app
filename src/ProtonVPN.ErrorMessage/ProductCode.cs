/*
 * Copyright (c) 2022 Proton Technologies AG
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

using System;
using Microsoft.Win32;

namespace ProtonVPN.ErrorMessage
{
    internal class ProductCode
    {
        private readonly string _name;
        private string _value = string.Empty;

        public ProductCode(string name)
        {
            _name = name;
        }

        public string Value()
        {
            if (!string.IsNullOrEmpty(_value))
            {
                return _value;
            }

            _value = GetValueFromRegistry();

            return _value;
        }

        private string GetValueFromRegistry()
        {
            var val = string.Empty;

            try
            {
                var path = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Installer\\UserData\\S-1-5-18\\Products";
                var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(path);
                if (key == null)
                {
                    return val;
                }

                foreach (var tempKeyName in key.GetSubKeyNames())
                {
                    var tempKey = key.OpenSubKey(tempKeyName + "\\InstallProperties");
                    if (tempKey == null || !string.Equals(Convert.ToString(tempKey.GetValue("DisplayName")), _name))
                    {
                        continue;
                    }

                    val = Convert.ToString(tempKey.GetValue("UninstallString"));
                    val = val.Replace("/I", "");
                    val = val.Replace("/X", "");
                    val = val.Replace("MsiExec.exe", "").Trim();
                    break;
                }

                return val;
            }
            catch (Exception)
            {
                return val;
            }
        }
    }
}
