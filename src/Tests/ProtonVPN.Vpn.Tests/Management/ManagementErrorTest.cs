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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Vpn.Management;

namespace ProtonVPN.Vpn.Tests.Management
{
    [TestClass]
    public class ManagementErrorTest
    {
        [TestMethod]
        public void Message_ShouldBe_ManagementError_Message()
        {
            // Arrange
            const string expected = "Error!";
            ManagementError error = new ManagementError(expected);

            // Act
            string result = error.Message;

            // Assert
            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(VpnError.AuthorizationError, ">PASSWORD:Verification Failed: 'Auth'.")]
        [DataRow(VpnError.AuthorizationError, ">EXITING,auth-failure,,,")]
        [DataRow(VpnError.TapAdapterInUseError, ">LOG:1579682468,F,All TAP-Windows adapters on this system are currently in use.")]
        [DataRow(VpnError.TapAdapterInUseError, ">LOG:1579682468,F,All TAP-Win32 adapters on this system are currently in use.")]
        [DataRow(VpnError.NoTapAdaptersError, ">LOG:1579682468,F,There are no TAP-Windows adapters on this system.")]
        [DataRow(VpnError.NoTapAdaptersError, ">LOG:1579682468,F,There are no TAP-Win32 adapters on this system.")]
        [DataRow(VpnError.TapRequiresUpdateError, ",,,This version of OpenVPN requires a TAP-Windows driver that is at least version XXX")]
        [DataRow(VpnError.TapRequiresUpdateError, ",,,This version of OpenVPN requires a TAP-Win32 driver that is at least version XXX")]
        [DataRow(VpnError.AdapterTimeoutError, ",,,Timeout,,,")]
        [DataRow(VpnError.NetshError, ",,,NETSH: command failed,,,")]
        [DataRow(VpnError.TlsCertificateError, ">LOG:1579630832,,VERIFY SCRIPT ERROR: depth=0, CN=nl-free.proton.com")]
        [DataRow(VpnError.Unknown, ">LOG:1579630832,,VERIFY SCRIPT OK: depth=0, CN=nl-free.proton.com")]
        [DataRow(VpnError.TlsCertificateError, ">LOG:1579630614,N,VERIFY ERROR: depth=0, error=certificate has expired: CN=nl-free.proton.com")]
        [DataRow(VpnError.Unknown, ">LOG:1579630614,N,VERIFY OK: depth=0, error=certificate has expired: CN=nl-free.proton.com")]
        [DataRow(VpnError.Unknown, ">STATE:1579680207,ASSIGN_IP,,10.8.0.13,,,,")]
        public void VpnError_ShouldBe(VpnError expected, string message)
        {
            // Arrange
            ManagementError error = new ManagementError(message);

            // Act
            VpnError result = error.VpnError();

            // Assert
            result.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(true, ">FATAL,BlaBlaBla")]
        [DataRow(true, ">PASSWORD:Verification Failed: 'Auth'")]
        [DataRow(true, ">STATE:1579680219,RECONNECTING,tls-error,,,,,")]
        [DataRow(false, ">STATE:1579680219,RECONNECTING,,,,,,,")]
        [DataRow(true, ">LOG:1579630832,,VERIFY SCRIPT ERROR: depth=0, CN=nl-free.proton.com")]
        [DataRow(false, ">LOG:1579630832,,VERIFY SCRIPT OK: depth=0, CN=nl-free.proton.com")]
        [DataRow(true, ">LOG:1579630614,N,VERIFY ERROR: depth=0, error=certificate has expired: CN=nl-free.proton.com")]
        [DataRow(false, ">LOG:1579630614,N,VERIFY OK: depth=0, error=certificate has expired: CN=nl-free.proton.com")]
        [DataRow(false, ">STATE:1579680207,ASSIGN_IP,,10.8.0.13,,,,")]
        public void ContainsError_ShouldBe(bool expected, string message)
        {
            // Act
            bool result = ManagementError.ContainsError(message);

            // Assert
            result.Should().Be(expected);
        }
    }
}
