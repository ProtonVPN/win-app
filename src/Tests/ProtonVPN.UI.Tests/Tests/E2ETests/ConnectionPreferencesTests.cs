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

using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.UI.Tests.Enums.Locations;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
public class ConnectionPreferencesTests : FreshSessionSetUp
{
    private const Country COUNTRY_TO_SEARCH = Country.Australia;
    private const Country EXCLUDED_LOCATION_AFGHANISTAN = Country.Afghanistan;
    private const Country EXCLUDED_LOCATION_UNITED_STATES = Country.UnitedStates;

    private static readonly string _excludedLocationSearchQuery = Country.UnitedStates.GetName().FirstCharToUpper();

    private static readonly string _fastestCountry = LanguageHelper.GetTranslatedString("Country_Fastest");
    private static readonly string _randomCountry = LanguageHelper.GetTranslatedString("Country_Random");

    [SetUp]
    public void SetUp()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test, Order(0)]
    [Property("TestCaseId", "867494")]
    [Retry(3)]
    public void DefaultConnectionTitleIsFastest()
    {
        HomeRobot
            .Verify.ConnectionCardTitleEquals(_fastestCountry)
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
            .ConnectionCardTitleEquals(_fastestCountry)
            .Disconnect()
            .Verify.IsDisconnected();
    }

    [Test, Order(1)]
    [Property("TestCaseId", "867496")]
    public void DefaultLastConnectionConnectsToCorrectServer()
    {
        SidebarRobot
            .SearchFor(COUNTRY_TO_SEARCH.GetName())
            .ConnectToCountry(COUNTRY_TO_SEARCH);

        HomeRobot
            .Verify.IsConnected()
            .ConnectionCardTitleEquals(COUNTRY_TO_SEARCH.GetName());
        NetworkUtils.VerifyUserIsConnectedToExpectedCountry(COUNTRY_TO_SEARCH);

        HomeRobot.Disconnect()
            .Verify.IsDisconnected()
            .ConnectionCardTitleEquals(_fastestCountry);

        SettingRobot
            .OpenSettings()
            .OpenConnectionPreferencesSettingsCard()
            .SelectLastConnectionOption()
            .ApplySettings()
            .CloseSettings();

        HomeRobot
            .Verify.ConnectionCardTitleEquals(COUNTRY_TO_SEARCH.GetName())
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
            .Disconnect()
            .Verify.IsDisconnected();
    }

    [Test, Order(2)]
    [Property("TestCaseId", "867492")]
    public void DefaultConnectionUpdatesConnectionCardTitle()
    {
        SidebarRobot
            .SearchFor(COUNTRY_TO_SEARCH.GetName())
            .ConnectToCountry(COUNTRY_TO_SEARCH);
        HomeRobot
            .Verify.IsConnected()
            .Disconnect()
            .Verify.IsDisconnected();

        ChooseDefaultConnectionFromSettingsAndVerifyConnectionCardTitle(VpnConnectionOption.Last, COUNTRY_TO_SEARCH.GetName());
        ChooseDefaultConnectionFromSettingsAndVerifyConnectionCardTitle(VpnConnectionOption.Random, _randomCountry);
        ChooseDefaultConnectionFromSettingsAndVerifyConnectionCardTitle(VpnConnectionOption.Fastest, _fastestCountry);
    }

    [Test, Order(3)]
    [Property("TestCaseId", "867493")]
    public void ConnectToVpnFastestCountryAndRandomCountry()
    {
        ConnectToDefaultConnectionAndVerify(VpnConnectionOption.Fastest, _fastestCountry);
        ConnectToDefaultConnectionAndVerify(VpnConnectionOption.Random, _randomCountry);
    }

    [Test, Order(4)]
    [Property("TestCaseId", "867497")]
    public void AllowSelectingAndSearchingTheExcludedLocationsSelector()
    {
        SettingRobot
            .OpenSettings()
            .OpenConnectionPreferencesSettingsCard()
            .OpenExcludedLocationsSelector()
            .SelectExcludedCountry(EXCLUDED_LOCATION_AFGHANISTAN)
            .Verify.IsRemoveExcludedLocationButtonDisplayed()
            .Verify.IsExcludedLocationDisplayed(EXCLUDED_LOCATION_AFGHANISTAN)
            .OpenExcludedLocationsSelector()
            .SearchExcludedLocations(_excludedLocationSearchQuery)
            .SelectExcludedCountry(EXCLUDED_LOCATION_UNITED_STATES)
            .Verify.IsExcludedLocationDisplayed(EXCLUDED_LOCATION_UNITED_STATES)
            .RemoveFirstExcludedLocation()
            .Verify.IsExcludedLocationNotDisplayed(EXCLUDED_LOCATION_AFGHANISTAN)
            .RemoveFirstExcludedLocation()
            .CloseSettings();
    }

    private void ConnectToDefaultConnectionAndVerify(VpnConnectionOption vpnConnectionOption, string expectedConnectionCardTitle)
    {
        HomeRobot
            .SelectDefaultConnectionOption(vpnConnectionOption)
            .ConnectViaConnectionCard()
            .Verify.ConnectionCardTitleEquals(expectedConnectionCardTitle)
                   .IsConnected()
            .Disconnect()
            .Verify.IsDisconnected();
    }

    private void ChooseDefaultConnectionFromSettingsAndVerifyConnectionCardTitle(VpnConnectionOption vpnConnectionOption, string expectedConnectionCardTitle)
    {
        SettingRobot
           .OpenSettings()
           .OpenConnectionPreferencesSettingsCard()
           .SelectDefaultConnectionOption(vpnConnectionOption)
           .ApplySettings()
           .CloseSettings();
        HomeRobot
            .Verify.ConnectionCardTitleEquals(expectedConnectionCardTitle);
    }
}