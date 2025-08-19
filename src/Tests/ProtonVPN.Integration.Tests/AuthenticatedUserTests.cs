/*
 * Copyright (c) 2025 Proton AG
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

using System.Net;
using System.Security;
using ProtonVPN.Client.Logic.Auth;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Models;
using RichardSzalay.MockHttp;

namespace ProtonVPN.Integration.Tests;

public class AuthenticatedUserTests : TestBase
{
    protected const string CORRECT_PASSWORD = "password";
    protected const string WRONG_PASSWORD = "wrong";
    protected const string CERTIFICATE =
        "-----BEGIN CERTIFICATE-----\n" +
        "MIICEzCCAcWgAwIBAgICA6QwBQYDK2VwMDgxNjA0BgNVBAMMLVByb3RvblZQTiBD\n" +
        "bGllbnQgQ2VydGlmaWNhdGUgQXV0aG9yaXR5IDE3ODYxMzAeFw0yMjA2MzAwOTIy\n" +
        "NDZaFw0yMjA3MDEwOTIyNDdaMA4xDDAKBgNVBAMMAzkzMjAqMAUGAytlcAMhAIRl\n" +
        "nVhh8k5SlmSWqEjvaLnghx2z01fD/jWtWikfyJ3oo4IBGzCCARcwHQYDVR0OBBYE\n" +
        "FD2ziAiZ58wvQokqBl/YrHX3rU9gMBMGDCsGAQQBg7tpAQAAAAQDAgEAMBMGDCsG\n" +
        "AQQBg7tpAQAAAQQDAgEAMBwGDCsGAQQBg7tpAQAAAgQMMAoECHZwbi1mcmVlMBkG\n" +
        "DCsGAQQBg7tpAQAAAwQJBAdXaW5kb3dzMA4GA1UdDwEB/wQEAwIHgDAMBgNVHRMB\n" +
        "Af8EAjAAMBMGA1UdJQQMMAoGCCsGAQUFBwMCMGAGA1UdIwRZMFeAFAM/RhKQDNy0\n" +
        "eNyeSWDEioh6Rv1goTykOjA4MTYwNAYDVQQDDC1Qcm90b25WUE4gQ2xpZW50IENl\n" +
        "cnRpZmljYXRlIEF1dGhvcml0eSAxNzg2MTOCAQIwBQYDK2VwA0EATuIkjDSzmZa5\n" +
        "UIfgGDGcNvFTVQBxSNzmGgZ+8YS60asMpo2vlGOyuy1U8vb3PsHSRYIgnJVup0P0\n" +
        "aFJlft9QCg==\n" +
        "-----END CERTIFICATE-----\n";

    protected void SetApiResponsesForAuth()
    {
        SetAuthInfoResponse();
        SetAuthResponse();
        SetVpnInfoResponse();
        SetAuthCertificateResponse();
        SetUserResponse();
        SetIpv6ragmentsResponse();
    }

    protected void SetApiResponsesForAuthWithTwoFactor()
    {
        SetAuthInfoResponse();
        SetAuthResponseWithTwoFactorEnabled();
    }

    private void SetAuthInfoResponse()
    {
        MessageHandler!.When(HttpMethod.Post, "/auth/info").Respond(_ => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(GetJsonMock("AuthInfoResponseMock"))
        });
    }

    private void SetAuthResponse()
    {
        MessageHandler!.When(HttpMethod.Post, "/auth").Respond(_ => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(GetJsonMock("AuthResponseMock"))
        });
    }

    private void SetAuthResponseWithTwoFactorEnabled()
    {
        MessageHandler!.When(HttpMethod.Post, "/auth").Respond(_ => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(GetJsonMock("AuthResponseWithTwoFactorEnabledMock"))
        });
    }

    private void SetVpnInfoResponse()
    {
        MessageHandler!.When(HttpMethod.Get, "/vpn/v2").Respond(_ => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(GetJsonMock("VpnInfoWrapperResponseMock"))
        });
    }

    private void SetAuthCertificateResponse()
    {
        MessageHandler!.When(HttpMethod.Post, "/vpn/v1/certificate").Respond(_ => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(GetJsonMock("CertificateResponseMock"))
        });
    }

    private void SetUserResponse()
    {
        MessageHandler!.When(HttpMethod.Get, "/core/v4/users").Respond(_ => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(GetJsonMock("UserResponseMock"))
        });
    }

    private void SetIpv6ragmentsResponse()
    {
        MessageHandler!.When(HttpMethod.Get, "/vpn/v1/ipv6-fragments").Respond(_ => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(GetJsonMock("IPv6FragmentsResponseMock"))
        });
    }

    protected async Task<AuthResult> MakeUserAuthAsync(string password)
    {
        SrpPInvoke.SetUnitTest();
        SecureString securePassword = new NetworkCredential("", password).SecurePassword;
        return await Resolve<IUserAuthenticator>().LoginUserAsync("username", securePassword);
    }
}