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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;

namespace ProtonVPN.Core.OS.Net.DoH
{
    public class Client
    {
        private readonly HttpClient _httpClient;
        private readonly string _providerUrl;

        public Client(string providerUrl, TimeSpan timeout)
        {
            _providerUrl = providerUrl;
            _httpClient = new HttpClient {Timeout = timeout};
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/dns-message"));
        }

        public async Task<List<string>> ResolveTxtAsync(string domain)
        {
            var message = new TxtMessage(domain).ToBase64String();
            var result = await _httpClient.GetAsync($"{_providerUrl}?dns={message}");
            var content = await result.Content.ReadAsByteArrayAsync();
            var messageResult = DnsMessage.Parse(content);
            var list = new List<string>();

            foreach (var record in messageResult.AnswerRecords)
            {
                if (record is TxtRecord txt)
                {
                    list.Add(txt.TextData);
                }
            }

            return list;
        }
    }
}
