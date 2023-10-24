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

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using ProtonVPN.Configurations.Contracts;

namespace ProtonVPN.Api.Handlers.TlsPinning;

public class TlsPinnedCertificateHandler : CertificateHandlerBase
{
    private readonly ICertificateValidator _certificateValidator;
    private readonly IConfiguration _config;

    public TlsPinnedCertificateHandler(ICertificateValidator certificateValidator, IConfiguration config)
    {
        _certificateValidator = certificateValidator;
        _config = config;
        ServerCertificateCustomValidationCallback = CertificateCustomValidationCallback;
    }

    protected bool CertificateCustomValidationCallback(HttpRequestMessage request, X509Certificate certificate,
        X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        CertificateValidationParams validationParams = new()
        {
            Certificate = certificate,
            Chain = GetCertificateChain(chain),
            HasSslError = _config.IsCertificateValidationEnabled && sslPolicyErrors != SslPolicyErrors.None,
            Host = request.Headers.Host ?? request.RequestUri.Host,
            RequestUri = request.RequestUri,
        };

        return _certificateValidator.IsValid(validationParams);
    }

    private IReadOnlyList<string> GetCertificateChain(X509Chain chain)
    {
        List<string> list = new();
        foreach (X509ChainElement element in chain.ChainElements)
        {
            list.Add(element.Certificate.ExportToPem());
        }

        return list;
    }
}