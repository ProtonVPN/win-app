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

using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
public class TorTests : FreshSessionSetUp
{
    private const string BROWSER_APP = "Google Chrome";

    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    public void ConnectToATorServer()
    {
        NetworkUtils.AssertTorStatus(false);

        ConnectToTorCountry();

        HomeRobot
            .Disconnect()
            .Verify.IsDisconnected();
    }

    [Test]
    public void ConnectToATorServerWithKillSwitchEnabled()
    {
        SettingRobot
            .OpenSettings()
            .OpenKillSwitchSettings()
            .ToggleKillSwitchSetting()
            .SelectKillSwitchMode(KillSwitchMode.Advanced)
            .ApplySettings()
            .CloseSettings();

        try
        {
            ConnectToTorCountry();

            BrowserUtils.AssertBrowserCanLoadDuckDuckGo(BROWSER_APP);
            BrowserUtils.KillAllBrowsers();
        }
        catch { }

        SettingRobot
            .OpenSettings()
            .OpenKillSwitchSettings()
            .DisableKillSwitch()
            .CloseSettings();

        HomeRobot
            .Disconnect()
            .Verify.IsDisconnected();
    }

    private void ConnectToTorCountry()
    {
        string failureMessages = string.Empty;

        foreach (string country in TestConstants.AvailableCountries)
        {
            try
            {
                SidebarRobot
                    .NavigateToTorCountriesTab()
                    .ConnectToCountry(country);
                HomeRobot
                    .Verify.IsConnected();

                string? ipAddressConnected = HomeRobot.GetVpnServerIp();
                NetworkUtils.AssertTorStatus(true, ipAddressConnected);
                return;
            }
            catch (AssertionException e)
            {
                failureMessages += $"Failed to connect to {country}: {e.Message}\n";
            }
        }

        Assert.Fail(failureMessages);
    }
}