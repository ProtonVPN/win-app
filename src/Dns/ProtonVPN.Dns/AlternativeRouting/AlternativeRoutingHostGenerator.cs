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
using System.Text;
using Albireo.Base32;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Dns.Contracts.AlternativeRouting;

namespace ProtonVPN.Dns.AlternativeRouting
{
    public class AlternativeRoutingHostGenerator : IAlternativeRoutingHostGenerator
    {
        private const string HOST_FORMAT = "d{0}.protonpro.xyz";

        private Lazy<string> _baseHost;

        public AlternativeRoutingHostGenerator(IConfiguration configuration)
        {
            _baseHost = new Lazy<string>(() => GenerateBaseHost(configuration.Urls.ApiUrl));
        }

        private string GenerateBaseHost(string apiUrl)
        {
            string base32ApiUrl = CalculateBase32ApiUrl(apiUrl);
            return string.Format(HOST_FORMAT, base32ApiUrl);
        }

        private string CalculateBase32ApiUrl(string apiUrl)
        {
            Uri apiUri = new(apiUrl);
            return Base32.Encode(Encoding.UTF8.GetBytes(apiUri.Host)).TrimEnd('=');
        }

        public string Generate(string uid)
        {
            return uid.IsNullOrEmpty() ? _baseHost.Value : $"{uid}.{_baseHost.Value}";
        }
    }
}