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
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.Robots.Shell;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("2")]
public class LogoutTests : TestSession
{
    private LoginRobot _loginRobot = new();
    private HomeRobot _homeRobot = new();
    private ShellRobot _shellRobot = new();

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();

        _loginRobot
            .Wait(TestConstants.StartupDelay)
            .DoLogin(TestUserData.PlusUser);

        _homeRobot
            .DoCloseWelcomeOverlay()
            .DoWaitForVpnStatusSubtitleLabel()
            .VerifyVpnStatusIsDisconnected()
            .VerifyConnectionCardIsInInitalState();
    }

    [Test]
    public void LogoutUser()
    {
        _shellRobot
            .DoOpenAccount()
            .DoSignOut()
            .VerifySignOutConfirmationMessage(TestUserData.PlusUser)
            .DoClickOverlayMessagePrimaryButton();

        _loginRobot
            .VerifyIsInLoginWindow();
    }

    [Test]
    public void LogoutWhileConnected()
    {
        string unprotectedIpAddress = NetworkUtils.GetIpAddress();

        _homeRobot
            .DoConnect()
            .VerifyAllStatesUntilConnected();

        CommonAssertions.AssertIpAddressChanged(unprotectedIpAddress);

        _shellRobot
            .DoOpenAccount()
            .DoSignOut()
            .VerifySignOutConfirmationMessage(TestUserData.PlusUser)
            .DoClickOverlayMessageCloseButton();

        // Signout canceled, verify that we are still connected to the VPN
        _homeRobot
            .VerifyVpnStatusIsConnected();

        _shellRobot
            .DoOpenAccount()
            .DoSignOut()
            .VerifySignOutConfirmationMessage(TestUserData.PlusUser)
            .DoClickOverlayMessagePrimaryButton()
            .Wait(TestConstants.DisconnectionDelay);

        _loginRobot
            .VerifyIsInLoginWindow();

        // Check that we have been properly disconnected from the VPN during signout
        CommonAssertions.AssertIpAddressUnchanged(unprotectedIpAddress);
    }

    [TearDown]
    public void TestCleanup()
    {
        Cleanup();
    }
}