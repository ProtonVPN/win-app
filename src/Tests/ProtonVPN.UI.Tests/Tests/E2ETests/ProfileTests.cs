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
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Enums.Locations;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("2")]
[Category("ARM")]
[Category("SMOKE_4")]
public class ProfileTests : FreshSessionSetUp
{
    private const string PROFILE_NAME = "Profile A";
    private const string CUSTOM_SETTINGS_PROFILE_NAME = "Profile C";
    private const Protocol CUSTOM_SETTINGS_PROTOCOL = Protocol.WireGuardTcp;

    private const Country COUNTRY_NAME = Country.Australia;
    private const City CITY_NAME = City.Perth;

    private const string WEBSITE_PROFILE_NAME = "Open web Profile";
    private const string WEBSITE_TO_OPEN = "youtube.com";
    private const string WEBSITE_TO_ASSERT = "YouTube";

    private const string APP_PROFILE_NAME = "Open app Profile";
    private const string APP_TO_OPEN = "Google Chrome";
    private const string APP_TO_OPEN_PATH = @"C:\Program Files\Google\Chrome\Application\chrome.exe";

    private readonly string _connectionCardDescription = $"{COUNTRY_NAME.GetName()} - {CITY_NAME.GetEnumValue()}";

    private static readonly (string profileName, ConnectionType connectionType, Country countryName, Protocol protocol)[] _profiles =
    {
        (profileName: "Profile 1", connectionType: ConnectionType.Standard, countryName: Country.Argentina, protocol: Protocol.OpenVpnUdp),
        (profileName: "Profile 2", connectionType: ConnectionType.P2P, countryName: Country.Belgium, protocol: Protocol.WireGuardTcp),
        (profileName: "Profile 3", connectionType: ConnectionType.SecureCore, countryName: Country.Egypt, protocol: Protocol.WireGuardUdp)
    };

    [SetUp]
    public void SetUp()
    {
        BrowserUtils.KillAllBrowsers();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
        SidebarRobot
            .NavigateToProfiles();
        NavigationRobot
            .Verify.IsOnProfilesPage();
    }

    [Test, Order(0)]
    [Property("TestCaseId", "247")]
    public void VerifyDefaultProfilesExist()
    {
        foreach (DefaultProfile profile in Enum.GetValues(typeof(DefaultProfile)))
        {
            SidebarRobot
                .Verify.DoesConnectionItemExist(profile.GetEnumValue());
        }
    }

    [Test, Order(1)]
    [Property("TestCaseId", "602398")]
    public void EmptyProfileList()
    {
        RemoveProfiles();

        SidebarRobot
            .Verify.NoProfilesLabelIsDisplayed();
    }

    [Test, Order(2)]
    [Property("TestCaseId", "602399")]
    public void CreateProfile()
    {
        SidebarRobot
            .ClickCreateProfile();
        NavigationRobot
            .Verify.IsOnProfilePage();
        ProfileRobot
            .SetProfileName(PROFILE_NAME)
            .SelectCountry(COUNTRY_NAME)
            .SaveProfile();
        SidebarRobot
            .ScrollToProfile(PROFILE_NAME)
            .Verify.DoesConnectionItemExist(PROFILE_NAME);
    }

    [Test, Order(3)]
    [Property("TestCaseId", "602400")]
    public void ConnectToProfileAndDisconnect()
    {
        QuickCreateProfile(PROFILE_NAME);

        SidebarRobot
            .ConnectToProfile(PROFILE_NAME);

        HomeRobot
            .Verify.IsConnecting()
                   .ConnectionCardTitleEquals(PROFILE_NAME)
                   .IsConnected()
                   .ConnectionCardTitleEquals(PROFILE_NAME);

        SidebarRobot
            .ScrollToProfile(PROFILE_NAME)
            .Verify.DoesConnectionItemExist(PROFILE_NAME)
            .DisconnectViaProfile(PROFILE_NAME);

        HomeRobot
            .Verify.IsDisconnected();
    }

