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

using ProtonVPN.Client.Logic.Connection.Contracts.Enums;

namespace ProtonVPN.Client.Logic.Connection.Contracts.Extensions;

public static class VpnErrorExtensions
{
    private static readonly List<VpnError> _errorsForUser =
    [
        VpnError.NoServers,
        VpnError.TapAdapterInUseError,
        VpnError.TapRequiresUpdateError,
        VpnError.TlsCertificateError,
        VpnError.RpcServerUnavailable,
        VpnError.MissingAuthCertificate,
        VpnError.WireGuardAdapterInUseError,
        VpnError.SessionLimitReachedBasic,
        VpnError.SessionLimitReachedFree,
        VpnError.SessionLimitReachedPlus,
        VpnError.SessionLimitReachedPro,
        VpnError.SessionLimitReachedVisionary,
        VpnError.SessionLimitReachedUnknown,
    ];

    private static readonly List<VpnError> _errorsForReconnect =
    [
        VpnError.TlsError,
        VpnError.PingTimeoutError,
        VpnError.AdapterTimeoutError,
        VpnError.PlanNeedsToBeUpgraded,
        VpnError.Unpaid,
        VpnError.PasswordChanged,
        VpnError.ServerUnreachable,
        VpnError.Unknown,
    ];

    private static readonly List<VpnError> _errorsForReconnectWithoutLastServer =
    [
        VpnError.ServerOffline,
        VpnError.ServerRemoved,
        VpnError.NoServerValidationPublicKey,
        VpnError.SessionBeingInstalled,
        VpnError.SystemErrorOnTheServer,
    ];

    private static readonly List<VpnError> _errorsForCertificateUpdate =
    [
        VpnError.CertificateRevoked,
        VpnError.CertRevokedOrExpired,
        VpnError.ClientKeyMismatch,
        VpnError.SessionKilledDueToMultipleKeys,
    ];

    public static bool RequiresInformingUser(this VpnError error)
    {
        return _errorsForUser.Contains(error);
    }

    public static bool RequiresReconnect(this VpnError error)
    {
        return _errorsForReconnect.Contains(error);
    }

    public static bool RequiresReconnectWithoutLastServer(this VpnError error)
    {
        return _errorsForReconnectWithoutLastServer.Contains(error);
    }

    public static bool RequiresCertificateUpdate(this VpnError error)
    {
        return _errorsForCertificateUpdate.Contains(error);
    }
}