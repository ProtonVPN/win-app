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

using System.Drawing;
using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.UiTools;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.UI.Tests.Enums.Locations;
using ProtonVPN.UI.Tests.TestsHelper.TestData;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
public class ConnectionPreferencesTests : FreshSessionSetUp
{
    private const Country COUNTRY_TO_SEARCH = Country.Australia;
    private const Country EXCLUDED_LOCATION_AFGHANISTAN = Country.Afghanistan;
    private const Country EXCLUDED_LOCATION_UNITED_STATES = Country.UnitedStates;

    private const Country COUNTRY_NAME = Country.Belgium;

    private const Country COUNTRY_NAME_WITH_CITY = Country.Austria;
    private const City CITY_NAME = City.Vienna;

    private const Country COUNTRY_NAME_WITH_SERVER = Country.Australia;

    private const Country SECURE_CORE_COUNTRY_NAME = Country.Argentina;
    private const Country VIA_COUNTRY_SWITZERLAND = Country.Switzerland;

    private const DefaultProfile PROFILE_NAME = DefaultProfile.StreamingUS;

    private static readonly Country[] _balkanCountries = [Country.Macedonia, Country.Greece, Country.Albania, Country.Bulgaria, Country.Serbia, Country.Kosovo, Country.Montenegro];
    private static readonly Country[] _euCountries = [Country.Austria, Country.Germany, Country.Hungary, Country.Lithuania, Country.France, Country.Poland];
    private static readonly Country[] _neighbouringCountries = TestEnvironment.AreTestsRunningLocally() ? _balkanCountries : _euCountries;

    private static readonly string _excludedLocationSearchQuery = Country.UnitedStates.GetName().FirstCharToUpper();

    private static readonly string _fastestCountry = LanguageHelper.GetTranslatedString("Country_Fastest");
    private static readonly string _randomCountry = LanguageHelper.GetTranslatedString("Country_Random");

    [SetUp]
    public void SetUp()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
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

    [Test]
    [Property("TestCaseId", "867496")]
    [Retry(3)]
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

    [Test]
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

    [Test]
    [Property("TestCaseId", "867493")]
    public void ConnectToVpnFastestCountryAndRandomCountry()
    {
        ConnectToDefaultConnectionAndVerify(VpnConnectionOption.Fastest, _fastestCountry);
        ConnectToDefaultConnectionAndVerify(VpnConnectionOption.Random, _randomCountry);
    }

    [Test]
    [Property("TestCaseId", "867497")]
    [Retry(3)]
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

    [Test]
    [Property("TestCaseId", "890307")]
    public void ConnectionPreferencesListUpdatesHome()
    {
        PopulateRecents();

        HomeRobot
            .Verify.ConnectionPreferencesDropdownContains([
                COUNTRY_NAME.GetName(),
                CITY_NAME.GetEnumValue(),
                COUNTRY_NAME_WITH_SERVER.GetCode(),
                SECURE_CORE_COUNTRY_NAME.GetName(),
                VIA_COUNTRY_SWITZERLAND.GetName(),
                PROFILE_NAME.GetEnumValue()]);

        SidebarRobot
            .NavigateToRecents()
            .ExpandSecondaryActionsForRecents(COUNTRY_NAME.GetName())
            .RemoveRecent();
        Thread.Sleep(TestConstants.UserInputSimulationDelay);
        SidebarRobot
            .ExpandSecondaryActionsForRecents(SECURE_CORE_COUNTRY_NAME.GetName())
            .RemoveRecent();

        HomeRobot
            .Verify.ConnectionPreferencesDropdownContains([
                CITY_NAME.GetEnumValue(),
                COUNTRY_NAME_WITH_SERVER.GetCode(),
                PROFILE_NAME.GetEnumValue()])
            .Verify.ConnectionPreferencesDropdownDoesNotContain([
                COUNTRY_NAME.GetName(),
                SECURE_CORE_COUNTRY_NAME.GetName(),
                VIA_COUNTRY_SWITZERLAND.GetName()]);
    }

    [Test]
    [Property("TestCaseId", "890306")]
    public void ConnectionPreferencesListUpdatesSettings()
    {
        PopulateRecents();

        SettingRobot
            .OpenSettings()
            .OpenConnectionPreferencesSettingsCard();
        HomeRobot
            .Verify.ConnectionPreferencesDropdownContains([
                COUNTRY_NAME.GetName(),
                CITY_NAME.GetEnumValue(),
                COUNTRY_NAME_WITH_SERVER.GetCode(),
                SECURE_CORE_COUNTRY_NAME.GetName(),
                VIA_COUNTRY_SWITZERLAND.GetName(),
                PROFILE_NAME.GetEnumValue()], isOnConnectionPreferencesPage: true);
        SettingRobot
            .CloseSettings();
        SidebarRobot
            .NavigateToRecents()
            .ExpandSecondaryActionsForRecents(PROFILE_NAME.GetEnumValue())
            .RemoveRecent();
        Thread.Sleep(TestConstants.UserInputSimulationDelay);
        SidebarRobot
            .ExpandSecondaryActionsForRecents(COUNTRY_NAME_WITH_CITY.GetEnumValue())
            .RemoveRecent();

        SettingRobot
            .OpenSettings()
            .OpenConnectionPreferencesSettingsCard();
        HomeRobot
            .Verify.ConnectionPreferencesDropdownContains([
                COUNTRY_NAME.GetName(),
                COUNTRY_NAME_WITH_SERVER.GetCode(),
                SECURE_CORE_COUNTRY_NAME.GetName(),
                VIA_COUNTRY_SWITZERLAND.GetName()], isOnConnectionPreferencesPage: true)
            .Verify.ConnectionPreferencesDropdownDoesNotContain([
                CITY_NAME.GetEnumValue(),
                PROFILE_NAME.GetEnumValue()], isOnConnectionPreferencesPage: true);
    }

