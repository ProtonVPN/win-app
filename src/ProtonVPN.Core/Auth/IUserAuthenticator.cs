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
using System.Security;
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Auth;

namespace ProtonVPN.Core.Auth
{
    public interface IUserAuthenticator
    {
        bool IsLoggedIn { get; }
        event EventHandler<EventArgs> UserLoggedOut;
        event EventHandler<UserLoggedInEventArgs> UserLoggedIn;
        event EventHandler<EventArgs> UserLoggingIn;

        Task<AuthResult> LoginUserAsync(string username, SecureString password);
        Task<AuthResult> AuthAsync(string username, SecureString password);
        Task<AuthResult> SendTwoFactorCodeAsync(string code);
        Task<ApiResponseResult<VpnInfoWrapperResponse>> RefreshVpnInfoAsync();
        Task InvokeAutoLoginEventAsync();
        Task LogoutAsync();
    }
}