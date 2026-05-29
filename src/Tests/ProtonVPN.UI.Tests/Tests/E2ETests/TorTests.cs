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

using System.Text;
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
    [Property("TestCaseId", "602368")]
    [Retry(3)]
    [Category("ARM")]
    [Category("SMOKE_1")]
    public void ConnectToATorServer()
    {
        NetworkUtils.AssertTorStatus(shouldBeAvailable: false);

        ConnectToTorCountry();

        HomeRobot
            .Disconnect()
            .Verify.IsDisconnected();
    }

    [Test]
    [Property("TestCaseId", "760479")]
    [Retry(4)]
    public void ConnectToATorServerWithKillSwitchEnabled()
    {
        SettingRobot
            .OpenSettings()
            .OpenKillSwitchSettings()
            .EnableKillSwitchToggle()
            .SelectKillSwitchMode(KillSwitchMode.Advanced)
            .ApplySettings()
            .CloseSettings();

        try
        {
            ConnectToTorCountry();

            BrowserUtils.AssertBrowserCanLoadDuckDuckGo(BROWSER_APP);
            BrowserUtils.KillAllBrowsers();
        }
        finally
        {
            SettingRobot
                .OpenSettings()
                .OpenKillSwitchSettings()
                .DisableKillSwitchToggle()
                .ApplySettings()
                .CloseSettings();

            HomeRobot
                .Disconnect()
                .Verify.IsDisconnected();
        }
    }

    private void ConnectToTorCountry()
    {
        StringBuilder failureMessages = new();

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
                NetworkUtils.AssertTorStatus(shouldBeAvailable: true, ipAddressConnected);
                return;
            }
            catch (AssertionException e)
            {
                failureMessages.AppendLine($"Failed to connect to {country}: {e.Message}");
            }
        }

        Assert.Fail(failureMessages.ToString());
    }
}