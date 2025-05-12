/*
 * Copyright (c) 2024 Proton AG
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
            VpnError.BaseFilteringEngineServiceNotRunning => GetConnectionError<NoConnectionError>(),

            VpnError.NoServers when _connectionManager.CurrentConnectionIntent is IConnectionProfile =>
                GetConnectionError<NoServersForProfileConnectionError>(),
            VpnError.NoServers => GetConnectionError<NoServersConnectionError>(),

            VpnError.WireGuardAdapterInUseError => GetConnectionError<WireGuardAdapterInUseConnectionError>(),
            VpnError.MissingConnectionCertificate => GetConnectionError<MissingConnectionCertificateError>(),
            VpnError.TlsCertificateError => GetConnectionError<TlsCertificateConnectionError>(),

            VpnError.NoTapAdaptersError => GetConnectionError<NoTapAdaptersConnectionError>(),
            VpnError.TapAdapterInUseError => GetConnectionError<TapAdapterInUseConnectionError>(),
            VpnError.TapRequiresUpdateError => GetConnectionError<TapRequiresUpdateConnectionError>(),
            VpnError.RpcServerUnavailable => GetConnectionError<RpcServerUnavailableConnectionError>(),

            VpnError.SessionLimitReachedBasic or
            VpnError.SessionLimitReachedFree or
            VpnError.SessionLimitReachedPlus or
            VpnError.SessionLimitReachedPro or
            VpnError.SessionLimitReachedVisionary or
            VpnError.SessionLimitReachedUnknown => GetConnectionError<SessionLimitReachedConnectionError>(),

            _ => GetUnknownConnectionError(),
        };
    }

    private IConnectionError GetConnectionError<T>() where T : IConnectionError
    {
        return _connectionErrors.Value.FirstOrDefault(e => e is T) ?? GetUnknownConnectionError();
    }

    private IConnectionError GetUnknownConnectionError()
    {
        return _unknownConnectionError.Value;
    }
}