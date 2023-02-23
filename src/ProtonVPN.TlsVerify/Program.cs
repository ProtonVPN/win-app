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

using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace ProtonVPN.TlsVerify
{
    public class Program
    {
        private const int SuccessResult = 0;
        private const int FailureResult = 1;
        private const string CertFileNameEnvironmentVariable = "peer_cert";
        private const string ServerNameEnvironmentVariable = "peer_dns_name";

        /// <summary>
        /// Verifies the certificate by comparing expected server domain name with Subject Alternative Names
        /// in the certificate.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>0 if verification succeeded; 1 otherwise.</returns>
        /// <remarks>The command line arguments:
        /// Depth - the remaining depth of the certificate chain. "0" for the server certificate.
        /// <para>The path to the certificate file is passed in an environment variable "peer_cert".</para>
        /// <para>The expected server domain name is passed in an environment variable "peer_dns_name".</para>
        /// </remarks>
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                return FailureResult;
            }

            if (!int.TryParse(args[0], out var depth))
            {
                return FailureResult;
            }

            if (depth != 0)
            {
                // We do not check anything for intermediate or root certificate
                return SuccessResult;
            }

            var domainName = Environment.GetEnvironmentVariable(ServerNameEnvironmentVariable);
            if (string.IsNullOrEmpty(domainName))
            {
                return FailureResult;
            }

            var certificateFileName = Environment.GetEnvironmentVariable(CertFileNameEnvironmentVariable);
            if (string.IsNullOrEmpty(certificateFileName))
            {
                return FailureResult;
            }

            var certificate = Certificate(certificateFileName);
            if (certificate == null)
            {
                return FailureResult;
            }

            return Valid(certificate, domainName) ? SuccessResult : FailureResult;
        }

        private static X509Certificate2 Certificate(string fileName)
        {
            try
            {
                return new X509Certificate2(fileName);
            }
            catch 
            {
                return null;
            }
        }

        private static bool Valid(X509Certificate2 certificate, string domainName)
        {
            return certificate.SubjectAlternativeDnsNames().Contains(domainName, StringComparer.OrdinalIgnoreCase);
        }
    }
}
