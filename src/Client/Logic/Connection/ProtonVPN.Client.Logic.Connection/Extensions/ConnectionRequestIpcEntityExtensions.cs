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
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.Logic.Connection.Extensions;

public static class ConnectionRequestIpcEntityExtensions
{
    public static VpnError GetVpnError(this ConnectionRequestIpcEntity request)
    {
        VpnError error = VpnError.None;

        if (request.Servers.Length <= 0)
        {
            error = VpnError.NoServers;
        }
        else if (request.Credentials.Certificate is null ||
                 request.Credentials.ClientKeyPair is null ||
                 string.IsNullOrWhiteSpace(request.Credentials.Certificate.Pem))
        {
            error = VpnError.MissingConnectionCertificate;
        }
        else if (request.Credentials.Certificate.ExpirationDateUtc <= DateTimeOffset.UtcNow)
        {
            error = VpnError.CertificateExpired;
        }

        return error;
    }
}