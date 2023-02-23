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
using System.Linq;
using Microsoft.Win32;

namespace ProtonVPN.Core.OS
{
    public class SystemState : ISystemState
    {
        private readonly List<string> _keysExist = new()
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootInProgress",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\PackagesPending",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\PostRebootReporting",
        };

        private readonly Dictionary<string, List<string>> _valuesEqual = new()
        {
            {@"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce", new List<string> {"DVDRebootSignal"}},
        };

        public bool PendingReboot()
        {
            if (_keysExist.Any(key => Registry.LocalMachine.OpenSubKey(key) != null))
            {
                return true;
            }

            if ((from item in _valuesEqual
                let key = Registry.LocalMachine.OpenSubKey(item.Key)
                let val = (string)key?.GetValue("")
                where val != null && item.Value.Contains(val)
                select item).Any())
            {
                return true;
            }

            return PendingUpdates();
        }

        private bool PendingUpdates()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Updates");
            object val = key?.GetValue("UpdateExeVolatile");
            if (val != null)
            {
                int.TryParse((string)val, out int result);
                if (result != 0)
                {
                    return true;
                }
            }

            key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Services\Pending");

            return key?.SubKeyCount > 0;
        }
    }
}