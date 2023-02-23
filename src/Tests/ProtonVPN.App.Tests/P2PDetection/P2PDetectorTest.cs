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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Api.Contracts.Geographical;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Vpn;
using ProtonVPN.P2PDetection;
using ProtonVPN.P2PDetection.Blocked;
using PhysicalServer = ProtonVPN.Core.Servers.Models.PhysicalServer;
using Server = ProtonVPN.Core.Servers.Models.Server;

namespace ProtonVPN.App.Tests.P2PDetection
{
    [TestClass]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    public class P2PDetectorTest
    {
        private ILogger _logger;
        private IConfiguration _appConfig;
        private IBlockedTraffic _blockedTraffic;
        private IScheduler _scheduler;
        private IDialogs _dialogs;

        private ISchedulerTimer _timer;
        private bool _timerIsEnabled;

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = Substitute.For<ILogger>();
            _appConfig = new Common.Configuration.Config();
            _blockedTraffic = Substitute.For<IBlockedTraffic>();
            _scheduler = Substitute.For<IScheduler>();
            _dialogs = Substitute.For<IDialogs>();

            _appConfig.P2PCheckInterval = TimeSpan.FromSeconds(10);

            _timer = Substitute.For<ISchedulerTimer>();
            _timer.When(x => x.Start()).Do(_ => _timerIsEnabled = true);
            _timer.When(x => x.Stop()).Do(_ => _timerIsEnabled = false);
            _timer.When(x => x.IsEnabled = Arg.Do<bool>(value => _timerIsEnabled = value));
            _timer.IsEnabled.Returns(_ => _timerIsEnabled);
            _scheduler.Timer().Returns(_timer);
        }

        [TestMethod]
        public void P2PDetector_ShouldInitialize_Timer()
        {
            // Arrange
            TimeSpan checkInterval = TimeSpan.FromSeconds(30);
            _appConfig.P2PCheckInterval = checkInterval;
            // Act
            new P2PDetector(_logger, _appConfig, _blockedTraffic, _scheduler, _dialogs);
            // Assert
            _scheduler.Timer().Received(1);
            _timer.Received(1).Tick += Arg.Any<EventHandler>();
        }

        [TestMethod]
        public async Task OnVpnStateChanged_ShouldStartTimer_WhenVpnStatus_IsConnected()
        {
            // Arrange
            P2PDetector detector = new P2PDetector(_logger, _appConfig, _blockedTraffic, _scheduler, _dialogs);
            // Act
            await detector.OnVpnStateChanged(new VpnStateChangedEventArgs(VpnStatus.Connected, VpnError.None, "", false, 
                VpnProtocol.Smart));
            // Assert
            _timer.IsEnabled.Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow(VpnStatus.Disconnected)]
        [DataRow(VpnStatus.Pinging)]
        [DataRow(VpnStatus.Connecting)]
        [DataRow(VpnStatus.Waiting)]
        [DataRow(VpnStatus.Authenticating)]
        [DataRow(VpnStatus.RetrievingConfiguration)]
        [DataRow(VpnStatus.AssigningIp)]
        [DataRow(VpnStatus.Disconnecting)]
        public async Task OnVpnStateChanged_ShouldStopTimer_WhenVpnStatus_IsNotConnected(VpnStatus status)
        {
            // Arrange
            P2PDetector detector = new P2PDetector(_logger, _appConfig, _blockedTraffic, _scheduler, _dialogs);
            await detector.OnVpnStateChanged(new VpnStateChangedEventArgs(VpnStatus.Connected, VpnError.None, "", false, 
                VpnProtocol.Smart));
            // Act
            await detector.OnVpnStateChanged(new VpnStateChangedEventArgs(status, VpnError.None, "", false, 
                VpnProtocol.Smart));
            // Assert
            _timer.IsEnabled.Should().BeFalse();
        }

