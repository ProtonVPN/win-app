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
using ProtonVPN.UI.Tests.Robots.Overlays;
using ProtonVPN.UI.Tests.Robots.Shell;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests;

[TestFixture]
[Category("UI")]
public class FreeConnectionTests : TestSession
{
    private const string SERVER_LOAD_OVERLAY_TITLE = "What is server load?";
    private const string LATENCY_OVERLAY_TITLE = "What is latency?";
    private const string PROTOCOL_OVERLAY_TITLE = "What is a VPN protocol?";

    private const string FREE_CONNECTIONS_OVERLAY_TITLE = "Free connections";

    private ShellRobot _shellRobot = new();
    private HomeRobot _homeRobot = new();
    private OverlaysRobot _overlaysRobot = new();
    private LoginRobot _loginRobot = new();

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();

        _loginRobot
            .Wait(TestConstants.StartupDelay)
            .DoLogin(TestUserData.FreeUser);

        _homeRobot
            .DoWaitForVpnStatusSubtitleLabel()
            .VerifyVpnStatusIsDisconnected()
            .VerifyConnectionCardIsInInitalStateForFreeUser();

        //TODO When reconnection logic is implemented remove this sleep.
        //Certificate sometimes takes longer to get and app does not handle it yet
        _shellRobot
            .Wait(TestConstants.InitializationDelay);
    }

    [Test]
    public async Task FreeConnectAndDisconnectAsync()
    {
        string unprotectedIpAddress = await CommonAssertions.GetCurrentIpAddressAsync();

        _homeRobot
            .DoConnect()
            .VerifyAllStatesUntilConnectedToFreeServer();

        await CommonAssertions.AssertIpAddressChangedAsync(unprotectedIpAddress);

        _homeRobot
            .DoDisconnect()
            .VerifyVpnStatusIsDisconnected()
            .VerifyConnectionCardIsInInitalStateForFreeUser()
            .Wait(TestConstants.DisconnectionDelay);

        await CommonAssertions.AssertIpAddressUnchangedAsync(unprotectedIpAddress);
    }

    [Retry(3)]
    [Test]
    public async Task FreeConnectAndCancelAsync()
    {
        string unprotectedIpAddress = await CommonAssertions.GetCurrentIpAddressAsync();

        _homeRobot
            .DoConnect()
            .VerifyVpnStatusIsConnecting()
            .VerifyConnectionCardIsConnectingToFreeServer()
            //Imitate user's delay
            .Wait(1000)
            .DoCancelConnection()
            .VerifyVpnStatusIsDisconnected()
            .VerifyConnectionCardIsInInitalStateForFreeUser()
            .Wait(TestConstants.DisconnectionDelay);

        await CommonAssertions.AssertIpAddressUnchangedAsync(unprotectedIpAddress);
    }

    [Test]
    public async Task FreeConnectAndChangeServerAsync()
    {
        string unprotectedIpAddress = await CommonAssertions.GetCurrentIpAddressAsync();

        _homeRobot
            .DoConnect()
            .VerifyAllStatesUntilConnectedToFreeServer();

        await CommonAssertions.AssertIpAddressChangedAsync(unprotectedIpAddress);

        string protectedIpAddress = await CommonAssertions.GetCurrentIpAddressAsync();

        _homeRobot
            .DoChangeServer()
            .VerifyVpnStatusIsConnecting()
            .VerifyVpnStatusIsConnected();

        await CommonAssertions.AssertIpAddressChangedAsync(unprotectedIpAddress);
        await CommonAssertions.AssertIpAddressChangedAsync(protectedIpAddress);

        _homeRobot
            .DoChangeServer();
        
        _overlaysRobot
            .VerifyOverlayIsOpened(string.Empty, false)
            .VerifyChangeServerCountdownProgressRing()
            .DoCloseOverlay();

        _homeRobot
            .DoDisconnect()
            .VerifyVpnStatusIsDisconnected()
            .VerifyConnectionCardIsInInitalStateForFreeUser()
            .Wait(TestConstants.DisconnectionDelay);

        await CommonAssertions.AssertIpAddressUnchangedAsync(unprotectedIpAddress);
    }

    [Test]
    public void OpenFreeConnectionDetails()
    {
        _homeRobot
            .DoConnect()
            .VerifyAllStatesUntilConnectedToFreeServer()
            .DoOpenConnectionDetails()
            .VerifyConnectionDetailsIsOpened();

        // TODO: Latency is kept hidden as it is not implemented yet. 
        //_homeRobot
        //    .DoOpenLatencyOverlay();

        //_overlaysRobot
        //    .VerifyOverlayIsOpened(LATENCY_OVERLAY_TITLE, true)
        //    .DoCloseOverlay();

        _homeRobot
            .DoOpenServerLoadOverlay();

        _overlaysRobot
            .VerifyOverlayIsOpened(SERVER_LOAD_OVERLAY_TITLE, true)
            .DoCloseOverlay();

        _homeRobot
            .DoOpenProtocolOverlay();

        _overlaysRobot
            .VerifyOverlayIsOpened(PROTOCOL_OVERLAY_TITLE, true)
            .DoCloseOverlay();

        _homeRobot
            .DoCloseConnectionDetails();
    }

    [Test]
    public void OpenFreeConnectionsAbout()
    {
        _homeRobot
            .DoOpenConnectionDetails();

        _overlaysRobot
            .VerifyOverlayIsOpened(FREE_CONNECTIONS_OVERLAY_TITLE, false)
            .DoCloseOverlay();
    }

    [TearDown]
    public void TestCleanup()
    {
        Cleanup();
    }
}