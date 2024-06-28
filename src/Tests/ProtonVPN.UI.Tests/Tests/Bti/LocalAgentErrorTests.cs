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

using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ProtonVPN.UI.Tests.ApiClient.TestEnv;
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.Bti;

[TestFixture]
[Category("BTI")]
public class LocalAgentErrorTests : TestSession
{
    private LoginRobot _loginRobot = new();
    private HomeRobot _homeRobot = new();

    private const string MAX_SESSIONS_MESSAGE = "You have reached your maximum device limit. Please disconnect another device to connect this one.";
    private string _ip_me_url_bti = Environment.GetEnvironmentVariable("IP_ENDPOINT_BTI");

    [SetUp]
    public async Task TestInitializeAsync()
    {
        BtiController.SetScenarioAsync("reset");
        BtiController.SetScenarioAsync("enable/sessions_un_hardjail_all");
        LaunchApp();
        _loginRobot.DoLogin(TestUserData.PlusUserBti);
        _homeRobot.DoCloseWelcomeOverlay();
    }
    
    [Test]
    [Ignore("VPNWIN-2247")]
    public async Task LocalAgentError86101()
    {
        _homeRobot
            .DoWaitForVpnStatusSubtitleLabel()
            .DoConnect()
            .VerifyVpnStatusIsConnected();

        string certificateBefore = TestEnvironment.GetConnectionCertificate();

        BtiController.SetScenarioAsync(BtiScenarios.HARDJAIL_86101);

        //Give some time for the error to kick in and refresh the cert
        Thread.Sleep(TestConstants.TenSecondsTimeout);

        string certificateAfter = TestEnvironment.GetConnectionCertificate();
        Assert.AreNotEqual(certificateBefore, certificateAfter);
    }

    [Test]
    public async Task LocalAgentError86102()
    {
        await NewCertificateRequiredTest(BtiScenarios.HARDJAIL_86102);
    }

    [Test]
    public async Task LocalAgentError86103()
    {
        await NewCertificateRequiredTest(BtiScenarios.HARDJAIL_86103);
    }

    [Test]
    public async Task LocalAgentError86105()
    {
        await ReconnectionRequiredTestCase(BtiScenarios.HARDJAIL_86105);
    }

    [Test]
    public async Task LocalAgentError86110()
    {
        await DisconnectionRequiredTestCase(BtiScenarios.HARDJAIL_86110);
        _homeRobot.VerifyErrorMessageExists(MAX_SESSIONS_MESSAGE);
    }

    [Test]
    public async Task LocalAgentError86113()
    {
        await DisconnectionRequiredTestCase(BtiScenarios.HARDJAIL_86113);
        _homeRobot.VerifyErrorMessageExists(MAX_SESSIONS_MESSAGE);
    }

    [Test]
    public async Task LocalAgentError86203()
    {
        string currentIpAddress = NetworkUtils.GetIpAddress(_ip_me_url_bti);

        await ReconnectionRequiredTestCase(BtiScenarios.HARDJAIL_86203);
        string ipAddress = NetworkUtils.GetIpAddress(_ip_me_url_bti);

        Assert.AreNotEqual(currentIpAddress, ipAddress, $"Failed to reconnect user to new server. " +
            $"Old IP: ${currentIpAddress}. New IP: ${ipAddress}");
    }

    [Test]
    public async Task LocalAgentError86999()
    {
        _homeRobot
            .DoWaitForVpnStatusSubtitleLabel()
            .DoConnect()
            .VerifyVpnStatusIsConnected();

        BtiController.SetScenarioAsync(BtiScenarios.HARDJAIL_86999);
        _homeRobot.VerifyUserIsNotReconnected();
    }

    [TearDown]
    public async Task TestCleanupAsync()
    {
        Cleanup();
        BtiController.SetScenarioAsync("reset");
        BtiController.SetScenarioAsync("enable/sessions_un_hardjail_all");
    }

    private async Task ReconnectionRequiredTestCase(string scenario)
    {
        _homeRobot
            .DoWaitForVpnStatusSubtitleLabel()
            .DoConnect()
            .VerifyVpnStatusIsConnected();

        BtiController.SetScenarioAsync(scenario);

        _homeRobot.VerifyAllStatesUntilConnected()
            .DoDisconnect();
    }

    private async Task DisconnectionRequiredTestCase(string scenario)
    {
        _homeRobot
            .DoWaitForVpnStatusSubtitleLabel()
            .DoConnect()
            .VerifyVpnStatusIsConnected();

        BtiController.SetScenarioAsync(scenario);

        _homeRobot.VerifyVpnStatusIsDisconnected();
    }

    private async Task NewCertificateRequiredTest(string scenario)
    {
        string certificateBefore = TestEnvironment.GetConnectionCertificate();
        await ReconnectionRequiredTestCase(scenario);
        string certificateAfter = TestEnvironment.GetConnectionCertificate();
        Assert.AreNotEqual(certificateBefore, certificateAfter);
    }
}
