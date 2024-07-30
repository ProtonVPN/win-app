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

using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.Robots.Settings;
using ProtonVPN.UI.Tests.Robots.Shell;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
public class CustomDnsTests : TestSession
{
    private ShellRobot _shellRobot = new();
    private SettingsRobot _settingsRobot = new();
    private HomeRobot _homeRobot = new();
    private LoginRobot _loginRobot = new();

    private const string CUSTOM_DNS_ADDRESS = "8.8.8.8";

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        LaunchApp();

        _loginRobot
            .Wait(TestConstants.StartupDelay)
            .DoLogin(TestUserData.PlusUser);

        _homeRobot
            .DoCloseWelcomeOverlay()
            .DoWaitForVpnStatusSubtitleLabel()
            .VerifyVpnStatusIsDisconnected()
            .VerifyConnectionCardIsInInitalState();

        _homeRobot
            .DoConnect()
            .VerifyAllStatesUntilConnected();
    }

    [SetUp]
    public void SetUp()
    {
        _shellRobot
            .DoNavigateToSettingsPage();
        _settingsRobot
            .DoNavigateToCustomDnsServersSettingsPage();
    }

    [Test, Order(0)]
    public void NetshieldIsDisabledWhenCustomDnsIsSet()
    {
        _settingsRobot
            .DoClickCustomDnsToggle()
            .DoEnableOnWarning()
            .DoApplyChanges()
            .VerifyCustomDnsIsEnabled();

        _shellRobot
            .DoNavigateToHomePage();
        _homeRobot
            .VerifyVpnStatusIsConnected();

        _shellRobot
            .DoNavigateToNetShieldFeaturePage();
        _settingsRobot
            .VerifyNetshieldIsDisabled()
            .Wait(TestConstants.FiveSecondsTimeout);
        _settingsRobot
            .VerifyNetshieldIsNotBlocking();
    }

    [Test, Order(1)]
    public void CustomDnsIsSet()
    {
        _settingsRobot
            .DoSetCustomDnsIpAddress(CUSTOM_DNS_ADDRESS)
            .DoApplyChanges();

        VerifyIfConnectedAndDnsIsSet(CUSTOM_DNS_ADDRESS);
    }

    [Test, Order(2)]
    public void CustomDnsIsDisabledByToggle()
    {
        _settingsRobot
            .DoClickCustomDnsToggle()
            .DoApplyChanges();

        VerifyIfConnectedAndDnsIsNotSet(CUSTOM_DNS_ADDRESS);

        _shellRobot
            .DoNavigateToSettingsPage();
        _settingsRobot
            .DoNavigateToCustomDnsServersSettingsPage()
            .DoClickCustomDnsToggle()
            .DoApplyChanges();
    }

    [Test, Order(3)]
    public void CustomDnsIsDisabledByTickingDnsAddressCheckBox()
    {
        _settingsRobot
            .DoClickCustomDnsAddress(CUSTOM_DNS_ADDRESS)
            .DoApplyChanges();

        VerifyIfConnectedAndDnsIsNotSet(CUSTOM_DNS_ADDRESS);
    }

    [Test, Order(4)]
    public void CustomDnsIsEnabledByTickingDnsAddressCheckBox()
    {
        _settingsRobot
            .DoClickCustomDnsAddress(CUSTOM_DNS_ADDRESS)
            .DoApplyChanges();

        VerifyIfConnectedAndDnsIsSet(CUSTOM_DNS_ADDRESS);
    }

    [Test, Order(5)]
    public void CustomDnsIsDisabledByClickingTrashIcon()
    {
        _settingsRobot
            .DoClickTrashIcon()
            .DoApplyChanges();

        VerifyIfConnectedAndDnsIsNotSet(CUSTOM_DNS_ADDRESS);
    }

    private void VerifyIfConnectedAndDnsIsNotSet(string dnsAddress)
    {
        _shellRobot
            .DoNavigateToHomePage();
        _homeRobot
            .VerifyVpnStatusIsConnected();
        _settingsRobot
            .VerifyCustomDnsIsNotSet(dnsAddress);
    }

    private void VerifyIfConnectedAndDnsIsSet(string dnsAddress)
    {
        _shellRobot
            .DoNavigateToHomePage();
        _homeRobot
            .VerifyVpnStatusIsConnected();
        _settingsRobot
            .VerifyCustomDnsIsSet(dnsAddress);
    }

    [TearDown]
    public void SaveArtifacts()
    {
        SaveScreenshotAndLogsIfFailed();
    }

    [OneTimeTearDown]
    public void TestCleanup()
    {
        Cleanup();
    }
}
