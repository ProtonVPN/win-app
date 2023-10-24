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
using System.Linq;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Configurations.Contracts.Entities;

namespace ProtonVPN.Api.Handlers.TlsPinning;

public class CertificateValidator : ICertificateValidator
{
    private readonly TlsPinningPolicy _policy;
    private readonly IReportClient _reportClient;
    private readonly IConfiguration _config;

    public CertificateValidator(IReportClient reportClient, IConfiguration config)
    {
        _policy = new();
        _reportClient = reportClient;
        _config = config;
    }

    public bool IsValid(CertificateValidationParams validationParams)
    {
        ITlsPinnedDomain domain = GetPinnedDomain(validationParams.Host) ?? GetPinnedDomain(Constants.ALTERNATIVE_ROUTING_HOSTNAME);
        if (domain == null)
        {
            return !validationParams.HasSslError && !_config.TlsPinning.Enforce;
        }

        if (domain.Name != Constants.ALTERNATIVE_ROUTING_HOSTNAME && validationParams.HasSslError)
        {
            return false;
        }

        bool isValid = _policy.Valid(domain, validationParams.Certificate);
        if (!isValid && domain.SendReport)
        {
            List<string> knownPins = domain.PublicKeyHashes.ToList();
            _reportClient.Send(new ReportBody(knownPins, validationParams.RequestUri, validationParams.Chain).Value());
        }

        return (!validationParams.HasSslError && !domain.Enforce) || isValid;
    }

    private ITlsPinnedDomain GetPinnedDomain(string host)
    {
        return _config.TlsPinning.PinnedDomains.FirstOrDefault(d => d.Name == host);
    }
}