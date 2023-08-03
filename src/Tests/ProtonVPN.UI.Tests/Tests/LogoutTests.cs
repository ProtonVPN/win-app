/*
 * Copyright (c) 2023 Proton AG
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
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests;

[TestFixture]
[Category("UI")]
public class LogoutTests : TestSession
{
    private const string FREE_PLAN_NAME = "Proton VPN Free";

    private LoginTests _loginTests = new();
    private LoginRobot _loginRobot = new();
    private HomeRobot _homeRobot = new();

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();
    }

    [Test]
    public void LogoutFreeUser()
    {
        _loginTests
            .LoginWithUser(TestUserData.FreeUser, FREE_PLAN_NAME);

        _homeRobot
            .DoSignOut();

        _loginRobot
            .VerifyIsInLoginWindow();
    }

    [TearDown]
    public void TestCleanup()
    {
        Cleanup();
    }
}