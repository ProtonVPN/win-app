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

using System.Security;
using Microsoft.Win32;
using ProtonVPN.Common.Events;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.OperatingSystemLogs;

namespace ProtonVPN.Service.Vpn
{
    /**
     * Sometimes ComponentId is missing in the registry and OpenVPN won't find TUN adapter.
     * This class checks if the key is missing and creates one.
     */
    public class WintunRegistryFixer
    {
        // https://docs.microsoft.com/en-us/windows-hardware/drivers/install/system-defined-device-setup-classes-available-to-vendors
        private readonly string _regPath = "SYSTEM\\ControlSet001\\Control\\Class\\{4d36e972-e325-11ce-bfc1-08002be10318}";
        private readonly ILogger _logger;
        private readonly IEventPublisher _eventPublisher;

        public WintunRegistryFixer(ILogger logger, IEventPublisher eventPublisher)
        {
            _logger = logger;
            _eventPublisher = eventPublisher;
        }

        public void EnsureTunAdapterRegistryIsCorrect()
        {
            try
            {
                using RegistryKey adaptersKey = Registry.LocalMachine.OpenSubKey(_regPath);
                if (adaptersKey == null)
                {
                    return;
                }

                string[] folders = adaptersKey.GetSubKeyNames();
                foreach (string folder in folders)
                {
                    if (folder.Length != 4)
                    {
                        continue;
                    }

                    RegistryKey adapterKey = Registry.LocalMachine.OpenSubKey($"{_regPath}\\{folder}");
                    if (adapterKey == null)
                    {
                        continue;
                    }

                    string matchingDeviceId = (string)adapterKey.GetValue("MatchingDeviceId");
                    if (matchingDeviceId != "wintun")
                    {
                        continue;
                    }

                    string componentId = (string)adapterKey.GetValue("ComponentId");
                    if (componentId.IsNullOrEmpty())
                    {
                        Registry.SetValue($"HKEY_LOCAL_MACHINE\\{_regPath}\\{folder}", "ComponentId", matchingDeviceId);
                        _eventPublisher.CaptureMessage("Fixed missing ComponentId on wintun adapter.");
                    }
                    else if (matchingDeviceId != componentId)
                    {
                        _logger.Info<OperatingSystemLog>($"WintunRegistryFixer: ComponentId '{componentId}' " +
                            $"has a value but is different from the MatchingDeviceId '{matchingDeviceId}'.");
                    }
                }
            }
            catch (SecurityException)
            {
            }
        }
    }
}