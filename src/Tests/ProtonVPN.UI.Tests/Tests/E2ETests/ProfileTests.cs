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
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using static ProtonVPN.UI.Tests.TestsHelper.TestConstants;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("2")]
[Category("ARM")]
public class ProfileTests : BaseTest
{
    private const string PROFILE_NAME = "Profile A";
    private const string CUSTOM_SETTINGS_PROFILE_NAME = "Profile C";
    private const Protocol CUSTOM_SETTINGS_PROTOCOL = Protocol.WireGuardTcp;

    private const string COUNTRY_NAME = "Australia";
    private const string CITY_NAME = "Perth";
    private const string CONNECTION_CARD_DESCRIPTION = $"{COUNTRY_NAME} - {CITY_NAME}";

    private const string WEBSITE_PROFILE_NAME = "Open web Profile";
    private const string WEBSITE_TO_OPEN = "youtube.com";
    private const string WEBSITE_TO_ASSERT = "YouTube";

    private const string APP_PROFILE_NAME = "Open app Profile";
    private const string APP_TO_OPEN = "Google Chrome";
    private const string APP_TO_OPEN_PATH = @"C:\Program Files\Google\Chrome\Application\chrome.exe";

    private static readonly string[] _defaultProfiles = { "Streaming US", "Gaming", "P2P", "Max security", "Work/School" };

    private static readonly (string profileName, ConnectionType connectionType, string countryName, TestConstants.Protocol protocol)[] _profiles =
    {
        (profileName: "Profile 1", connectionType: ConnectionType.Standard, countryName: "Argentina", protocol: TestConstants.Protocol.OpenVpnUdp),
        (profileName: "Profile 2", connectionType: ConnectionType.P2P, countryName: "Bosnia and Herzegovina", protocol: TestConstants.Protocol.WireGuardTcp),
        (profileName: "Profile 3", connectionType: ConnectionType.SecureCore, countryName: "Egypt", protocol: TestConstants.Protocol.WireGuardUdp)
    };

    [OneTimeSetUp]
    public void SetUp()
    {
        LaunchApp();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test, Order(0)]
    public void VerifyDefaultProfilesExist()
    {
        NavigationRobot
            .Verify.IsOnConnectionsPage();
        SidebarRobot
            .NavigateToProfiles();

        foreach (string profile in _defaultProfiles)
        {
            SidebarRobot
                .Verify.DoesConnectionItemExist(profile);
        }
    }

    [Test, Order(1)]
    public void EmptyProfileList()
    {
        NavigationRobot
            .Verify.IsOnProfilesPage();

        RemoveProfiles();

        SidebarRobot
            .Verify.NoProfilesLabelIsDisplayed();
    }

    [Test, Order(2)]
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
    public void ConnectToProfileAndDisconnect()
    {
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
    public void EditProfile()
    {
        SidebarRobot
            .ScrollToProfile(PROFILE_NAME)
            .Verify.DoesConnectionItemExist(PROFILE_NAME)
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
    public void DeleteProfile()
    {
        SidebarRobot
            .ScrollToProfile(PROFILE_NAME)
            .Verify.DoesConnectionItemExist(PROFILE_NAME)
            .DisconnectViaProfile(PROFILE_NAME);
        HomeRobot
            .Verify.IsDisconnected();
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

        // Giving it some time for the app to open
        Thread.Sleep(TestConstants.FiveSecondsTimeout);

        DesktopRobot
            .Verify.IsWindowTitlePresent(WEBSITE_TO_ASSERT);

        BrowserUtils.KillAllBrowsers();
    }

    [Test, Order(8)]
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

        // Giving it some time for the app to open
        Thread.Sleep(TestConstants.TenSecondsTimeout);

        DesktopRobot
            .Verify.IsWindowTitlePresent(APP_TO_OPEN);

        BrowserUtils.KillAllBrowsers();
    }

    [Test, Order(9)]
    public void ConnectWithCustomSettings()
    {
        SidebarRobot
            .NavigateToProfiles()
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
                   .ConnectionCardDescriptionContains(CONNECTION_CARD_DESCRIPTION)
                   .IsPortForwardingEnabled()
                   .IsProtocolDisplayed(CUSTOM_SETTINGS_PROTOCOL);

        SettingRobot
            .Verify.IsNetshieldBlocking(NetShieldMode.BlockAdsMalwareTrackersAdultContent);

        //TODO: The map highlights the country of the server;
    }

    [Test, Order(10)]
    [Retry(3)]
    public void ConnectToDifferentProfilesWithDifferentConnectionTypesAndProtocols()
    {
        SidebarRobot
            .NavigateToProfiles();

        foreach ((string profileName, ConnectionType connectionType, string countryName, TestConstants.Protocol protocol) _profile in _profiles)
        {
            CreateProfile(_profile.profileName, _profile.connectionType, _profile.countryName, _profile.protocol);

            SidebarRobot
                .ConnectToProfile(_profile.profileName);

            HomeRobot
                .Verify.IsConnected()
                       .ConnectionCardTitleEquals(_profile.profileName)
                       .ConnectionCardDescriptionContains(_profile.countryName)
                       .IsProtocolDisplayed(_profile.protocol);

            if (_profile.connectionType == ConnectionType.P2P)
            {
                HomeRobot
                    .Verify.IsP2PConnection();
            }

            if (_profile.connectionType == ConnectionType.SecureCore)
            {
                HomeRobot.Verify
                    .ConnectionCardDescriptionContains(" via ");
            }

            //TODO: The map highlights the country of the server;
        }
    }

    private void CreateProfile(string profileName, ConnectionType connectionType, string country, TestConstants.Protocol protocol)
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

    [OneTimeTearDown]
    public void TearDown()
    {
        BrowserUtils.KillAllBrowsers();
        Cleanup();
    }
}
