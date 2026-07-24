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
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.Enums.Locations;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
[Category("ARM")]
[Category("SMOKE_1")]
public class SearchTests : FreshSessionSetUp
{
    private const Country COUNTRY_TO_SEARCH = Country.UnitedStates;
    private const State STATE = State.Arizona;
    private const City CITY = City.Berlin;
    private const string SERVER = "FR#223";

    private static readonly string _trySearchingForText = LanguageHelper.GetTranslatedString("Search_NoInput_TrySearchingFor");

    [SetUp]
    public void SetUp()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    [Property("TestCaseId", "602383")]
    public void SearchForCountryConnectAndDisconnect()
    {
        SidebarRobot
            .SearchFor(COUNTRY_TO_SEARCH.GetName())
            .ConnectToCountry(COUNTRY_TO_SEARCH);

        HomeRobot
            .Verify.IsConnected();

        SidebarRobot
            .DisconnectViaCountry(COUNTRY_TO_SEARCH);

        HomeRobot
            .Verify.IsDisconnected();
    }

    [Test]
    [Property("TestCaseId", "602382")]
    public void SearchForCountryAndConnectDisconnectToCity()
    {
        SidebarRobot
            .SearchFor(COUNTRY_TO_SEARCH.GetName())
            .ExpandCities(COUNTRY_TO_SEARCH)
            .ConnectToState(STATE);

        HomeRobot
            .Verify.IsConnected();

        SidebarRobot
            .DisconnectViaState(STATE);

        HomeRobot
            .Verify.IsDisconnected();
    }

    [Test]
    [Property("TestCaseId", "602381")]
    public void SearchCountryAndConnectDisconnectToSpecificServer()
    {
        SidebarRobot
            .SearchFor(COUNTRY_TO_SEARCH.GetName())
            .ExpandCities(COUNTRY_TO_SEARCH)
            .ExpandSpecificServerList()
            .ConnectToServer();

        HomeRobot
            .Verify.IsConnected();

        SidebarRobot
            .ExpandSpecificServerList()
            .DisconnectViaServer();

        HomeRobot
            .Verify.IsDisconnected();
    }

    [Test]
    [Property("TestCaseId", "610985")]
    public void SearchForCityAndConnectDisconnect()
    {
        SidebarRobot
            .SearchFor(CITY.GetEnumValue())
            .ConnectToCity(CITY);

        HomeRobot
            .Verify.IsConnected();

        SidebarRobot
            .DisconnectViaCity(CITY);

        HomeRobot
            .Verify.IsDisconnected();
    }

    [Test]
    [Property("TestCaseId", "610986")]
    public void SearchForServerAndConnectDisconnect()
    {
        SidebarRobot
            .SearchFor(SERVER)
            .ConnectToServer(SERVER);

        HomeRobot
            .Verify.IsConnected();

        SidebarRobot
            .DisconnectViaServer(SERVER);

        HomeRobot
            .Verify.IsDisconnected();
    }

    [Test]
    [Property("TestCaseId", "610987")]
    public void SearchBehaviorAfterRemovingFocus()
    {
        SidebarRobot
            .ClickSearchBox()
            .Verify.IsSearchBoxFocused()
                   .SidebarSearchResultContains(_trySearchingForText);

        HomeRobot
            .ClickOnConnectionCardTitle();

        SidebarRobot
            .Verify.IsSidebarConnectionsDisplayed()
            .SearchFor(COUNTRY_TO_SEARCH.GetName())
            .Verify.SidebarSearchResultContains(COUNTRY_TO_SEARCH.GetName());

        HomeRobot
            .ClickOnConnectionCardTitle();

        SidebarRobot
            .Verify.SidebarSearchResultContains(COUNTRY_TO_SEARCH.GetName());
    }

    [Test]
    [Property("TestCaseId", "610988")]
    public void SearchBehaviorAfterRemovingText()
    {
        SidebarRobot
            .SearchFor(COUNTRY_TO_SEARCH.GetName())
            .Verify.IsBackButtonInSearchBoxDisplayed()
            .ClickXButtonInSearchBox()
            .Verify.SidebarSearchResultContains(_trySearchingForText)
            .SearchFor(CITY.GetEnumValue())
            .Verify.IsBackButtonInSearchBoxDisplayed()
            .ClickBackButtonInSearchBox()
            .Verify.IsSidebarConnectionsDisplayed();
    }
}
