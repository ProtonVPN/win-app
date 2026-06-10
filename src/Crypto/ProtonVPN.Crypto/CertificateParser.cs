/*
 * Copyright (c) 2026 Proton AG
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.X509;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Crypto;

public class CertificateParser : ICertificateParser
{
    private readonly ILogger _logger;

    public CertificateParser(ILogger logger)
    {
        _logger = logger;
    }

    public List<string> GetExtensionStrings(string certificatePem, string oid)
    {
        try
        {
            X509Certificate certificate = new X509CertificateParser().ReadCertificate(Encoding.ASCII.GetBytes(certificatePem));
            Asn1OctetString extension = certificate.GetExtensionValue(new DerObjectIdentifier(oid));
            if (extension is null)
            {
                return [];
            }

            Asn1Sequence sequence = (Asn1Sequence)Asn1Object.FromByteArray(extension.GetOctets());

            return sequence
                .OfType<Asn1OctetString>()
                .Select(o => Encoding.ASCII.GetString(o.GetOctets()))
                .ToList();
        }
        catch (Exception e)
        {
            _logger.Warn<AppLog>($"Failed to parse strings for OID {oid}.", e);

            return [];
        }
    }
}