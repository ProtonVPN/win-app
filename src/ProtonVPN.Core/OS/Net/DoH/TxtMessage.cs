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

using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;

namespace ProtonVPN.Core.OS.Net.DoH
{
    public class TxtMessage : BaseDnsMessage
    {
        private readonly string _domain;

        public TxtMessage(string domain)
        {
            _domain = domain;
        }

        protected override byte[] GetBytes()
        {
            var dnsMessage = new DnsMessage();
            var question = new DnsQuestion(DomainName.Parse(_domain), RecordType.Txt, RecordClass.INet);
            dnsMessage.Questions.Add(question);
            dnsMessage.IsRecursionDesired = true;

            return dnsMessage.Encode();
        }
    }
}