    [Test]
    [Property("TestCaseId", "890305")]
    public void ExcludedCountriesAreRespected()
    {
        SettingRobot
            .OpenSettings()
            .OpenConnectionPreferencesSettingsCard();

        foreach (Country country in _neighbouringCountries)
        {
            SettingRobot
                .OpenExcludedLocationsSelector()
                .SearchExcludedLocations(country.GetName())
                .SelectExcludedCountry(country)
                .Verify.IsExcludedLocationDisplayed(country);
        }

        SettingRobot
            .ApplySettings()
            .CloseSettings();

        for (int i = 0; i < 5; i++)
        {
            HomeRobot
                .ConnectViaConnectionCard()
                .Verify.IsConnected()
                .ConnectionCardDescriptionDoesNotContainOneOf(_neighbouringCountries)
                .Disconnect()
                .Verify.IsDisconnected();
            Thread.Sleep(TestConstants.UserInputSimulationDelay);
        }
    }

    [Test]
    [Property("TestCaseId", "890304")]
    public void SearchingExcludedLocationsWithSpecialChars()
    {
        SettingRobot
            .OpenSettings()
            .OpenConnectionPreferencesSettingsCard()
            .OpenExcludedLocationsSelector()
            .SearchExcludedLocations("------", useKeyboard: false);

        (Point Position, Size Size) elementBeforeTyping = UiActions.GetElementSizeAndPosition(SettingRobot.ExcludedLocationFlyout);

        foreach (string specialChar in InputTestData.SpecialInputs)
        {
            UiActions.VerifyElementSizeAndPosition(SettingRobot.ExcludedLocationFlyout, elementBeforeTyping, () =>
            SettingRobot
                .SearchExcludedLocations(specialChar, useKeyboard: false)
            );
            SettingRobot.Verify.IsNoLocationsAvailableDisplayed();
            UiActions.ClearInputWithKeyboard();
        }
    }

    [Test]
    [Property("TestCaseId", "890304")]
    [Ignore("JIRA - VPNWIN-3343")]
    public void SearchingExcludedLocationsWithMaxChars()
    {
        SettingRobot
            .OpenSettings()
            .OpenConnectionPreferencesSettingsCard()
            .OpenExcludedLocationsSelector()
            .SearchExcludedLocations("------", useKeyboard: false);

        (Point Position, Size Size) elementBeforeTyping = UiActions.GetElementSizeAndPosition(SettingRobot.ExcludedLocationFlyout);

        UiActions.VerifyElementSizeAndPosition(SettingRobot.ExcludedLocationFlyout, elementBeforeTyping, () =>
            SettingRobot
                .SearchExcludedLocations(InputTestData.LongInput, useKeyboard: false)
        );
        SettingRobot.Verify.IsNoLocationsAvailableDisplayed();
        UiActions.ClearInputWithKeyboard();
    }

    private static void PopulateRecents()
    {
        RecentsFlow.PopulateRecentsListWithCountry(COUNTRY_NAME);
        RecentsFlow.PopulateRecentsListWithCity(COUNTRY_NAME_WITH_CITY, CITY_NAME);
        RecentsFlow.PopulateRecentsListWithServer(COUNTRY_NAME_WITH_SERVER);
        RecentsFlow.PopulateRecentsListWithSecureCore(SECURE_CORE_COUNTRY_NAME, VIA_COUNTRY_SWITZERLAND);
        RecentsFlow.PopulateRecentsListWithProfile(PROFILE_NAME.GetEnumValue());
    }

    private static void ConnectToDefaultConnectionAndVerify(VpnConnectionOption vpnConnectionOption, string expectedConnectionCardTitle)
    {
        HomeRobot
            .SelectDefaultConnectionOption(vpnConnectionOption)
            .ConnectViaConnectionCard()
            .Verify.ConnectionCardTitleEquals(expectedConnectionCardTitle)
                   .IsConnected()
            .Disconnect()
            .Verify.IsDisconnected();
    }

    private static void ChooseDefaultConnectionFromSettingsAndVerifyConnectionCardTitle(VpnConnectionOption vpnConnectionOption, string expectedConnectionCardTitle)
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