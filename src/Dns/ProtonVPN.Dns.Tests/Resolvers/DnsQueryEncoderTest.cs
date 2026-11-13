/*
 * Copyright (c) 2026 Proton AG
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
using ARSoft.Tools.Net.Dns;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Dns.Resolvers;

namespace ProtonVPN.Dns.Tests.Resolvers;

[TestClass]
public class DnsQueryEncoderTest
{
    [TestMethod]
    public void Encode_ARecordQuery_ProducesExpectedBytes()
    {
        byte[] expected =
        [
            0x00, 0x00, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x03, (byte)'a', (byte)'p', (byte)'i',
            0x09, (byte)'p', (byte)'r', (byte)'o', (byte)'t', (byte)'o', (byte)'n', (byte)'v', (byte)'p', (byte)'n',
            0x02, (byte)'c', (byte)'h',
            0x00,
            0x00, 0x01, 0x00, 0x01,
        ];

        byte[] result = DnsQueryEncoder.Encode("api.protonvpn.ch", RecordType.A);

        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    [DataRow("api.protonvpn.ch", RecordType.A)]
    [DataRow("dMFYGSLTQOJXXI33ONVQWS3BOMNUA.protonpro.xyz.", RecordType.Txt)]
    public void Encode_IsParsedByDnsLibrary(string host, RecordType recordType)
    {
        DnsMessage message = DnsMessage.Parse(DnsQueryEncoder.Encode(host, recordType));

        Assert.IsTrue(message.IsQuery);
        Assert.IsTrue(message.IsRecursionDesired);
        Assert.AreEqual(1, message.Questions.Count);
        Assert.AreEqual(host.TrimEnd('.'), message.Questions[0].Name.ToString().TrimEnd('.'), ignoreCase: true);
        Assert.AreEqual(recordType, message.Questions[0].RecordType);
        Assert.AreEqual(RecordClass.INet, message.Questions[0].RecordClass);
    }

    [TestMethod]
    public void Encode_LabelLongerThan63Characters_Throws()
    {
        string host = new string('a', 64) + ".com";

        Assert.ThrowsExactly<ArgumentException>(() => DnsQueryEncoder.Encode(host, RecordType.A));
    }

    [TestMethod]
    public void Encode_EmptyLabel_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => DnsQueryEncoder.Encode("api..protonvpn.ch", RecordType.A));
    }
}