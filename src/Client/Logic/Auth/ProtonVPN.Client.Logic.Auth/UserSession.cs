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

using ProtonVPN.Api.Contracts;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;

namespace ProtonVPN.Client.Logic.Auth;

public class UserSession : IUserSession,
    IEventMessageReceiver<AuthenticationStatusChanged>
{
    private AuthenticationStatus _authenticationStatus;

    public bool IsLoggedIn => _authenticationStatus == AuthenticationStatus.LoggedIn;

    public void Receive(AuthenticationStatusChanged message)
    {
        _authenticationStatus = message.AuthenticationStatus;
    }
}