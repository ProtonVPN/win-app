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
using ProtonVPN.Client.Logic.Servers.Contracts;

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
    public void TestGetPhysicalServer()
    {
        PhysicalServer server = _mapper.Map(PhysicalServerResponse);

        Assert.AreEqual(server.Domain, "host.protonvpn.com");
        Assert.AreEqual(server.EntryIp, "127.0.0.1");
        Assert.AreEqual(server.ExitIp, "128.0.0.1");
        Assert.AreEqual(server.Label, "0");
        Assert.AreEqual(server.Signature, "signature");
        Assert.AreEqual(server.Status, 0);
        Assert.AreEqual(server.X25519PublicKey, "public key");
    }

    private PhysicalServerResponse PhysicalServerResponse => new()
    {
        Id = "ID",
        Domain = "host.protonvpn.com",
        EntryIp = "127.0.0.1",
        ExitIp = "128.0.0.1",
        Label = "0",
        Signature = "signature",
        Status = 0,
        X25519PublicKey = "public key",
    };
}