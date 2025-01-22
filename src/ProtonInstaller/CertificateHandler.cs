/*
 * Copyright (c) 2024 Proton AG
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

using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Formats.Asn1;

namespace ProtonInstaller
{
    public class CertificateHandler : HttpClientHandler
    {
        private const string TLS_PIN = "CT56BhOTmj5ZIPgb/xD5mH8rY3BLo/MlhP7oPyJUEDo=";

        public CertificateHandler()
        {
            ServerCertificateCustomValidationCallback = CertificateCustomValidationCallback;
        }

        protected bool CertificateCustomValidationCallback(
            HttpRequestMessage request,
            X509Certificate2? certificate,
            X509Chain? chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None && certificate is not null && GetTlsPin(certificate) == TLS_PIN;
        }

        private string GetTlsPin(X509Certificate2 certificate)
        {
            byte[] digest;
            using (SHA256 sha256 = SHA256.Create())
            {
                digest = sha256.ComputeHash(GetSubjectPublicKeyInfo(certificate));
            }

            return Convert.ToBase64String(digest);
        }

        private byte[] GetSubjectPublicKeyInfo(X509Certificate2 certificate)
        {
            byte[] rawCert = certificate.GetRawCertData();
            
            // Parse the certificate
            AsnReader reader = new AsnReader(rawCert, AsnEncodingRules.DER);
            AsnReader certificateReader = reader.ReadSequence();

            // Parse TBSCertificate
            AsnReader tbsCertificateReader = certificateReader.ReadSequence();

            // Parse version
            Asn1Tag versionTag = new Asn1Tag(TagClass.ContextSpecific, 0);
            if (tbsCertificateReader.PeekTag().HasSameClassAndValue(versionTag))
            {
                tbsCertificateReader.ReadSequence(versionTag);
            }

            // Parse serial number
            tbsCertificateReader.ReadIntegerBytes();

            // Parse signature algorithm
            AsnReader signatureAlgorithmReader = tbsCertificateReader.ReadSequence();
            signatureAlgorithmReader.ReadObjectIdentifier();

            // Parse issuer
            tbsCertificateReader.ReadEncodedValue();

            // Parse validity
            tbsCertificateReader.ReadSequence();

            // Parse subject
            tbsCertificateReader.ReadEncodedValue();

            // Parse subject public key info
            byte[] subjectPublicKeyInfo = tbsCertificateReader.ReadEncodedValue().ToArray();

            return subjectPublicKeyInfo;
        }
    }
}