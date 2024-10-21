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
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
public class ConnectionTests : TestSession
{
    private LoginRobot _loginRobot = new();
    private HomeRobot _homeRobot = new();
    private NavigationRobot _navigationRobot = new();
    private SidebarRobot _sidebarRobot = new();

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();

        _loginRobot
            .Login(TestUserData.PlusUser);

        _navigationRobot
            .Verify.IsOnMainPage()
                   .IsOnHomePage();

        _homeRobot
            .DismissWelcomeModal();
    }

    [Test]
    public void QuickConnect()
    {
        _homeRobot
            .QuickConnect()
            .Verify.IsConnected();
    }

    [Test]
    public void ConnectToCountry()
    {
        const string country = "Australia";

        _navigationRobot
            .Verify.IsOnConnectionsPage();
        _sidebarRobot
            .NavigateToAllCountriesTab();
        _navigationRobot
            .Verify.IsOnCountriesPage();
        _sidebarRobot
            .ConnectToCountry(country);
        _homeRobot
            .Verify.IsConnected();
        _sidebarRobot
            .DisconnectFromCountry(country);
        _homeRobot
            .Verify.IsDisconnected();
    }

    [TearDown]
    public void TestCleanup()
    {
        Cleanup();
    }
}