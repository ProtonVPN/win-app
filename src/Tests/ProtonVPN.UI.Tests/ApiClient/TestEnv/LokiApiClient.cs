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

using System.Net.Http;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;

namespace ProtonVPN.UI.Tests.ApiClient.TestEnv;

public class LokiApiClient
{
    private readonly string _lokiCertificate = Environment.GetEnvironmentVariable("LOKI_CERTIFICATE_WINDOWS");
    private readonly string _lokiPrivateKey = Environment.GetEnvironmentVariable("LOKI_PRIVATE_KEY_WINDOWS");

    public HttpClient GetHttpClient()
    {
        HttpClientHandler handler = new();
        X509Certificate2 certWithPrivateKey = GetCertificateWithPrivateKey();
        handler.ClientCertificates.Add(certWithPrivateKey);
        return new HttpClient(handler);
    }

    private X509Certificate2 GetCertificateWithPrivateKey()
    {
        byte[] certBytes = Convert.FromBase64String(GetCleanedPem(_lokiCertificate, "CERTIFICATE"));

        X509CertificateParser certParser = new();
        Org.BouncyCastle.X509.X509Certificate cert = certParser.ReadCertificate(certBytes);

        AsymmetricKeyParameter privateKey = GetPrivateKeyParameter();
        Pkcs12Store store = CreatePkcs12Store(cert, privateKey);

        return ConvertToX509Certificate2(store, cert.SubjectDN.ToString());
    }

    private AsymmetricKeyParameter GetPrivateKeyParameter()
    {
        StringReader reader = new(_lokiPrivateKey);
        PemReader pemReader = new(reader);
        return (AsymmetricKeyParameter)pemReader.ReadObject();
    }

    private Pkcs12Store CreatePkcs12Store(Org.BouncyCastle.X509.X509Certificate cert, AsymmetricKeyParameter privateKey)
    {
        Pkcs12Store store = new();
        string alias = cert.SubjectDN.ToString();
        X509CertificateEntry certEntry = new(cert);
        store.SetCertificateEntry(alias, certEntry);
        store.SetKeyEntry(alias, new AsymmetricKeyEntry(privateKey), [certEntry]);
        return store;
    }

    private X509Certificate2 ConvertToX509Certificate2(Pkcs12Store store, string alias)
    {
        MemoryStream memeoryStream = new();
        store.Save(memeoryStream, [], new SecureRandom());
        memeoryStream.Seek(0, SeekOrigin.Begin);
        byte[] pfxBytes = memeoryStream.ToArray();
        return new X509Certificate2(pfxBytes);
    }

    private string GetCleanedPem(string pem, string boundary)
    {
        return pem.Replace($"-----BEGIN {boundary}-----", string.Empty)
                  .Replace($"-----END {boundary}-----", string.Empty)
                  .Replace("\n", string.Empty)
                  .Replace("\r", string.Empty);
    }
}