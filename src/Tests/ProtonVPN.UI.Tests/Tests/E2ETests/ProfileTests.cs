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
public class ProfileTests : TestSession
{
    private LoginRobot _loginRobot = new();
    private HomeRobot _homeRobot = new();
    private NavigationRobot _navigationRobot = new();
    private SidebarRobot _sidebarRobot = new();
    private ProfileRobot _profileRobot = new();

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
    public void CreateFastestProfile()
    {
        const string profileName = "Profile A";

        _navigationRobot
            .Verify.IsOnConnectionsPage();
        _sidebarRobot
            .NavigateToProfiles();
        _navigationRobot
            .Verify.IsOnProfilesPage();

        _sidebarRobot
            .CreateProfile();
        _profileRobot
            .Verify.IsProfileOverlayDisplayed()
            .SetProfileName(profileName)
            .SaveProfile();
        _sidebarRobot
            .Verify.ConnectionItemExists(profileName);
    }

    [TearDown]
    public void TestCleanup()
    {
        Cleanup();
    }
}
