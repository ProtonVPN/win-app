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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;

namespace ProtonVPN.Client.Logic.Servers.Mappers.Tests;

[TestClass]
public class PhysicalServerMapperTest
{
    private PhysicalServerMapper _mapper;

    [TestInitialize]
    public void Initialize()
    {
        _mapper = new();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _mapper = null;
    }

    [TestMethod]
    [DataRow((sbyte)0, true)]
    [DataRow((sbyte)1, false)]
    public void TestGetPhysicalServerUnderMaintenance(sbyte status, bool expectedIsUnderMaintenance)
    {
        PhysicalServer server = _mapper.Map(CreatePhysicalServerResponse(status));

        Assert.AreEqual("host.protonvpn.com", server.Domain);
        Assert.AreEqual("127.0.0.1", server.EntryIp);
        Assert.AreEqual("0", server.Label);
        Assert.AreEqual("signature", server.Signature);
        Assert.AreEqual(status, server.Status);
        Assert.AreEqual(expectedIsUnderMaintenance, server.IsUnderMaintenance());
        Assert.AreEqual("public key", server.X25519PublicKey);
    }

    private PhysicalServerResponse CreatePhysicalServerResponse(sbyte status)
    {
        return new PhysicalServerResponse()
        {
            Id = "ID",
            Domain = "host.protonvpn.com",
            EntryIp = "127.0.0.1",
            Label = "0",
            Signature = "signature",
            Status = status,
            X25519PublicKey = "public key",
        };
    }
}