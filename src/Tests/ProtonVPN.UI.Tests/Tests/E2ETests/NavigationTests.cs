/*
 * Copyright (c) 2025 Proton AG
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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("2")]
public class NavigationTests : FreshSessionSetUp
{
    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    public void NavigateToSettingsViaKebabMenu()
    {
        HomeRobot.ExpandKebabMenuButton()
            .NavigateToSettingsViaKebabMenu();

        SettingRobot.Verify.SettingsPageIsDisplayed();
    }

    [Test]
    public void AppExitViaKebabMenu()
    {
        HomeRobot.ExpandKebabMenuButton()
            .ExitViaKebabMenu();

        // Allow some delay after exiting the app
        Thread.Sleep(TestConstants.FiveSecondsTimeout);

        Assert.That(AreNoProtonVPNProcessesRunning, Is.True, "ProtonVPN process was still running after app was exited.");
    }

    [Test]
    public void AppExitViaAccountDropDown()
    {
        SettingRobot
            .OpenSettings()
            .ExpandAccountDropdown()
            .ExitTheApp();

        // Allow some delay after exiting the app
        Thread.Sleep(TestConstants.FiveSecondsTimeout);

        Assert.That(AreNoProtonVPNProcessesRunning, Is.True, "ProtonVPN process was still running after app was exited.");
    }

    public static bool AreNoProtonVPNProcessesRunning() =>
        !Process.GetProcesses().Any(p => new[] { "ProtonVPN", "ProtonVPNService" }.Contains(p.ProcessName, StringComparer.OrdinalIgnoreCase));
}
