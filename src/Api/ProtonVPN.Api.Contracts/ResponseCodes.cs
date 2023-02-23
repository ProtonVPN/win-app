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

namespace ProtonVPN.Api.Contracts
{
    public class ResponseCodes
    {
        public const int OkResponse = 1000;
        public const int ForcePasswordChangeResponse = 2011;
        public const int ClientPublicKeyConflict = 2500;
        public const int OutdatedAppResponse = 5003;
        public const int OutdatedApiResponse = 5005;
        public const int InvalidProfileIdOnUpdate = 86062;
        public const int InvalidProfileIdOnDelete = 86063;
        public const int ProfileNameConflict = 86065;
        public const int HumanVerificationRequired = 9001;
        public const int NoVpnConnectionsAssigned = 86300;
        public const int IncorrectLoginCredentials = 8002;
    }
}