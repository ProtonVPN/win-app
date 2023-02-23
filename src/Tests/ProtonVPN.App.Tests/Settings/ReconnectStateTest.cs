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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Settings.ReconnectNotification;

namespace ProtonVPN.App.Tests.Settings
{
    [TestClass]
    public class ReconnectStateTest
    {
        private IAppSettings _appSettings;
        private SettingsBuilder _settingsBuilder;

        [TestInitialize]
        public void TestInitialize()
        {
            _appSettings = Substitute.For<IAppSettings>();
            _settingsBuilder = new SettingsBuilder(_appSettings);
        }

        [TestMethod]
        public async Task ReconnectShouldBeRequiredOnlyIfChangesPending()
        {
            // Arrange
            _appSettings.OvpnProtocol.Returns("tcp");
            ReconnectState sut = new ReconnectState(_settingsBuilder);
            await sut.OnVpnStateChanged(GetVpnStateEventArgs(VpnStatus.Connected));
            _appSettings.OvpnProtocol.Returns("udp");

            // Assert
            sut.Required(nameof(IAppSettings.OvpnProtocol)).Should().BeTrue();
        }

        [TestMethod]
        public async Task ReconnectShouldNotBeRequiredIfDisconnected()
        {
            // Arrange
            _appSettings.OvpnProtocol.Returns("tcp");
            ReconnectState sut = new ReconnectState(_settingsBuilder);
            await sut.OnVpnStateChanged(GetVpnStateEventArgs(VpnStatus.Connected));
            _appSettings.OvpnProtocol.Returns("udp");

            // Act
            await sut.OnVpnStateChanged(GetVpnStateEventArgs(VpnStatus.Disconnected));

            // Assert
            sut.Required(nameof(IAppSettings.OvpnProtocol)).Should().BeFalse();
        }

        private VpnStateChangedEventArgs GetVpnStateEventArgs(VpnStatus status)
        {
            return new VpnStateChangedEventArgs(status, VpnError.None, string.Empty, false, default);
        }
    }
}
