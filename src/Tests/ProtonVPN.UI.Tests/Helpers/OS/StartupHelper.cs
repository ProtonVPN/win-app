/*
 * Copyright (c) 2026 Proton AG
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

using Microsoft.Win32;
using NUnit.Framework;

namespace ProtonVPN.UI.Tests.TestsHelper;

public static class StartupHelper
{
    private const string REGISTRY_PATH = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string REGISTRY_NAME = "Proton VPN";
    private const string REGISTRY_DATA = @"C:\Program Files\Proton\VPN\ProtonVPN.Launcher.exe ""----ms-protocol:ms-encodedlaunch:App?ContractId=Windows.StartupTask&TaskId=Proton%20VPN""";

    public static void AssertVpnRunsOnStartup(bool shouldRunOnStartup)
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(REGISTRY_PATH);
        Assert.That(key, Is.Not.Null);
        string? value = key!.GetValue(REGISTRY_NAME) as string;
        Assert.That(value, shouldRunOnStartup ? Is.EqualTo(REGISTRY_DATA) : Is.Null);
    }
}
