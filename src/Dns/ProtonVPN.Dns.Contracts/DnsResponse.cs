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
using System.Linq;
using ProtonVPN.Common.Networking;

namespace ProtonVPN.Dns.Contracts
{
    public class DnsResponse
    {
        public string Host { get; set; }
        public IList<IpAddress> IpAddresses { get; set; }
        public IList<string> AlternativeHosts { get; set; }

        public TimeSpan TimeToLive { get; set; }
        public DateTime ResponseDateTimeUtc { get; set; }
        public DateTime ExpirationDateTimeUtc { get; set; }

        public DnsResponse()
        {
        }
        
        public DnsResponse(string host, TimeSpan timeToLive, IList<IpAddress> ipAddresses, DateTime? responseDateTimeUtc = null)
            : this(host, timeToLive, ipAddresses, null, responseDateTimeUtc)
        {
        }
        
        public DnsResponse(string host, TimeSpan timeToLive, IList<string> alternativeHosts, DateTime? responseDateTimeUtc = null)
            : this(host, timeToLive, null, alternativeHosts, responseDateTimeUtc)
        {
        }

        private DnsResponse(string host, TimeSpan timeToLive, IList<IpAddress> ipAddresses,
            IList<string> alternativeHosts, DateTime? responseDateTimeUtc)
        {
            ResponseDateTimeUtc = responseDateTimeUtc ?? DateTime.UtcNow;
            ExpirationDateTimeUtc = ResponseDateTimeUtc + timeToLive;
            TimeToLive = timeToLive;

            Host = host;
            IpAddresses = ipAddresses ?? new List<IpAddress>();
            AlternativeHosts = alternativeHosts?.Distinct().ToList() ?? new List<string>();
        }

        public void SetDatesAndTimeToLive(TimeSpan timeToLive)
        {
            ResponseDateTimeUtc = DateTime.UtcNow;
            ExpirationDateTimeUtc = ResponseDateTimeUtc + timeToLive;
            TimeToLive = timeToLive;
        }
    }
}