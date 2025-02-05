/*
 * Copyright (c) 2024 Proton AG
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
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
[Category("ARM")]
public class CustomDnsTests : BaseTest
{
    private const string CUSTOM_DNS_SERVER = "8.8.8.8";
    private const string SECONDARY_CUSTOM_DNS_SERVER = "1.1.1.1";

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        LaunchApp();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test, Order(0)]
    public void NetshieldIsDisabledWhenCustomDnsIsEnabled()
    {
        SettingRobot
           .OpenSettings()
           .Verify.IsNetshieldEnabledStateDisplayed()
           .OpenAdvancedSettings();

        AdvancedSettingsRobot
            .NavigateToCustomDns()
            .ToggleCustomDnsSetting()
            .PressEnable();

        SettingRobot
            .ApplySettings()
            .Verify.IsNetshieldDisableStateDisplayed();
    }

    //On Some screens notifications have to be disabled in order for it to pass. 
    // (Navigation to settings page is overlayed by nofication)
    [Test, Order(1)]
    public void CustomDnsIsSet()
    {
        SettingRobot
            .OpenAdvancedSettings();
        AdvancedSettingsRobot
            .NavigateToCustomDns();

        AdvancedSettingsRobot
            .SetCustomDns(CUSTOM_DNS_SERVER);
        SettingRobot
            .ApplySettings()
            .CloseSettings();
        
        HomeRobot.ConnectViaConnectionCard()
            .Verify.IsConnected();

        AdvancedSettingsRobot.Verify.IsCustomDnsAddressSet(CUSTOM_DNS_SERVER);
    }

    [Test, Order(2)]
    public void CustomDnsIsDisabledByTickingCheckBox()
    {
        NavigateToCustomDnsSetting();

        AdvancedSettingsRobot
            .TickCustomDnsServer(CUSTOM_DNS_SERVER);

        SettingRobot.Reconnect();
        HomeRobot.Verify.IsConnected();
        AdvancedSettingsRobot.Verify.IsCustomDnsAddressNotSet(CUSTOM_DNS_SERVER);
    }

    [Test, Order(3)]
    public void CustomDnsServerRemoval()
    {
        NavigateToCustomDnsSetting();

        AdvancedSettingsRobot
            .RemoveCustomDnsServer()
            .SetCustomDns(SECONDARY_CUSTOM_DNS_SERVER);
        SettingRobot.Reconnect();
        HomeRobot.Verify.IsConnected();

        AdvancedSettingsRobot.Verify.IsCustomDnsAddressNotSet(CUSTOM_DNS_SERVER);
        AdvancedSettingsRobot.Verify.IsCustomDnsAddressSet(SECONDARY_CUSTOM_DNS_SERVER);
    }

    [Test, Order(4)]
    public void CustomDnsDisableSettingDoesNotSetCustomDnsServer()
    {
        NavigateToCustomDnsSetting();

        AdvancedSettingsRobot.ToggleCustomDnsSetting();
        SettingRobot.Reconnect();
        HomeRobot.Verify.IsConnected();

        AdvancedSettingsRobot.Verify.IsCustomDnsAddressNotSet(SECONDARY_CUSTOM_DNS_SERVER);
    }

    [OneTimeTearDown] 
    public void OneTimeTearDown() 
    {
        Cleanup();
    }

    private void NavigateToCustomDnsSetting()
    {
        SettingRobot.OpenSettings()
            .OpenAdvancedSettings();
        AdvancedSettingsRobot
            .NavigateToCustomDns();
    }
}