        [TestMethod]
        public async Task OnTimerTick_ShouldCheck_BlockedTraffic_WhenFreeServer()
        {
            // Arrange
            P2PDetector detector = new P2PDetector(_logger, _appConfig, _blockedTraffic, _scheduler, _dialogs);
            await detector.OnVpnStateChanged(new VpnStateChangedEventArgs(VpnStatus.Connected, VpnError.None, FreeServer(), false));
            // Act
            _timer.Tick += Raise.Event();
            // Assert
            await _blockedTraffic.Received(1).Detected();
        }

        [TestMethod]
        public async Task OnTimerTick_ShouldShow_BlockedTrafficModal_WhenFreeServer()
        {
            // Arrange
            _blockedTraffic.Detected().Returns(true);
            P2PDetector detector = new P2PDetector(_logger, _appConfig, _blockedTraffic, _scheduler, _dialogs);
            await detector.OnVpnStateChanged(new VpnStateChangedEventArgs(VpnStatus.Connected, VpnError.None, FreeServer(), false));
            // Act
            _timer.Tick += Raise.Event();
            // Assert
            _dialogs.ReceivedWithAnyArgs(1).ShowWarning("");
        }

        [TestMethod]
        public async Task OnTimerTick_ShouldStopTimer_WhenBlockedTrafficDetected_OnFreeServer()
        {
            // Arrange
            _blockedTraffic.Detected().Returns(true);
            P2PDetector detector = new P2PDetector(_logger, _appConfig, _blockedTraffic, _scheduler, _dialogs);
            await detector.OnVpnStateChanged(new VpnStateChangedEventArgs(VpnStatus.Connected, VpnError.None, FreeServer(), false));
            // Act
            _timer.Tick += Raise.Event();
            // Assert
            _timer.IsEnabled.Should().BeFalse();
        }

        [TestMethod]
        public async Task OnTimerTick_ShouldCheck_BlockedTraffic_WhenPaidServer()
        {
            // Arrange
            P2PDetector detector = new P2PDetector(_logger, _appConfig, _blockedTraffic, _scheduler, _dialogs);
            await detector.OnVpnStateChanged(new VpnStateChangedEventArgs(VpnStatus.Connected, VpnError.None, PaidServer(), false));
            // Act
            _timer.Tick += Raise.Event();
            // Assert
            await _blockedTraffic.Received(1).Detected();
        }

        [TestMethod]
        public async Task OnTimerTick_ShouldShow_BlockedTrafficModal_WhenPaidServer()
        {
            // Arrange
            _blockedTraffic.Detected().Returns(true);
            P2PDetector detector = new P2PDetector(_logger, _appConfig, _blockedTraffic, _scheduler, _dialogs);
            await detector.OnVpnStateChanged(new VpnStateChangedEventArgs(VpnStatus.Connected, VpnError.None, PaidServer(), false));
            // Act
            _timer.Tick += Raise.Event();
            // Assert
            _dialogs.ReceivedWithAnyArgs(1).ShowWarning("");
        }

        [TestMethod]
        public async Task OnTimerTick_ShouldStopTimer_WhenBlockedTrafficDetected_OnPaidServer()
        {
            // Arrange
            _blockedTraffic.Detected().Returns(true);
            P2PDetector detector = new P2PDetector(_logger, _appConfig, _blockedTraffic, _scheduler, _dialogs);
            await detector.OnVpnStateChanged(new VpnStateChangedEventArgs(VpnStatus.Connected, VpnError.None, PaidServer(), false));
            // Act
            _timer.Tick += Raise.Event();
            // Assert
            _timer.IsEnabled.Should().BeFalse();
        }

        #region Helpers

        private Server FreeServer()
        {
            return new(
                "ID",
                "Name",
                "City",
                "ZZZ",
                "ZZZ",
                "Domain",
                1,
                0,
                0,
                0,
                0,
                new LocationResponse(),
                new List<PhysicalServer>(),
                null
            );
        }

        private Server PaidServer()
        {
            return new(
                "ID",
                "Name",
                "City",
                "ZZZ",
                "ZZZ",
                "Domain",
                1,
                1,
                0,
                0,
                0,
                new LocationResponse(),
                new List<PhysicalServer>(),
                null
            );
        }

        #endregion
    }
}