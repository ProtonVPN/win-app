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
    public class AuthResponseMock : AuthResponse
    {
        public AuthResponseMock()
        {
            Code = ResponseCodes.OkResponse;
            AccessToken = "7ys35w5j3kjen44a6a5olvg5ndemalzz";
            RefreshToken = "3aqsccsx5b66aif5zvpjrbidzc7uyb5v";
            Scope = "full self payments parent user loggedin vpn verified";
            ServerProof = "EXbM89LueOivOAoBPaEy1QXMmrEm45WqYAn+71VBXfkHcSEIzTp5OJzow/Nts1wmwC/qcHHG6kkpI50uWM9tQok11s7MV76RMHE/vttvRl//TngR9RXqpNhEzmlKxtoAnTUB3jR6J2v9rkhp3ofm5bl+m7IR0SEHmTMcUxHdWPNuL7v5rScR6U/UgBKc3HMyLo0YimItA3dkedVKM2HUWB6dRZ3U3Z4+n6+llOOBzIFZOT7V/nlqk+BfrOn2S8CiV5xEKIMFGGDUxT69D+FvpHR+OBUthORaMHhk2ArSbSyKWputH9uhiyEP05dgrkjC3HOza5rB++JMRgpOYxeiMw==";
            TwoFactor = new TwoFactorAuthResponse { Enabled = 0, };
            Uid = "k3woa7bk7eitzotqh6p4a3wcm6homm4i";
        }

        public static AuthResponseMock WithTwoFactorEnabled()
        {
            return new AuthResponseMock
            {
                TwoFactor = new TwoFactorAuthResponse
                {
                    Enabled = 1
                }
            };
        }
    }
}