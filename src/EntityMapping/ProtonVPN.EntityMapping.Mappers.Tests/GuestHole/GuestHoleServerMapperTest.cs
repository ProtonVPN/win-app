﻿/*
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

using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.EntityMapping.Mappers.GuestHole;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Crypto;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.EntityMapping.Mappers.Tests.GuestHole;

[TestClass]
public class GuestHoleServerMapperTest
{
    private IMapper<GuestHoleServerContract, VpnServerIpcEntity> _mapper;

    [TestInitialize]
    public void Initialize()
    {
        _mapper = new GuestHoleServerMapper();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _mapper = null;
    }

    [TestMethod]
    public void TestMapLeftToRight()
    {
        GuestHoleServerContract entity = new()
        {
            Host = "protonvpn.com",
            Ip = "192.168.0.0",
            Label = "1",
            Signature = "sdh2uS26AfSADioe5w6p6S5D2H5fkdY8p9Jfh1F1sdo2a5JfGHroGeunf6K9G4H1c1K/2u3G3oGKdso=="
        };

        VpnServerIpcEntity result = _mapper.Map(entity);

        Assert.IsNotNull(result);
        Assert.AreEqual(entity.Host, result.Name);
        Assert.AreEqual(entity.Ip, result.Ip);
        Assert.AreEqual(entity.Label, result.Label);
        Assert.AreEqual(entity.Signature, result.Signature);
        Assert.IsNull(result.X25519PublicKey);
    }

    [TestMethod]
    public void TestMapLeftToRight_WhenNull()
    {
        GuestHoleServerContract entity = null;

        VpnServerIpcEntity result = _mapper.Map(entity);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void TestMapRightToLeft()
    {
        VpnServerIpcEntity entity = new()
        {
            Name = "protonvpn.com",
            Ip = "192.168.0.0",
            Label = "1",
            Signature = "sdh2uS26AfSADioe5w6p6S5D2H5fkdY8p9Jfh1F1sdo2a5JfGHroGeunf6K9G4H1c1K/2u3G3oGKdso==",
            X25519PublicKey = new ServerPublicKeyIpcEntity()
        };

        GuestHoleServerContract result = _mapper.Map(entity);

        Assert.IsNotNull(result);
        Assert.AreEqual(entity.Name, result.Host);
        Assert.AreEqual(entity.Ip, result.Ip);
        Assert.AreEqual(entity.Label, result.Label);
        Assert.AreEqual(entity.Signature, result.Signature);
    }

    [TestMethod]
    public void TestMapRightToLeft_WhenNull()
    {
        VpnServerIpcEntity entity = null;

        GuestHoleServerContract result = _mapper.Map(entity);

        Assert.IsNull(result);
    }
}