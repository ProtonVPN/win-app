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
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Vpn;

namespace ProtonVPN.Vpn.Management
{
    /// <summary>
    /// Parses an error message received from OpenVPN management interface.
    /// </summary>
    public class ManagementError
    {
        private static readonly Dictionary<VpnError, Predicate<string>> ErrorMap = new()
        {
            [ProtonVPN.Common.Vpn.VpnError.AuthorizationError] = ContainsAuthError,
            [ProtonVPN.Common.Vpn.VpnError.TapAdapterInUseError] = ContainsTapInUseError,
            [ProtonVPN.Common.Vpn.VpnError.NoTapAdaptersError] = ContainsNoTapError,
            [ProtonVPN.Common.Vpn.VpnError.TapRequiresUpdateError] = ContainsTapRequiresUpdateError,
            [ProtonVPN.Common.Vpn.VpnError.AdapterTimeoutError] = ContainsTimeoutError,
            [ProtonVPN.Common.Vpn.VpnError.NetshError] = ContainsNetshError,
            [ProtonVPN.Common.Vpn.VpnError.TlsCertificateError] = ContainsTlsCertificateError,
            [ProtonVPN.Common.Vpn.VpnError.ClientKeyMismatch] = ContainsKeyVerificationError,
        };

        public string Message { get; }

        public ManagementError(string message)
        {
            Message = message;
        }

        public VpnError VpnError() => GetErrorType();

        public static bool ContainsError(string message)
        {
            return message.StartsWithIgnoringCase(">FATAL") || 
                   message.StartsWithIgnoringCase(">PASSWORD:Verification Failed: 'Auth'") ||
                   ContainsTlsError(message) ||
                   ContainsTlsCertificateError(message);
        }

        private VpnError GetErrorType()
        {
            foreach (KeyValuePair<VpnError, Predicate<string>> mapItem in ErrorMap)
            {
                if (mapItem.Value(Message))
                {
                    return mapItem.Key;
                }
            }

            return ProtonVPN.Common.Vpn.VpnError.Unknown;
        }

        private static bool ContainsNetshError(string message)
        {
            return message.ContainsIgnoringCase("NETSH: command failed");
        }

        private static bool ContainsAuthError(string message)
        {
            return message.ContainsIgnoringCase("PASSWORD:Verification Failed: 'Auth'")
                || message.ContainsIgnoringCase("EXITING,auth-failure");
        }

        private static bool ContainsTapInUseError(string message)
        {
            return message.ContainsIgnoringCase("All TAP-Windows adapters on this system are currently in use")
                || message.ContainsIgnoringCase("All TAP-Win32 adapters on this system are currently in use")
                || message.ContainsIgnoringCase("Failed to open wintun adapter");
        }

        private static bool ContainsNoTapError(string message)
        {
            return message.ContainsIgnoringCase("There are no TAP-Windows adapters on this system")
                || message.ContainsIgnoringCase("There are no TAP-Win32 adapters on this system");
        }

        private static bool ContainsTapRequiresUpdateError(string message)
        {
            return message.ContainsIgnoringCase(
                       "This version of OpenVPN requires a TAP-Windows driver that is at least version")
                || message.ContainsIgnoringCase(
                       "This version of OpenVPN requires a TAP-Win32 driver that is at least version");
        }

        private static bool ContainsTlsError(string message)
        {
            return message.StartsWithIgnoringCase(">STATE:") && message.ContainsIgnoringCase(",RECONNECTING,tls-error,");
        }

        private static bool ContainsTlsCertificateError(string message)
        {
            return message.StartsWithIgnoringCase(">LOG:") && (
                message.ContainsIgnoringCase("VERIFY ERROR:") ||
                message.ContainsIgnoringCase("VERIFY SCRIPT ERROR:"));
        }

        private static bool ContainsTimeoutError(string message)
        {
            return message.ContainsIgnoringCase("Timeout");
        }

        private static bool ContainsKeyVerificationError(string message)
        {
            return message.ContainsIgnoringCase("private key password verification failed");
        }
    }
}