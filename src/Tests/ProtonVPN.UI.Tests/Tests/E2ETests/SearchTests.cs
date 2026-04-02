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

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
[Category("ARM")]
public class SearchTests : FreshSessionSetUp
{
    private const string COUNTRY_TO_SEARCH = "United States";
    private const string STATE = "Arizona";
    private const string COUNTRY_CODE = "US";

    private const string CITY = "Berlin";
    private const string SERVER = "FR#223";

    private const string TRY_SEARCHING_TEXT = "Try searching for...";

    [SetUp]
    public void SetUp()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    public void SearchForCountryConnectAndDisconnect()
    {
        SidebarRobot
            .SearchFor(COUNTRY_TO_SEARCH)
            .ConnectToCountry(COUNTRY_CODE);

        HomeRobot
            .Verify.IsConnected();

        SidebarRobot
            .DisconnectViaCountry(COUNTRY_CODE);

        HomeRobot
            .Verify.IsDisconnected();
    }

    [Test]
    public void SearchForCountryAndConnectDisconnectToCity()
    {
        SidebarRobot
            .SearchFor(COUNTRY_TO_SEARCH)
            .ExpandCities()
            .ConnectToCity(STATE);

        HomeRobot
            .Verify.IsConnected();

        SidebarRobot
            .DisconnectViaCity(STATE);

        HomeRobot
            .Verify.IsDisconnected();
    }

    [Test]
    public void SearchCountryAndConnectDisconnectToSpecificServer()
    {
        SidebarRobot
            .SearchFor(COUNTRY_TO_SEARCH)
            .ExpandCities()
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
    public void SearchForCityAndConnectDisconnect()
    {
        SidebarRobot
            .SearchFor(CITY)
            .ConnectToCity(CITY);

        HomeRobot
            .Verify.IsConnected();

        SidebarRobot
            .DisconnectViaCity(CITY);

        HomeRobot
            .Verify.IsDisconnected();
    }

    [Test]
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
    public void SearchBehaviorAfterRemovingFocus()
    {
        SidebarRobot
            .ClickSearchBox()
            .Verify.IsSearchBoxFocused()
                   .AssertSidebarSearchResults(TRY_SEARCHING_TEXT);

        HomeRobot
            .ClickOnConnectionCardTitle();

        SidebarRobot
            .Verify.IsSidebarConnectionsDisplayed()
            .SearchFor(COUNTRY_TO_SEARCH)
            .Verify.AssertSidebarSearchResults(COUNTRY_TO_SEARCH);

        HomeRobot
            .ClickOnConnectionCardTitle();

        SidebarRobot
            .Verify.AssertSidebarSearchResults(COUNTRY_TO_SEARCH);
    }

    [Test]
    public void SearchBehaviorAfterRemovingText()
    {
        SidebarRobot
            .SearchFor(COUNTRY_TO_SEARCH)
            .Verify.IsBackBtnInSearchBoxDisplayed()
            .ClickXBtnInSearchBox()
            .Verify.AssertSidebarSearchResults(TRY_SEARCHING_TEXT)
            .SearchFor(CITY)
            .Verify.IsBackBtnInSearchBoxDisplayed()
            .ClickBackBtnInSearchBox()
            .Verify.IsSidebarConnectionsDisplayed();
    }
}