    [Test, Order(4)]
    [Property("TestCaseId", "602401")]
    public void EditProfile()
    {
        QuickCreateProfile(PROFILE_NAME);

        SidebarRobot
            .ConnectToProfile(PROFILE_NAME);
        HomeRobot
            .Verify.IsConnected();

        SettingRobot
            .Verify.IsNetshieldBlocking(NetShieldMode.BlockAdsMalwareTrackers);

        SidebarRobot
            .ExpandSecondaryActionsForProfile(PROFILE_NAME)
            .EditProfile();

        ProfileRobot
            .ExpandSettingsSection()
            .DisableNetShield()
            .SaveProfile();

        HomeRobot
            .Verify.IsConnected();

        SettingRobot
            .Verify.IsNetshieldNotBlocking();
    }

    [Test, Order(5)]
    [Property("TestCaseId", "602402")]
    public void DeleteProfile()
    {
        QuickCreateProfile(PROFILE_NAME);

        SidebarRobot
            .ExpandSecondaryActionsForProfile(PROFILE_NAME)
            .DeleteProfile();

        ConfirmationRobot
            .PrimaryAction();

        // Wait for profile to be deleted
        Thread.Sleep(TestConstants.AnimationDelay);

        SidebarRobot
            .Verify.IsConnectionItemMissing(PROFILE_NAME);
    }

    [Test, Order(6)]
    [Property("TestCaseId", "254")]
    public void DiscardNewProfile()
    {
        SidebarRobot
            .ClickCreateProfile();
        NavigationRobot
            .Verify.IsOnProfilePage();
        ProfileRobot
            .SetProfileName(PROFILE_NAME)
            .SelectCountry(COUNTRY_NAME)
            .CloseProfile();

        ConfirmationRobot
            .PrimaryAction();

        SidebarRobot
            .Verify.IsConnectionItemMissing(PROFILE_NAME);
    }

    [Test, Order(7)]
    [Property("TestCaseId", "610978")]
    [Category("5")]
    [Retry(3)]
    public void ConnectAndGoWebsite()
    {
        BrowserUtils.KillAllBrowsers();

        SidebarRobot
            .ClickCreateProfile();
        NavigationRobot
            .Verify.IsOnProfilePage();
        ProfileRobot
            .SetProfileName(WEBSITE_PROFILE_NAME)
            .SelectConnectAndGoOption(ConnectAndGoOption.OpenWebsite)
            .TypeConnectAndGoWebsite(WEBSITE_TO_OPEN);
        SaveProfile();

        SidebarRobot
            .ConnectToProfile(WEBSITE_PROFILE_NAME);

        HomeRobot
            .Verify.IsConnected()
                   .ConnectionCardTitleEquals(WEBSITE_PROFILE_NAME);

        DesktopRobot
            .Verify.IsWindowTitlePresent(WEBSITE_TO_ASSERT);

        BrowserUtils.KillAllBrowsers();
    }

    [Test, Order(8)]
    [Property("TestCaseId", "760486")]
    [Category("5")]
    [Retry(3)]
    public void ConnectAndGoApp()
    {
        BrowserUtils.KillAllBrowsers();

        SidebarRobot
            .ClickCreateProfile();
        NavigationRobot
            .Verify.IsOnProfilePage();
        ProfileRobot
            .SetProfileName(APP_PROFILE_NAME)
            .SelectConnectAndGoOption(ConnectAndGoOption.OpenApp)
            .SelectConnectAndGoApp(APP_TO_OPEN_PATH)
            .Verify.IsAppSelected(APP_TO_OPEN);
        SaveProfile();

        SidebarRobot
            .ConnectToProfile(APP_PROFILE_NAME);

        HomeRobot
            .Verify.IsConnected()
                   .ConnectionCardTitleEquals(APP_PROFILE_NAME);

        DesktopRobot
            .Verify.IsWindowTitlePresent(APP_TO_OPEN);

        BrowserUtils.KillAllBrowsers();
    }

