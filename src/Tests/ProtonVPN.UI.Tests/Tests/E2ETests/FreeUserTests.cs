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

using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("2")]
[Category("ARM")]
public class FreeUserTests : FreshSessionSetUp
{
    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.FreeUser);
    }

    [Test]
    public void ChangeServerFreeUser()
    {
        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        HomeRobot
            .ChangeServer()
            .Verify
                .IsConnecting()
                .IsConnected()
                .IsChangeServerLocked()
                .IsNotTheCountryWantedBannerDisplayed();

        HomeRobot.ClickLockedChangedServer()
            .Verify.IsConnected()
            .IsUnlimitedServersChangesUpsellDisplayed();
    }

    [Test]
    public void CancelChangeServerDoesNotTriggerTimer()
    {
        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();
        HomeRobot.ChangeServer();

        // Intentional delay to simlute user's input
        Thread.Sleep(500);

        HomeRobot
            .CancelConnection()
            .Verify.IsDisconnected();

        // Intentional delay to simlute user's input
        Thread.Sleep(500);

        HomeRobot.ConnectViaConnectionCard()
            .Verify.IsConnected()
            .IsChangeServerNotLocked();
    }

    [Test]
    public void UpsellCarousel()
    {
        SidebarRobot.ConnectToFastest();
        UpsellCarrouselRobot
            .Verify.IsServersUpsellDisplayed()
            .NextUpsell()
            .Verify.IsServersSpeedUpsellDisplayed()
            .NextUpsell()
            .Verify.IsStreamingUpsellDisplayed()
            .NextUpsell()
            .Verify.IsNetshieldUpsellDisplayed()
            .NextUpsell()
            .Verify.IsSecureCoreUpsellDisplayed()
            .NextUpsell()
            .Verify.IsP2pUpsellDisplayed()
            .NextUpsell()
            .Verify.IsTenDevicesUpsellDisplayed()
            .NextUpsell()
            .Verify.IsTorUpsellDisplayed()
            .NextUpsell()
            .Verify.IsSplitTunnelingUpsellDisplayed()
            .NextUpsell()
            .Verify.IsProfilesUpsellDisplayed()
            .NextUpsell()
            .Verify.IsAdvancedSettingsUpsellDisplayed()
            .NextUpsell()
            .Verify.IsServersUpsellDisplayed()
            .GoBackUpsell()
            .Verify.IsAdvancedSettingsUpsellDisplayed()
            .GoBackUpsell()
            .Verify.IsProfilesUpsellDisplayed();
    }

    [Test]
    public void UpsellThroughSettings()
    {
        SettingRobot
            .OpenSettings()
            .OpenNetShieldSettings();
        UpsellCarrouselRobot.Verify.IsNetshieldUpsellDisplayed()
            .CloseModal();

        SettingRobot.OpenPortForwardingSettings();
        UpsellCarrouselRobot.Verify.IsP2pUpsellDisplayed()
            .CloseModal();

        SettingRobot.OpenSplitTunnelingSettingsCard();
        UpsellCarrouselRobot.Verify.IsSplitTunnelingUpsellDisplayed()
            .CloseModal();

        SettingRobot.OpenVpnAcceleratorSettingsCard();
        UpsellCarrouselRobot.Verify.IsServersSpeedUpsellDisplayed()
            .CloseModal();

        SettingRobot.OpenAdvancedSettings();
        AdvancedSettingsRobot.NavigateToCustomDns();
        UpsellCarrouselRobot.Verify.IsAdvancedSettingsUpsellDisplayed()
            .CloseModal();

        AdvancedSettingsRobot.NavigateToNatSettings();
        UpsellCarrouselRobot.Verify.IsAdvancedSettingsUpsellDisplayed();
    }

    [Test]
    public void HomeScreenUpsell()
    {
        HomeRobot.Verify.IsConnectionCardFreeConnectionsTaglineDisplayed();

        SidebarRobot.Verify.IsAllCountriesUpsellDisplayed();

        SidebarRobot.NavigateToSecureCoreCountriesTab()
            .Verify.IsSecureCoreUpsellDisplayed();

        SidebarRobot.NavigateToP2PCountriesTab()
            .Verify.IsP2pUpsellDisplayed();

        SidebarRobot.NavigateToTorCountriesTab()
            .Verify.IsTorUpsellDisplayed();

        SidebarRobot.NavigateToProfiles()
            .Verify.IsProfileUpsellLabelDisplayed();

    }
}
