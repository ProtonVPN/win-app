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

using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Api.Contracts.Certificates;
using ProtonVPN.Api.Contracts.Common;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ProtonVPN.IntegrationTests.Api
{
    public class ApiServerMock
    {
        private WireMockServer _server;

        public void Start(int port)
        {
            _server = WireMockServer.Start(port);
        }

        public void Stop()
        {
            _server.Stop();
        }

        public void SetAuthInfoResponse(AuthInfoResponse response)
        {
            SetJsonResponse("/auth/info", response);
        }

        public void SetAuthResponse(AuthResponse response)
        {
            SetJsonResponse("/auth", response);
        }

        public void SetVpnInfoResponse(VpnInfoWrapperResponse response)
        {
            SetJsonResponse("/vpn/v2", response);
        }

        public void SetAuthCertificateResponse(CertificateResponse response)
        {
            SetJsonResponse("/vpn/v1/certificate", response);
        }

        private void SetJsonResponse(string path, BaseResponse response)
        {
            _server.Given(Request.Create().WithPath(path))
                .RespondWith(Response.Create().WithBodyAsJson(response));
        }
    }
}