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

using System.Threading.Tasks;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.Robots.Shell;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests;

[TestFixture]
[Category("UI")]
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
            .DoWaitForVpnStatusSubtitleLabel()
            .VerifyVpnStatusIsDisconnected()
            .VerifyConnectionCardIsInInitalState();

        //TODO When reconnection logic is implemented remove this sleep.
        //Certificate sometimes takes longer to get and app does not handle it yet
        _shellRobot
            .Wait(TestConstants.InitializationDelay);
    }

    [Test]
    public void LogoutUser()
    {
        _homeRobot
            .DoOpenAccount()
            .DoSignOut();

        _loginRobot
            .VerifyIsInLoginWindow();
    }

    [Test]
    public async Task LogoutWhileConnectedAsync()
    {
        string unprotectedIpAddress = await CommonAssertions.GetCurrentIpAddressAsync();

        _homeRobot
            .DoConnect()
            .VerifyAllStatesUntilConnected();

        await CommonAssertions.AssertIpAddressChangedAsync(unprotectedIpAddress);

        string protectedIpAddress = await CommonAssertions.GetCurrentIpAddressAsync();

        _homeRobot
            .DoOpenAccount()
            .DoSignOut();

        _shellRobot
            .VerifySignOutConfirmationMessage(TestUserData.PlusUser)
            .DoClickOverlayMessageCloseButton();

        // Signout canceled, verify that we are still connected to the VPN
        await CommonAssertions.AssertIpAddressUnchangedAsync(protectedIpAddress);

        _homeRobot
            .DoOpenAccount()
            .DoSignOut();

        _shellRobot
            .VerifySignOutConfirmationMessage(TestUserData.PlusUser)
            .DoClickOverlayMessagePrimaryButton()
            .Wait(TestConstants.DisconnectionDelay);

        _loginRobot
            .VerifyIsInLoginWindow();

        // Check that we have been properly disconnected from the VPN during signout
        await CommonAssertions.AssertIpAddressUnchangedAsync(unprotectedIpAddress);
    }

    [TearDown]
    public void TestCleanup()
    {
        Cleanup();
    }
}