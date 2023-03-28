/* The MIT License (MIT)

Copyright (c) 2023 Proton AG
Copyright (c) .NET Foundation and Contributors

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace ProtonVPN.TlsVerify
{
    // Adapted from https://github.com/dotnet/wcf/blob/a9984490334fdc7d7382cae3c7bc0c8783eacd16/src/System.Private.ServiceModel/src/System/IdentityModel/Claims/X509CertificateClaimSet.cs

    public static class X509Certificate2Extensions
    {
        public static IReadOnlyList<string> SubjectAlternativeDnsNames(this X509Certificate2 cert)
        {
            var ext = cert.Extensions[X509SubjectAlternativeNameConstants.Oid];
            if (ext == null)
            {
                return new string[0];
            }

            var asnString = ext.Format(false);
            if (string.IsNullOrWhiteSpace(asnString))
            {
                return new string[0];
            }

            var rawDnsEntries =
                asnString.Split(new [] { X509SubjectAlternativeNameConstants.Separator }, StringSplitOptions.RemoveEmptyEntries);

            var dnsEntries = rawDnsEntries
                .Select(n => n.Split(X509SubjectAlternativeNameConstants.Delimiter))
                .Where(p => p[0] == X509SubjectAlternativeNameConstants.DnsNameIdentifier)
                .Select(p => p[1]);

            return dnsEntries.ToList();
        }
    }
}
