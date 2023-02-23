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
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace ProtonVPN.Api.Handlers.TlsPinning
{
    public class ReportBody
    {
        private readonly List<string> _knownPins;
        private readonly IReadOnlyList<string> _chain;
        private readonly Uri _uri;
        private string _hash;

        public ReportBody(List<string> knownPins, Uri uri, IReadOnlyList<string> chain)
        {
            _knownPins = knownPins;
            _uri = uri;
            _chain = chain;
        }

        public ReportBody Value()
        {
            _hash = GetCertChainHash(_chain);

            return new ReportBody
            {
                DateTime = System.DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture),
                Hostname = _uri.Host,
                Port = _uri.Port,
                ValidatedCertificateChain = _chain,
                KnownPins = _knownPins
            };
        }

        private ReportBody() { }

        public string Hash()
        {
            return _hash;
        }

        [JsonProperty(PropertyName = "date-time")]
        public string DateTime { get; private set; }

        [JsonProperty(PropertyName = "hostname")]
        public string Hostname { get; private set; }

        [JsonProperty(PropertyName = "port")]
        public int Port { get; private set; }

        [JsonProperty(PropertyName = "validated-certificate-chain")]
        public IReadOnlyList<string> ValidatedCertificateChain { get; private set; }

        [JsonProperty(PropertyName = "known-pins")]
        public List<string> KnownPins { get; private set; }

        private string GetCertChainHash(IReadOnlyList<string> chain)
        {
            StringBuilder stringBuilder = new();
            foreach (string cert in chain)
            {
                stringBuilder.Append(cert);
            }

            using (MD5 md5 = MD5.Create())
            {
                return md5.ComputeHash(Encoding.ASCII.GetBytes(stringBuilder.ToString())).ToString();
            }
        }
    }
}