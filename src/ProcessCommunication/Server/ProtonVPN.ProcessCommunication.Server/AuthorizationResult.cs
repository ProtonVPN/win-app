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

namespace ProtonVPN.ProcessCommunication.Server;

public class AuthorizationResult
{
    public static AuthorizationResult Ok() => new(0);
    public static AuthorizationResult Error(int statusCode) => new(statusCode);
    public static AuthorizationResult Error(int statusCode, string clientProcessFileName, string serverProcessFileName) =>
        new(statusCode, clientProcessFileName, serverProcessFileName);

    public int StatusCode { get; private set; }
    public string ClientProcessFileName { get; private set; }
    public string ServerProcessFileName { get; private set; }

    private AuthorizationResult(int statusCode)
    {
        StatusCode = statusCode;
    }

    private AuthorizationResult(int statusCode, string clientProcessFileName, string serverProcessFileName)
        : this(statusCode)
    {
        ClientProcessFileName = clientProcessFileName;
        ServerProcessFileName = serverProcessFileName;
    }
}