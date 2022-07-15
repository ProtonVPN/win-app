/*
 * Copyright (c) 2022 Proton Technologies AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software:
 you can redistribute it and/or modify
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
using ProtonVPN.Api.Contracts.Auth;

namespace ProtonVPN.IntegrationTests.Api.Responses
{
    public class VpnInfoWrapperResponseMock : VpnInfoWrapperResponse
    {
        public VpnInfoWrapperResponseMock()
        {
            Code = ResponseCodes.OkResponse;
            Credit = 0;
            Delinquent = 0;
            HasPaymentMethod = 0;
            Services = 1;
            Subscribed = 0;
            Vpn = new VpnInfoResponse
            {
                GroupId = "group",
                MaxConnect = 2,
                MaxTier = 0,
                PlanName = "free",
                Status = 1
            };
        }
    }
}