    [Test, Order(9)]
    [Property("TestCaseId", "610977")]
    public void ConnectWithCustomSettings()
    {
        SidebarRobot
            .ClickCreateProfile();
        NavigationRobot
            .Verify.IsOnProfilePage();
        ProfileRobot
            .SetProfileName(CUSTOM_SETTINGS_PROFILE_NAME)
            .SelectCountry(COUNTRY_NAME)
            .SelectCity(CITY_NAME)
            .ExpandSettingsSection()
            .SelectNetShieldMode(NetShieldMode.BlockAdsMalwareTrackersAdultContent)
            .SelectPortForwarding(true)
            .SelectProtocol(CUSTOM_SETTINGS_PROTOCOL);
        SaveProfile();

        SidebarRobot
            .ScrollToProfile(CUSTOM_SETTINGS_PROFILE_NAME)
            .Verify.DoesConnectionItemExist(CUSTOM_SETTINGS_PROFILE_NAME)
            .ConnectToProfile(CUSTOM_SETTINGS_PROFILE_NAME);

        HomeRobot
            .Verify.IsConnecting()
                   .IsConnected()
                   .ConnectionCardTitleEquals(CUSTOM_SETTINGS_PROFILE_NAME)
                   .ConnectionCardDescriptionContains(_connectionCardDescription);
        FeaturesRobot
            .Verify.IsPortForwardingEnabled();
        HomeRobot
            .Verify.IsProtocolDisplayed(CUSTOM_SETTINGS_PROTOCOL);

        SettingRobot
            .Verify.IsNetshieldBlocking(NetShieldMode.BlockAdsMalwareTrackersAdultContent)
            .OpenSettings()
            .Verify.IsProfileTaglineDisplayed(CUSTOM_SETTINGS_PROFILE_NAME);

        //TODO: The map highlights the country of the server;
    }

    [Test, Order(10)]
    [Property("TestCaseId", "602437")]
    [Retry(3)]
    [TestCaseSource(nameof(_profiles))]
    public void ConnectToDifferentProfilesWithDifferentConnectionTypesAndProtocols((string profileName, ConnectionType connectionType, Country countryName, Protocol protocol) profile)
    {
        CloseLeftoverProfilePage();

        SidebarRobot
            .NavigateToProfiles();

        CreateProfile(profile.profileName, profile.connectionType, profile.countryName, profile.protocol);

        SidebarRobot
            .ConnectToProfile(profile.profileName);

        HomeRobot
            .Verify.IsConnected()
                   .ConnectionCardTitleEquals(profile.profileName)
                   .ConnectionCardDescriptionContains(profile.countryName.GetName())
                   .IsProtocolDisplayed(profile.protocol);

        if (profile.connectionType == ConnectionType.P2P)
        {
            HomeRobot
                .Verify.IsP2PConnection();
        }

        if (profile.connectionType == ConnectionType.SecureCore)
        {
            HomeRobot.Verify
                .ConnectionCardDescriptionContains(TestConstants.ViaPrefix);
        }

        //TODO: The map highlights the country of the server;
    }

    private void QuickCreateProfile(string profileName)
    {
        SidebarRobot
            .ClickCreateProfile();
        ProfileRobot
            .SetProfileName(profileName);
        SaveProfile();
        SidebarRobot
            .ScrollToProfile(PROFILE_NAME);
    }

    private void CreateProfile(string profileName, ConnectionType connectionType, Country country, Protocol protocol)
    {
        SidebarRobot
            .ClickCreateProfile();
        NavigationRobot
            .Verify.IsOnProfilePage();
        ProfileRobot
            .SetProfileName(profileName)
            .SelectConnectionType(connectionType)
            .SelectCountry(country)
            .ExpandSettingsSection()
            .SelectProtocol(protocol);
        SaveProfile();

        SidebarRobot
            .ScrollToProfile(profileName)
            .Verify.DoesConnectionItemExist(profileName);
    }

    private void RemoveProfiles()
    {
        int profilesCount = SidebarRobot.GetProfileCount();
        for (int profileIndex = 0; profileIndex < profilesCount; profileIndex++)
        {
            SidebarRobot
                .ExpandFirstSecondaryActions()
                .DeleteProfile();

            ConfirmationRobot
                .PrimaryAction();

            // Wait for profile to be deleted
            Thread.Sleep(TestConstants.AnimationDelay);
        }
    }

    private void SaveProfile()
    {
        Thread.Sleep(TestConstants.AnimationDelay);
        ProfileRobot
            .SaveProfile();
        Thread.Sleep(TestConstants.AnimationDelay);
    }

    private void CloseLeftoverProfilePage()
    {
        try
        {
            ProfileRobot.CloseProfile();
            ConfirmationRobot.PrimaryAction();
        }
        catch (TimeoutException)
        {
            //do nothing
        }
    }

    [TearDown]
    public void TearDown()
    {
        Keyboard.Press(VirtualKeyShort.ESCAPE);
        try
        {
            ConfirmationRobot
                .Verify.IsOverlayDisplayed()
                .CancelAction();
        }
        catch { }
        BrowserUtils.KillAllBrowsers();
    }
}
