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

using System;
using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("2")]
[Category("ARM")]
public class FreeUserTests : FreshSessionSetUp
{
    private const string PROFILE_NAME = "Max security";

    private const string COUNTRY = "Austria";
    private const string CITY = "Vienna";
    private const string VIA_COUNTRY = "via Switzerland";
    private const string SERVER_COUNTRY = "Australia";
    private const string TOR_COUNTRY = "France";

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
            .Verify.IsConnected()
            .ChangeServer()
            .Verify.IsConnected()
                   .IsChangeServerLocked()
                   .IsNotTheCountryWantedBannerDisplayed()
            .ClickLockedChangedServer()
            .Verify.IsConnected()
            .IsUnlimitedServersChangesUpsellDisplayed();
    }

    [Test]
    public void CancelChangeServerDoesNotTriggerTimer()
    {
        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
            .ChangeServer();

        // Intentional delay to simulate user's input
        Thread.Sleep(TestConstants.UserInputSimulationDelay);

        HomeRobot
            .CancelConnection()
            .Verify.IsDisconnected();

        // Intentional delay to simulate user's input
        Thread.Sleep(TestConstants.UserInputSimulationDelay);

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
            .IsChangeServerNotLocked();
    }

    [Test]
    public void UpsellCarousel()
    {
        SidebarRobot
            .ConnectToFastest();
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
            .Verify.IsP2PUpsellDisplayed()
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
            .Verify.IsProfilesUpsellDisplayed()
            .CloseModal();
    }

    [Test]
    public void UpsellThroughSettings()
    {
        SettingRobot
            .OpenSettings()
            .OpenNetShieldSettings();
        UpsellCarrouselRobot
            .Verify.IsNetshieldUpsellDisplayed()
            .CloseModal();

        SettingRobot
            .OpenPortForwardingSettings();
        UpsellCarrouselRobot
            .Verify.IsP2PUpsellDisplayed()
            .CloseModal();

        SettingRobot
            .OpenSplitTunnelingSettingsCard();
        UpsellCarrouselRobot
            .Verify.IsSplitTunnelingUpsellDisplayed()
            .CloseModal();

        SettingRobot
            .OpenVpnAcceleratorSettingsCard();
        UpsellCarrouselRobot
            .Verify.IsServersSpeedUpsellDisplayed()
            .CloseModal();

        SettingRobot
            .OpenAdvancedSettings();
        AdvancedSettingsRobot
            .NavigateToCustomDns();
        UpsellCarrouselRobot
            .Verify.IsAdvancedSettingsUpsellDisplayed()
            .CloseModal();

        AdvancedSettingsRobot
            .NavigateToNatSettings();
        UpsellCarrouselRobot
            .Verify.IsAdvancedSettingsUpsellDisplayed()
            .CloseModal();
    }

    [Test]
    public void HomeScreenUpsell()
    {
        HomeRobot
            .Verify.IsConnectionCardFreeConnectionsTaglineDisplayed();

        SidebarRobot
            .Verify.IsAllCountriesUpsellDisplayed()
            .NavigateToSecureCoreCountriesTab()
            .Verify.IsSecureCoreUpsellDisplayed()
            .NavigateToP2PCountriesTab()
            .Verify.IsP2PUpsellDisplayed()
            .NavigateToTorCountriesTab()
            .Verify.IsTorUpsellDisplayed()
            .NavigateToProfiles()
            .Verify.IsProfileUpsellLabelDisplayed();
    }

    [Test]
    public void ConnectionRequestTriggersUpsellCarousel()
    {
        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        string? ipAddressToCompare = HomeRobot.GetVpnServerIp();

        SidebarRobot.NavigateToAllCountriesTab();
        VerifyTabUpsells(
            UpsellCarrouselRobot.Verify.IsServersUpsellDisplayed,
            country: COUNTRY,
            city: CITY,
            serverCountry: SERVER_COUNTRY);

        SidebarRobot.NavigateToSecureCoreCountriesTab();
        VerifyTabUpsells(
            UpsellCarrouselRobot.Verify.IsSecureCoreUpsellDisplayed,
            country: COUNTRY,
            secureCoreCountry: VIA_COUNTRY);

        SidebarRobot.NavigateToP2PCountriesTab();
        VerifyTabUpsells(
            UpsellCarrouselRobot.Verify.IsP2PUpsellDisplayed,
            country: COUNTRY, city: CITY,
            serverCountry: SERVER_COUNTRY);

        SidebarRobot.NavigateToTorCountriesTab();
        VerifyTabUpsells(
            UpsellCarrouselRobot.Verify.IsTorUpsellDisplayed,
            country: TOR_COUNTRY,
            serverCountry: TOR_COUNTRY);

        SidebarRobot.NavigateToProfiles();
        VerifyTabUpsells(
            UpsellCarrouselRobot.Verify.IsProfilesUpsellDisplayed,
            profileName: PROFILE_NAME);

        //Hover over the map and click a country's pin
        //The "Discover VPN Plus" pop-up modal is displayed

        CommonAssertions.AssertIpAddressUnchanged(ipAddressToCompare!);
    }

    private void VerifyTabUpsells(
        Func<UpsellCarrouselRobot.Verifications> verifyAction,
        string? country = null,
        string? city = null,
        string? serverCountry = null,
        string? secureCoreCountry = null,
        string? profileName = null)
    {
        if (country != null)
        {
            VerifyUpsellAndClose(() =>
                SidebarRobot
                    .ConnectToCountry(country), verifyAction);
        }

        if (country != null && city != null)
        {
            VerifyUpsellAndClose(() =>
                SidebarRobot
                    .ExpandCities(country)
                    .ConnectToCity(city), verifyAction);
        }

        if (country != null && secureCoreCountry != null)
        {
            VerifyUpsellAndClose(() =>
                SidebarRobot
                    .ExpandCities(country)
                    .ConnectViaSecureCore(country, secureCoreCountry), verifyAction);
        }

        if (serverCountry != null)
        {
            SidebarRobot customSidebarRobot = (city == null)
                ? SidebarRobot
                : SidebarRobot.ExpandCities(serverCountry);

            VerifyUpsellAndClose(() =>
                customSidebarRobot
                    .ExpandSpecificServerList()
                    .ConnectToServer(), verifyAction);
        }

        if (profileName != null)
        {
            VerifyUpsellAndClose(() =>
                SidebarRobot
                    .ConnectToProfile(profileName), verifyAction);
        }
    }

    private void VerifyUpsellAndClose(Action connectAction, Func<UpsellCarrouselRobot.Verifications> verifyAction)
    {
        connectAction();
        verifyAction();

        UpsellCarrouselRobot.CloseModal();
    }
}