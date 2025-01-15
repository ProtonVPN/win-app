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
                .NotTheCountryWantedBannerIsDisplayed();

        HomeRobot.ClickLockedChangedServer()
            .Verify.IsConnected()
            .UnlimitedServersChangesUpsellIsDisplayed();
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
            .Verify.ServersUpsellIsDisplayed()
            .NextUpsell()
            .Verify.ServersSpeedUpsellIsDisplayed()
            .NextUpsell()
            .Verify.StreamingUpsellIsDisplayed()
            .NextUpsell()
            .Verify.NetshieldUpsellIsDisplayed()
            .NextUpsell()
            .Verify.SecureCoreUpsellIsDisplayed()
            .NextUpsell()
            .Verify.P2pUpsellIsDisplayed()
            .NextUpsell()
            .Verify.TenDevicesUpsellIsDisplayed()
            .NextUpsell()
            .Verify.TorUpsellIsDisplayed()
            .NextUpsell()
            .Verify.SplitTunnelingUpsellIsDisplayed()
            .NextUpsell()
            .Verify.ProfilesUpsellIsDisplayed()
            .NextUpsell()
            .Verify.AdvancedSettingsUpsellIsDisplayed()
            .NextUpsell()
            .Verify.ServersUpsellIsDisplayed()
            .GoBackUpsell()
            .Verify.AdvancedSettingsUpsellIsDisplayed()
            .GoBackUpsell()
            .Verify.ProfilesUpsellIsDisplayed();
    }

    [Test]
    public void UpsellThroughSettings()
    {
        SettingRobot
            .OpenSettings()
            .OpenNetShieldSettings();
        UpsellCarrouselRobot.Verify.NetshieldUpsellIsDisplayed()
            .CloseModal();

        SettingRobot.OpenPortForwardingSettings();
        UpsellCarrouselRobot.Verify.P2pUpsellIsDisplayed()
            .CloseModal();

        SettingRobot.OpenSplitTunnelingSettingsCard();
        UpsellCarrouselRobot.Verify.SplitTunnelingUpsellIsDisplayed()
            .CloseModal();

        SettingRobot.OpenVpnAcceleratorSettingsCard();
        UpsellCarrouselRobot.Verify.ServersSpeedUpsellIsDisplayed()
            .CloseModal();

        SettingRobot.OpenAdvancedSettings();
        AdvancedSettingsRobot.NavigateToCustomDns();
        UpsellCarrouselRobot.Verify.AdvancedSettingsUpsellIsDisplayed()
            .CloseModal();

        AdvancedSettingsRobot.NavigateToNatSettings();
        UpsellCarrouselRobot.Verify.AdvancedSettingsUpsellIsDisplayed();
    }

    [Test]
    public void HomeScreenUpsell()
    {
        HomeRobot.Verify.ConnectionCardFreeConnectionsTaglineIsDisplayed();

        SidebarRobot.Verify.AllCountriesUpsellIsDisplayed();

        SidebarRobot.NavigateToSecureCoreCountriesTab()
            .Verify.SecureCoreUpsellIsDisplayed();

        SidebarRobot.NavigateToP2PCountriesTab()
            .Verify.P2pUpsellIsDisplayed();

        SidebarRobot.NavigateToTorCountriesTab()
            .Verify.TorUpsellIsDisplayed();

        SidebarRobot.NavigateToProfiles()
            .Verify.ProfileUpsellLabelIsDisplayed();

    }
}
