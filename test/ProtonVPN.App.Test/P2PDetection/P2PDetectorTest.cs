/*
 * Copyright (c) 2020 Proton Technologies AG
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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Modals;
using ProtonVPN.P2PDetection;
using ProtonVPN.P2PDetection.Blocked;
using ProtonVPN.P2PDetection.Forwarded;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using PhysicalServer = ProtonVPN.Core.Servers.Models.PhysicalServer;
using Server = ProtonVPN.Core.Servers.Models.Server;

namespace ProtonVPN.App.Test.P2PDetection
{
    [TestClass]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    public class P2PDetectorTest
    {
        private ILogger _logger;
        private Common.Configuration.Config _appConfig;
        private IBlockedTraffic _blockedTraffic;
        private IForwardedTraffic _forwardedTraffic;
        private IScheduler _scheduler;
        private IModals _modals;
        private IDialogs _dialogs;

        private ISchedulerTimer _timer;
        private bool _timerIsEnabled;

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = Substitute.For<ILogger>();
            _appConfig = new Common.Configuration.Config();
            _blockedTraffic = Substitute.For<IBlockedTraffic>();
            _forwardedTraffic = Substitute.For<IForwardedTraffic>();
            _scheduler = Substitute.For<IScheduler>();
            _modals = Substitute.For<IModals>();
            _dialogs = Substitute.For<IDialogs>();

            _timer = Substitute.For<ISchedulerTimer>();
            _timer.When(x => x.Start()).Do(x => _timerIsEnabled = true);
            _timer.When(x => x.Stop()).Do(x => _timerIsEnabled = false);
            _timer.When(x => x.IsEnabled = Arg.Do<bool>(value => _timerIsEnabled = value));
            _timer.IsEnabled.Returns(_ => _timerIsEnabled);
            _scheduler.Timer().Returns(_timer);
        }

        [TestMethod]
        public void P2PDetector_ShouldInitialize_Timer()
        {
            // Arrange
            var checkInterval = TimeSpan.FromSeconds(30);
            _appConfig.P2PCheckInterval = checkInterval;
            // Act
            new P2PDetector(_logger, _appConfig, _blockedTraffic, _forwardedTraffic, _scheduler, _modals, _dialogs);
            // Assert
            _scheduler.Timer().Received(1);
            _timer.Received(1).Interval = checkInterval;
            _timer.Received(1).Tick += Arg.Any<EventHandler>();
        }

        [TestMethod]
        public async Task OnVpnStateChanged_ShouldStartTimer_WhenVpnStatus_IsConnected()
        {
            // Arrange
            var detector = new P2PDetector(_logger, _appConfig, _blockedTraffic, _forwardedTraffic, _scheduler, _modals, _dialogs);
            // Act
            await detector.OnVpnStateChanged(new VpnStateChangedEventArgs(VpnStatus.Connected, VpnError.None, "", false, VpnProtocol.Auto));
            // Assert
            _timer.IsEnabled.Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow(VpnStatus.Disconnected)]
        [DataRow(VpnStatus.Connecting)]
        [DataRow(VpnStatus.Waiting)]
        [DataRow(VpnStatus.Authenticating)]
        [DataRow(VpnStatus.RetrievingConfiguration)]
        [DataRow(VpnStatus.AssigningIp)]
        [DataRow(VpnStatus.Disconnecting)]
        public async Task OnVpnStateChanged_ShouldStopTimer_WhenVpnStatus_IsNotConnected(VpnStatus status)
        {
            // Arrange
            var detector = new P2PDetector(_logger, _appConfig, _blockedTraffic, _forwardedTraffic, _scheduler, _modals, _dialogs);
            await detector.OnVpnStateChanged(new VpnStateChangedEventArgs(VpnStatus.Connected, VpnError.None, "", false, VpnProtocol.Auto));
            // Act
            await detector.OnVpnStateChanged(new VpnStateChangedEventArgs(status, VpnError.None, "", false, VpnProtocol.Auto));
            // Assert
            _timer.IsEnabled.Should().BeFalse();
        }

        [TestMethod]
        public async Task OnTimerTick_ShouldCheck_BlockedTraffic_WhenFreeServer()
        {
            // Arrange
            var detector = new P2PDetector(_logger, _appConfig, _blockedTraffic, _forwardedTraffic, _scheduler, _modals, _dialogs);
            await detector.OnVpnStateChanged(new VpnStateChangedEventArgs(VpnStatus.Connected, VpnError.None, FreeServer(), false, VpnProtocol.Auto));
            // Act
            _timer.Tick += Raise.Event();
            // Assert
            await _blockedTraffic.Received(1).Detected();
            await _forwardedTraffic.DidNotReceive().Value();
        }

        [TestMethod]
        public async Task OnTimerTick_ShouldShow_BlockedTrafficModal_WhenFreeServer()
        {
            // Arrange
            _blockedTraffic.Detected().Returns(true);
            var detector = new P2PDetector(_logger, _appConfig, _blockedTraffic, _forwardedTraffic, _scheduler, _modals, _dialogs);
            await detector.OnVpnStateChanged(new VpnStateChangedEventArgs(VpnStatus.Connected, VpnError.None, FreeServer(), false, VpnProtocol.Auto));
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
            var detector = new P2PDetector(_logger, _appConfig, _blockedTraffic, _forwardedTraffic, _scheduler, _modals, _dialogs);
            await detector.OnVpnStateChanged(new VpnStateChangedEventArgs(VpnStatus.Connected, VpnError.None, FreeServer(), false, VpnProtocol.Auto));
            // Act
            _timer.Tick += Raise.Event();
            // Assert
            _timer.IsEnabled.Should().BeFalse();
        }

        [TestMethod]
        public async Task OnTimerTick_ShouldCheck_ForwardedTraffic_WhenPaidServer()
        {
            // Arrange
            _forwardedTraffic.Value().Returns(new ForwardedTrafficResult(true, true, string.Empty));
            var detector = new P2PDetector(_logger, _appConfig, _blockedTraffic, _forwardedTraffic, _scheduler, _modals, _dialogs);
            await detector.OnVpnStateChanged(new VpnStateChangedEventArgs(VpnStatus.Connected, VpnError.None, PaidServer(), false, VpnProtocol.Auto));
            // Act
            _timer.Tick += Raise.Event();
            // Assert
            await _forwardedTraffic.Received(1).Value();
            await _blockedTraffic.DidNotReceive().Detected();
        }

        [TestMethod]
        public async Task OnTimerTick_ShouldShow_ForwardedTrafficModal_WhenPaidServer()
        {
            // Arrange
            _forwardedTraffic.Value().Returns(new ForwardedTrafficResult(true, true, string.Empty));
            var detector = new P2PDetector(_logger, _appConfig, _blockedTraffic, _forwardedTraffic, _scheduler, _modals, _dialogs);
            await detector.OnVpnStateChanged(new VpnStateChangedEventArgs(VpnStatus.Connected, VpnError.None, PaidServer(), false, VpnProtocol.Auto));
            // Act
            _timer.Tick += Raise.Event();
            // Assert
            _modals.Received(1).Show<P2PForwardModalViewModel>();
        }

        [TestMethod]
        public async Task OnTimerTick_ShouldCheck_BlockedTraffic_WhenPaidServer()
        {
            // Arrange
            _forwardedTraffic.Value().Returns(new ForwardedTrafficResult(true, false, string.Empty));
            var detector = new P2PDetector(_logger, _appConfig, _blockedTraffic, _forwardedTraffic, _scheduler, _modals, _dialogs);
            await detector.OnVpnStateChanged(new VpnStateChangedEventArgs(VpnStatus.Connected, VpnError.None, PaidServer(), false, VpnProtocol.Auto));
            // Act
            _timer.Tick += Raise.Event();
            // Assert
            await _forwardedTraffic.Received(1).Value();
            await _blockedTraffic.Received(1).Detected();
        }

        [TestMethod]
        public async Task OnTimerTick_ShouldShow_BlockedTrafficModal_WhenPaidServer()
        {
            // Arrange
            _blockedTraffic.Detected().Returns(true);
            _forwardedTraffic.Value().Returns(new ForwardedTrafficResult(true, false, string.Empty));
            var detector = new P2PDetector(_logger, _appConfig, _blockedTraffic, _forwardedTraffic, _scheduler, _modals, _dialogs);
            await detector.OnVpnStateChanged(new VpnStateChangedEventArgs(VpnStatus.Connected, VpnError.None, PaidServer(), false, VpnProtocol.Auto));
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
            _forwardedTraffic.Value().Returns(new ForwardedTrafficResult(true, false, string.Empty));
            var detector = new P2PDetector(_logger, _appConfig, _blockedTraffic, _forwardedTraffic, _scheduler, _modals, _dialogs);
            await detector.OnVpnStateChanged(new VpnStateChangedEventArgs(VpnStatus.Connected, VpnError.None, PaidServer(), false, VpnProtocol.Auto));
            // Act
            _timer.Tick += Raise.Event();
            // Assert
            _timer.IsEnabled.Should().BeFalse();
        }

        #region Helpers

        private Server FreeServer()
        {
            return new Server(
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
                new Location(),
                new List<PhysicalServer>(),
                null
            );
        }

        private Server PaidServer()
        {
            return new Server(
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
                new Location(),
                new List<PhysicalServer>(),
                null
            );
        }

        #endregion
    }
}
