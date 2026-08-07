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
using System.Collections.Generic;
using System.Threading;
using FlaUI.Core.Tools;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Enums.Locations;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("4")]
[Category("ARM")]
[Category("SMOKE_4")]
public class FreeUserTests : FreshSessionSetUp
{
    private const Browser BROWSER_APP = Browser.GoogleChrome;
    private const DefaultProfile PROFILE_NAME = DefaultProfile.MaxSecurity;

    private const Country COUNTRY = Country.Austria;
    private const City CITY = City.Vienna;
    private const Country VIA_COUNTRY = Country.Switzerland;
    private const Country SERVER_COUNTRY = Country.Australia;
    private const Country TOR_COUNTRY = Country.France;

    private static readonly Country[] _freeCountries = [Country.Canada, Country.Japan, Country.Mexico, Country.Netherlands, Country.Norway, Country.Poland, Country.Romania, Country.Singapore, Country.Switzerland, Country.UnitedStates];

    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.FreeUser);
    }

    [Test]
    [Property("TestCaseId", "602338")]
    public void ConnectToServerFreeUser()
    {
        HomeRobot
            .Verify.IsConnectionCardFreeConnectionsTaglineDisplayed()
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
                   .ConnectionCardDescriptionContainsOneOf(_freeCountries);
    }

    [Test]
    [Property("TestCaseId", "602339")]
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

        SettingRobot.CloseSettingsUsingEscButton();
    }

    [Test]
    [Property("TestCaseId", "602387")]
    [Retry(3)]
    public void CancelChangeServerDoesNotTriggerTimer()
    {
        CommonUiFlows.EnsureUserIsDisconnected();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        CancelChangeServerWithRetry();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
            .IsChangeServerNotLocked();
    }

    [Test]
    [Property("TestCaseId", "602459")]
    public void LocalNetworkingIsNotReachableWhileConnected()
    {
        HomeRobot
            .Verify.IsDisconnected()
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        NetworkUtils.VerifyLocalNetworking(isLanEnabled: false);

        SettingRobot
            .OpenSettings()
            .OpenAdvancedSettings();
        AdvancedSettingsRobot
            .NavigateToLan();
        UpsellCarrouselRobot
            .Verify.IsAdvancedSettingsUpsellDisplayed()
            .CloseModal();
    }

    [Test]
    [Property("TestCaseId", "602390")]
    public void UpsellCarousel()
    {
        CommonUiFlows.EnsureUserIsDisconnected();

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
    [Property("TestCaseId", "602388")]
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
            .OpenSplitTunnelingSettings();
        UpsellCarrouselRobot
            .Verify.IsSplitTunnelingUpsellDisplayed()
            .CloseModal();

        SettingRobot
            .OpenVpnAcceleratorSettings();
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
    [Property("TestCaseId", "602389")]
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
    [Property("TestCaseId", "609947")]
    [Retry(5)]
    public void ConnectionRequestTriggersUpsellCarousel()
    {
        CommonUiFlows.EnsureUserIsDisconnected();

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

        //TODO: Hover over the map and click a country's pin
        //The "Discover VPN Plus" pop-up modal is displayed

        CommonAssertions.AssertIpAddressUnchanged(ipAddressToCompare!);
    }

    [Test]
    [Property("TestCaseId", "870071")]
    [Retry(3)]
    public void StreamingInProgressUpsell()
    {
        CommonUiFlows.EnsureUserIsDisconnected();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        BrowserUtils.OpenStreamingWebsite(BROWSER_APP);

        UpsellCarrouselRobot
            .Verify.IsStreamingInProgressUpsellDisplayed()
            .CloseModal();

        BrowserUtils.KillAllBrowsers();
    }

    [Test]
    [Property("TestCaseId", "602438")]
    [Retry(4)]
    public void P2PInProgressUpsell()
    {
        TorrentHelper.AllowTorrentFirewall();
        TorrentHelper.StopAndCleanup();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        string? ipAddressConnected = HomeRobot.GetVpnServerIp();
        Assert.That(ipAddressConnected, Is.Not.Null);
        int portToCheck = 52069;

        try
        {
            TorrentHelper.StartTorrentOnPort(portToCheck);
            Window?.Focus();

            UpsellCarrouselRobot
                .Verify.IsP2PTorrentInProgressUpsellDisplayed()
                .CloseModal();

            // give it time so that the internet restores
            Thread.Sleep(TestConstants.TenSecondsTimeout);
            BrowserUtils.AssertBrowserInternetAvailability(BROWSER_APP, shouldBeAvailable: true);
            TorrentHelper.IsPortClosed(ipAddressConnected!, portToCheck);
        }
        finally
        {
            BrowserUtils.KillAllBrowsers();
            TorrentHelper.StopAndCleanup();
        }
    }

    private void VerifyTabUpsells(
        Func<UpsellCarrouselRobot.Verifications> verifyAction,
        Country? country = null,
        City? city = null,
        Country? serverCountry = null,
        Country? secureCoreCountry = null,
        DefaultProfile? profileName = null)
    {
        if (country != null)
        {
            VerifyUpsellAndClose(() =>
                SidebarRobot
                    .ConnectToCountry(country.Value), verifyAction);
        }

        if (country != null && city != null)
        {
            VerifyUpsellAndClose(() =>
                SidebarRobot
                    .ExpandCities(country.Value)
                    .ConnectToCity(city.Value), verifyAction);
        }

        if (country != null && secureCoreCountry != null)
        {
            VerifyUpsellAndClose(() =>
                SidebarRobot
                    .ExpandCities(country.Value)
                    .ConnectViaSecureCore(country.Value, secureCoreCountry.Value), verifyAction);
        }

        if (serverCountry != null)
        {
            SidebarRobot customSidebarRobot = (city == null)
                ? SidebarRobot
                : SidebarRobot.ExpandCities(serverCountry.Value);

            VerifyUpsellAndClose(() =>
                customSidebarRobot
                    .ExpandSpecificServerList()
                    .ConnectToServer(), verifyAction);
        }

        if (profileName != null)
        {
            VerifyUpsellAndClose(() =>
                SidebarRobot
                    .ConnectToProfile(profileName.Value.GetEnumValue()), verifyAction);
        }
    }

    private void VerifyUpsellAndClose(Action connectAction, Func<UpsellCarrouselRobot.Verifications> verifyAction)
    {
        connectAction();
        verifyAction();

        UpsellCarrouselRobot.CloseModal();
    }

    private void CancelChangeServerWithRetry()
    {
        Retry.WhileFalse(
           () =>
           {
               try
               {
                   HomeRobot
                       .ChangeServer()
                       .CancelConnection(TestConstants.MoreFrequentRetryInterval)
                       .Verify.IsDisconnected();

                   return true;
               }
               catch (TimeoutException)
               {
                   return false;
               }
           },
           TestConstants.ThirtySecondsTimeout, TestConstants.TenSecondsTimeout);
    }
}