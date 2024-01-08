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
using NSubstitute;
using ProtonVPN.Common.Legacy.PortForwarding;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.PortForwarding;
using ProtonVPN.ProcessCommunication.EntityMapping.PortForwarding;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Tests.PortForwarding;

[TestClass]
public class PortForwardingStateMapperTest
{
    private const PortMappingStatusIpcEntity EXPECTED_PORT_MAPPING_STATUS_IPC_ENTITY = PortMappingStatusIpcEntity.Error;
    private const PortMappingStatus EXPECTED_PORT_MAPPING_STATUS = PortMappingStatus.DestroyPortMappingCommunication;

    private IEntityMapper _entityMapper;
    private PortForwardingStateMapper _mapper;
    private TemporaryMappedPortIpcEntity _expectedTemporaryMappedPortIpcEntity;
    private TemporaryMappedPort _expectedTemporaryMappedPort;

    [TestInitialize]
    public void Initialize()
    {
        _entityMapper = Substitute.For<IEntityMapper>();
        _mapper = new(_entityMapper);

        _expectedTemporaryMappedPortIpcEntity = new TemporaryMappedPortIpcEntity();
        _entityMapper.Map<TemporaryMappedPort, TemporaryMappedPortIpcEntity>(Arg.Any<TemporaryMappedPort>())
            .Returns(_expectedTemporaryMappedPortIpcEntity);
        _entityMapper.Map<PortMappingStatus, PortMappingStatusIpcEntity>(Arg.Any<PortMappingStatus>())
            .Returns(EXPECTED_PORT_MAPPING_STATUS_IPC_ENTITY);

        _expectedTemporaryMappedPort = new TemporaryMappedPort();
        _entityMapper.Map<TemporaryMappedPortIpcEntity, TemporaryMappedPort>(Arg.Any<TemporaryMappedPortIpcEntity>())
            .Returns(_expectedTemporaryMappedPort);
        _entityMapper.Map<PortMappingStatusIpcEntity, PortMappingStatus>(Arg.Any<PortMappingStatusIpcEntity>())
            .Returns(EXPECTED_PORT_MAPPING_STATUS);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _entityMapper = null;
        _mapper = null;
        _expectedTemporaryMappedPortIpcEntity = null;
        _expectedTemporaryMappedPort = null;
    }

    [TestMethod]
    public void TestMapLeftToRight_WhenNull()
    {
        PortForwardingState entityToTest = null;

        PortForwardingStateIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void TestMapLeftToRight()
    {
        PortForwardingState entityToTest = new()
        {
            MappedPort = new TemporaryMappedPort(),
            Status = PortMappingStatus.Error,
            TimestampUtc = DateTime.UtcNow
        };

        PortForwardingStateIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(_expectedTemporaryMappedPortIpcEntity, result.MappedPort);
        Assert.AreEqual(EXPECTED_PORT_MAPPING_STATUS_IPC_ENTITY, result.Status);
        Assert.AreEqual(entityToTest.TimestampUtc, result.TimestampUtc);
    }

    [TestMethod]
    public void TestMapRightToLeft_WhenNull()
    {
        PortForwardingStateIpcEntity entityToTest = null;

        PortForwardingState result = _mapper.Map(entityToTest);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void TestMapRightToLeft()
    {
        PortForwardingStateIpcEntity entityToTest = new()
        {
            MappedPort = new TemporaryMappedPortIpcEntity(),
            Status = PortMappingStatusIpcEntity.DestroyPortMappingCommunication,
            TimestampUtc = DateTime.UtcNow
        };

        PortForwardingState result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(_expectedTemporaryMappedPort, result.MappedPort);
        Assert.AreEqual(EXPECTED_PORT_MAPPING_STATUS, result.Status);
        Assert.AreEqual(entityToTest.TimestampUtc, result.TimestampUtc);
    }
}