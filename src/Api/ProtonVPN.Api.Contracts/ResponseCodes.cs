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

namespace ProtonVPN.Api.Contracts;

public class ResponseCodes
{
    public const int OK_RESPONSE = 1000;
    public const int FORCE_PASSWORD_CHANGE_RESPONSE = 2011;
    public const int CLIENT_PUBLIC_KEY_CONFLICT = 2500;
    public const int SERVER_DOES_NOT_EXIST = 2501;
    public const int OUTDATED_APP_RESPONSE = 5003;
    public const int OUTDATED_API_RESPONSE = 5005;
    public const int INVALID_PROFILE_ID_ON_UPDATE = 86062;
    public const int INVALID_PROFILE_ID_ON_DELETE = 86063;
    public const int PROFILE_NAME_CONFLICT = 86065;
    public const int HUMAN_VERIFICATION_REQUIRED = 9001;
    public const int NO_VPN_CONNECTIONS_ASSIGNED = 86300;
    public const int INCORRECT_LOGIN_CREDENTIALS = 8002;
    public const int AUTH_SWITCH_TO_SSO = 8100;
    public const int AUTH_SWITCH_TO_SRP = 8101;
}