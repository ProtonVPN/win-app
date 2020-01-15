/* The MIT License (MIT)

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
using System.Security.Cryptography.X509Certificates;

namespace ProtonVPN.TlsVerify
{
    // Taken from https://github.com/dotnet/wcf/blob/a9984490334fdc7d7382cae3c7bc0c8783eacd16/src/System.Private.ServiceModel/src/System/IdentityModel/Claims/X509CertificateClaimSet.cs

    // We don't have a strongly typed extension to parse Subject Alt Names, so we have to do a workaround 
    // to figure out what the identifier, delimiter, and separator is by using a well-known extension

    internal static class X509SubjectAlternativeNameConstants
    {
        public const string Oid = "2.5.29.17";

        private static readonly string s_dnsNameIdentifier;
        private static readonly char s_delimiter;
        private static readonly string s_separator;

        private static readonly bool s_successfullyInitialized = false;
        private static readonly Exception s_initializationException;

        public static string DnsNameIdentifier
        {
            get
            {
                EnsureInitialized();
                return s_dnsNameIdentifier;
            }
        }

        public static char Delimiter
        {
            get
            {
                EnsureInitialized();
                return s_delimiter;
            }
        }

        public static string Separator
        {
            get
            {
                EnsureInitialized();
                return s_separator;
            }
        }

        private static void EnsureInitialized()
        {
            if (!s_successfullyInitialized)
            {
                throw new FormatException(
                    $"There was an error detecting the identifier, delimiter, and separator for X509CertificateClaims on this platform.{Environment.NewLine}" +
                    $"Detected values were: Identifier: '{s_dnsNameIdentifier}'; Delimiter:'{s_delimiter}'; Separator:'{s_separator}'", s_initializationException);
            }
        }

        static X509SubjectAlternativeNameConstants()
        {
            // Extracted a well-known X509Extension
            var x509ExtensionBytes = new byte[] {
                    48, 36, 130, 21, 110, 111, 116, 45, 114, 101, 97, 108, 45, 115, 117, 98, 106, 101, 99,
                    116, 45, 110, 97, 109, 101, 130, 11, 101, 120, 97, 109, 112, 108, 101, 46, 99, 111, 109
                };
            const string subjectName = "not-real-subject-name";

            try
            {
                var x509Extension = new X509Extension(Oid, x509ExtensionBytes, true);
                var x509ExtensionFormattedString = x509Extension.Format(false);

                // Each OS has a different dNSName identifier and delimiter
                // On Windows, dNSName == "DNS Name" (localizable), on Linux, dNSName == "DNS"
                // e.g.,
                // Windows: x509ExtensionFormattedString is: "DNS Name=not-real-subject-name, DNS Name=example.com"
                // Linux:   x509ExtensionFormattedString is: "DNS:not-real-subject-name, DNS:example.com"
                // Parse: <identifier><delimiter><value><separator(s)>

                var delimiterIndex = x509ExtensionFormattedString.IndexOf(subjectName) - 1;
                s_delimiter = x509ExtensionFormattedString[delimiterIndex];

                // Make an assumption that all characters from the the start of string to the delimiter 
                // are part of the identifier
                s_dnsNameIdentifier = x509ExtensionFormattedString.Substring(0, delimiterIndex);

                var separatorFirstChar = delimiterIndex + subjectName.Length + 1;
                var separatorLength = 1;
                for (var i = separatorFirstChar + 1; i < x509ExtensionFormattedString.Length; i++)
                {
                    // We advance until the first character of the identifier to determine what the
                    // separator is. This assumes that the identifier assumption above is correct
                    if (x509ExtensionFormattedString[i] == s_dnsNameIdentifier[0])
                    {
                        break;
                    }

                    separatorLength++;
                }

                s_separator = x509ExtensionFormattedString.Substring(separatorFirstChar, separatorLength);

                s_successfullyInitialized = true;
            }
            catch (Exception ex)
            {
                s_successfullyInitialized = false;
                s_initializationException = ex;
            }
        }
    }
}
