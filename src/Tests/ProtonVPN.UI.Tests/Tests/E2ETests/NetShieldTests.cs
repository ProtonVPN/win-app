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

using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
[Category("ARM")]
public class NetShieldTests : FreshSessionSetUp
{
    private const string ENABLE_NET_SHIELD_TITLE = "Enable NetShield?";
    private const string ENABLE_NET_SHIELD_DESCRIPTION = "You won't be able to connect with a custom DNS server when NetShield is enabled.";
    private const string ENABLE_NET_SHIELD_BUTTON = "Enable";

    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    public void NetshieldOnLevelTwo()
    {
        ConnectAndVerifyIsConnected();

        SettingRobot
            .Verify.IsNetshieldBlocking(NetShieldMode.BlockAdsMalwareTrackers);
    }

    [Test]
    public void NetshieldOnLevelThree()
    {
        SettingRobot
            .OpenSettings()
            .OpenNetShieldSettings()
            .SelectNetShieldMode(NetShieldMode.BlockAdsMalwareTrackersAdultContent)
            .ApplySettings()
            .CloseSettings();

        ConnectAndVerifyIsConnected();

        SettingRobot
            .Verify.IsNetshieldBlocking(NetShieldMode.BlockAdsMalwareTrackersAdultContent);
    }

    [Test]
    public void NetshieldOnLevelOne()
    {
        SettingRobot
            .OpenSettings()
            .OpenNetShieldSettings()
            .SelectNetShieldMode(NetShieldMode.BlockMalwareOnly)
            .ApplySettings()
            .CloseSettings();

        ConnectAndVerifyIsConnected();

        SettingRobot
            .Verify.IsNetshieldBlocking(NetShieldMode.BlockMalwareOnly);
    }

    [Test]
    public void NetshieldOff()
    {
        SettingRobot
            .OpenSettings()
            .OpenNetShieldSettings()
            .ToggleNetShieldSetting()
            .ApplySettings()
            .CloseSettings();

        ConnectAndVerifyIsConnected();

        SettingRobot
            .Verify.IsNetshieldNotBlocking();
    }

    [Test]
    public void PaidSettingsAreNotTransferedOnPaidToFreeUserSwitch()
    {
        SettingRobot
            .OpenSettings()
            .OpenNetShieldSettings()
            .SelectNetShieldMode(NetShieldMode.BlockMalwareOnly)
            .ApplySettings()
            .CloseSettings();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();
        SettingRobot
            .Verify.IsNetshieldBlocking(NetShieldMode.BlockMalwareOnly);

        SettingRobot.OpenSettings()
            .ExpandAccountDropdown()
            .SignOut()
            .ConfirmSignOut();

        CommonUiFlows.FullLogin(TestUserData.FreeUser);

        HomeRobot.ConnectViaConnectionCard()
            .Verify.IsConnected();

        SettingRobot.Verify.IsNetshieldNotBlocking()
            .OpenSettings()
            .Verify.IsNetshieldDisabledStateDisplayed();
    }

    [Test]
    public void CustomDnsIsDisabledWhenNetshieldIsEnabled()
    {
        TurnOnDns();

        TurnOnNetShieldAndVerifyConfirmationDialog();
        ConfirmationRobot
            .CancelAction();

        //delay to not trigger the "unsaved changes" modal)
        Thread.Sleep(TestConstants.OneSecondTimeout);

        VerifyCustomDnsIsEnabledAndNetShieldIsDisabled();

        TurnOnNetShieldAndVerifyConfirmationDialog();
        ConfirmationRobot
            .PrimaryAction();
        SettingRobot
            .ApplySettings();

        VerifyNetShieldIsEnabledAndCustomDnsIsDisabled();
    }

    private void ConnectAndVerifyIsConnected()
    {
        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();
    }

    private void TurnOnDns()
    {
        SettingRobot
            .OpenSettings()
            .OpenAdvancedSettings();
        AdvancedSettingsRobot
            .NavigateToCustomDns()
            .ToggleCustomDnsSetting();
        ConfirmationRobot
            .PrimaryAction();
        SettingRobot
            .ApplySettings();
    }

    private void TurnOnNetShieldAndVerifyConfirmationDialog()
    {
        SettingRobot
            .OpenNetShieldSettings()
            .ToggleNetShieldSetting();
        ConfirmationRobot
            .Verify.IsOverlayDisplayed()
                   .OverlayTextContains(ENABLE_NET_SHIELD_TITLE)
                   .OverlayTextContains(ENABLE_NET_SHIELD_DESCRIPTION)
                   .OverlayButtonsEquals(primary: ENABLE_NET_SHIELD_BUTTON);
    }

    private void VerifyNetShieldIsEnabledAndCustomDnsIsDisabled()
    {
        SettingRobot
            .Verify.IsNetshieldEnabledStateDisplayed()
            .OpenAdvancedSettings();
        AdvancedSettingsRobot
            .NavigateToCustomDns()
            .Verify.IsCustomDnsDisabled();
    }

    private void VerifyCustomDnsIsEnabledAndNetShieldIsDisabled()
    {
        SettingRobot
            .CloseSettings()
            .OpenSettings()
            .Verify.IsNetshieldDisabledStateDisplayed()
            .OpenAdvancedSettings();
        AdvancedSettingsRobot
            .NavigateToCustomDns()
            .Verify.IsCustomDnsEnabled();
        SettingRobot
            .CloseSettings()
            .OpenSettings();
    }
}