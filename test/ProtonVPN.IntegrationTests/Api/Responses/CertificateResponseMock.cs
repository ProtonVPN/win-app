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

using System;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Certificates;

namespace ProtonVPN.IntegrationTests.Api.Responses
{
    public class CertificateResponseMock : CertificateResponse
    {
        public const string CERTIFICATE =
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

        public const string SERVER_PUBLIC_KEY =
            "-----BEGIN PUBLIC KEY-----\n" +
            "MCowBQYDK2VwAyEAkQAJWbKXT0kuiOkA/QNpMQLMzUcRdsaY5bg9gO4/zUE=\n" +
            "-----END PUBLIC KEY-----\n";

        public CertificateResponseMock()
        {
            Code = ResponseCodes.OkResponse;
            Certificate = CERTIFICATE;
            ServerPublicKey = SERVER_PUBLIC_KEY;
            ExpirationTime = DateTimeOffset.Now.AddDays(1).ToUnixTimeSeconds();
            RefreshTime = DateTimeOffset.Now.AddHours(22).ToUnixTimeSeconds();
        }
    }
}