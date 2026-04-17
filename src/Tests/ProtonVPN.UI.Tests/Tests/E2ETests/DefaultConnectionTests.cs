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
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Extensions;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
public class DefaultConnectionTests : BaseTest
{
    private const string COUNTRY_TO_SEARCH = "Australia";
    private const string FASTEST_COUNTRY = "Fastest country";
    private const string STREAMING_PROFILE = "Streaming US";
    private const string STREAMING_COUNTRY = "United States";
    private const string DEFAULT_CONNECTION = "Default connection";

    private const string EXCLUDED_LOCATION_AFGHANISTAN = "Afghanistan";
    private const string EXCLUDED_LOCATION_SEARCH_QUERY = "U";
    private const string EXCLUDED_LOCATION_UNITED_STATES = "United States";

    [OneTimeSetUp]
    public void SetUp()
    {
        LaunchApp();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test, Order(0)]
    public void DefaultConnectionTitleIsFastest()
    {
        HomeRobot
            .Verify.ConnectionCardTitleEquals(FASTEST_COUNTRY)
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
            .ConnectionCardTitleEquals(FASTEST_COUNTRY)
            .Disconnect()
            .Verify.IsDisconnected();

        ConfirmationRobot.DismissExcludedLocationsPrompt();
    }

    [Test, Order(1)]
    public void DefaultLastConnectionConnectsToCorrectServer()
    {
        SidebarRobot
            .SearchFor(COUNTRY_TO_SEARCH)
            .ConnectToCountry(COUNTRY_TO_SEARCH);

        HomeRobot
            .Verify.IsConnected()
            .ConnectionCardTitleEquals(COUNTRY_TO_SEARCH);
        NetworkUtils.VerifyUserIsConnectedToExpectedCountry(COUNTRY_TO_SEARCH);

        HomeRobot.Disconnect()
            .Verify.IsDisconnected()
            .ConnectionCardTitleEquals(FASTEST_COUNTRY);

        SettingRobot
            .OpenSettings()
            .OpenConnectionPreferencesSettingsCard()
            .SelectLastConnectionOption()
            .ApplySettings()
            .CloseSettings();

        HomeRobot
            .Verify.ConnectionCardTitleEquals(COUNTRY_TO_SEARCH)
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
            .Disconnect()
            .Verify.IsDisconnected();
    }

    [Test]
    public void DefaultConnection()
    {
        HomeRobot
            .Verify.IsDisconnected();

        SettingRobot
            .OpenSettings()
            .OpenConnectionPreferencesSettingsCard()
            .SelectDefaultConnectionType(VpnConnectionOptions.Fast)
            .SelectDefaultConnectionType(VpnConnectionOptions.Random)
            .SelectDefaultConnectionType(VpnConnectionOptions.Last);
    }

    [Test, Order(2)]
    public void ExcludedLocationsSelector_AllowsSelectingAndSearching()
    {
        SettingRobot
            .OpenSettings()
            .OpenConnectionPreferencesSettingsCard()
            .OpenExcludedLocationsSelector()
            .SelectExcludedCountry(EXCLUDED_LOCATION_AFGHANISTAN)
            .Verify.IsRemoveExcludedLocationButtonDisplayed()
            .Verify.IsExcludedLocationDisplayed(EXCLUDED_LOCATION_AFGHANISTAN)
            .OpenExcludedLocationsSelector()
            .SearchExcludedLocations(EXCLUDED_LOCATION_SEARCH_QUERY)
            .SelectExcludedCountry(EXCLUDED_LOCATION_UNITED_STATES)
            .Verify.IsExcludedLocationDisplayed(EXCLUDED_LOCATION_UNITED_STATES)
            .RemoveFirstExcludedLocation()
            .Verify.IsExcludedLocationNotDisplayed(EXCLUDED_LOCATION_AFGHANISTAN)
            .RemoveFirstExcludedLocation()
            .CloseSettings();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Cleanup();
    }
}