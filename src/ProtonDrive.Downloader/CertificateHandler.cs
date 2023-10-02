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
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;
using SystemX509Certificate = System.Security.Cryptography.X509Certificates.X509Certificate;
using BouncyCastleX509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace ProtonDrive.Downloader
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
            SystemX509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None && GetTlsPin(certificate) == TLS_PIN;
        }

        private string GetTlsPin(SystemX509Certificate certificate)
        {
            byte[] digest;
            using (SHA256 sha256 = SHA256.Create())
            {
                digest = sha256.ComputeHash(GetSubjectPublicKeyInfo(certificate));
            }

            return Convert.ToBase64String(digest);
        }

        private byte[] GetSubjectPublicKeyInfo(SystemX509Certificate certificate)
        {
            BouncyCastleX509Certificate cert = new X509CertificateParser().ReadCertificate(certificate.GetRawCertData());
            TbsCertificateStructure tbsCert = TbsCertificateStructure.GetInstance(Asn1Object.FromByteArray(cert.GetTbsCertificate()));
            return tbsCert.SubjectPublicKeyInfo.GetDerEncoded();
        }
    }
}