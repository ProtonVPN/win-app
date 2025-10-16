/*
 * Copyright (c) 2025 Proton AG
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

using ProtonVPN.Client.Logic.Connection.ConnectionErrors;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;

namespace ProtonVPN.Client.Logic.Connection;

public class ConnectionErrorFactory : IConnectionErrorFactory
{
    private readonly IConnectionManager _connectionManager;

    private readonly Lazy<IEnumerable<IConnectionError>> _connectionErrors;
    private readonly Lazy<UnknownConnectionError> _unknownConnectionError;

    public ConnectionErrorFactory(
        IConnectionManager connectionManager,
        Lazy<IEnumerable<IConnectionError>> connectionErrors,
        Lazy<UnknownConnectionError> unknownConnectionError)
    {
        _connectionManager = connectionManager;
        _connectionErrors = connectionErrors;
        _unknownConnectionError = unknownConnectionError;
    }

    public IConnectionError GetConnectionError(VpnError vpnError)
    {
        return vpnError switch
        {
            VpnError.None or
            VpnError.NoneKeepEnabledKillSwitch or
            VpnError.BaseFilteringEngineServiceNotRunning => GetConnectionError<NoConnectionError>(vpnError),

            VpnError.NoServers when _connectionManager.CurrentConnectionIntent is IConnectionProfile =>
                GetConnectionError<NoServersForProfileConnectionError>(vpnError),
            VpnError.NoServers => GetConnectionError<NoServersConnectionError>(vpnError),

            VpnError.WireGuardAdapterInUseError => GetConnectionError<WireGuardAdapterInUseConnectionError>(vpnError),
            VpnError.MissingConnectionCertificate => GetConnectionError<MissingConnectionCertificateError>(vpnError),
            VpnError.TlsCertificateError => GetConnectionError<TlsCertificateConnectionError>(vpnError),

            VpnError.NoTapAdaptersError => GetConnectionError<NoTapAdaptersConnectionError>(vpnError),
            VpnError.TapAdapterInUseError => GetConnectionError<TapAdapterInUseConnectionError>(vpnError),
            VpnError.TapRequiresUpdateError => GetConnectionError<TapRequiresUpdateConnectionError>(vpnError),
            VpnError.RpcServerUnavailable => GetConnectionError<RpcServerUnavailableConnectionError>(vpnError),

            VpnError.SessionLimitReachedBasic or
            VpnError.SessionLimitReachedFree or
            VpnError.SessionLimitReachedPlus or
            VpnError.SessionLimitReachedPro or
            VpnError.SessionLimitReachedVisionary or
            VpnError.SessionLimitReachedUnknown => GetConnectionError<SessionLimitReachedConnectionError>(vpnError),

            VpnError.TwoFactorRequiredReasonUnknown or
            VpnError.TwoFactorExpired or
            VpnError.TwoFactorNewConnection => GetConnectionError<TwoFactorRequiredConnectionError>(vpnError),

            _ => GetUnknownConnectionError(vpnError),
        };
    }

    private IConnectionError GetConnectionError<T>(VpnError vpnError) where T : IConnectionError
    {
        return _connectionErrors.Value.FirstOrDefault(e => e is T) ?? GetUnknownConnectionError(vpnError);
    }

    private UnknownConnectionError GetUnknownConnectionError(VpnError error)
    {
        UnknownConnectionError unknownConnectionError = _unknownConnectionError.Value;

        unknownConnectionError.SetLastError(error);

        return unknownConnectionError;
    }
}