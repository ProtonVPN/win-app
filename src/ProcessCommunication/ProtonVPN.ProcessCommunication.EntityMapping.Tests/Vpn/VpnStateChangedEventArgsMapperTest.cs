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
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Vpn;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.ProcessCommunication.EntityMapping.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Tests.Vpn;

[TestClass]
public class VpnStateChangedEventArgsMapperTest
{
    private IEntityMapper _entityMapper;
    private VpnStateChangedEventArgsMapper _mapper;

    private VpnStatusIpcEntity? _expectedVpnStatusIpcEntity;
    private VpnErrorTypeIpcEntity? _expectedVpnErrorTypeIpcEntity;
    private OpenVpnAdapterIpcEntity? _expectedOpenVpnAdapterIpcEntity;
    private VpnProtocolIpcEntity? _expectedVpnProtocolIpcEntity;
    private VpnStatus? _expectedVpnStatus;
    private VpnError? _expectedVpnError;
    private OpenVpnAdapter? _expectedOpenVpnAdapter;
    private VpnProtocol? _expectedVpnProtocol;

    [TestInitialize]
    public void Initialize()
    {
        _entityMapper = Substitute.For<IEntityMapper>();
        _mapper = new(_entityMapper);

        _expectedVpnStatusIpcEntity = VpnStatusIpcEntity.Connecting;
        _entityMapper.Map<VpnStatus, VpnStatusIpcEntity>(Arg.Any<VpnStatus>())
            .Returns(_expectedVpnStatusIpcEntity.Value);

        _expectedVpnErrorTypeIpcEntity = VpnErrorTypeIpcEntity.TapAdapterInUseError;
        _entityMapper.Map<VpnError, VpnErrorTypeIpcEntity>(Arg.Any<VpnError>())
            .Returns(_expectedVpnErrorTypeIpcEntity.Value);

        _expectedOpenVpnAdapterIpcEntity = OpenVpnAdapterIpcEntity.Tun;
        _entityMapper.MapNullableStruct<OpenVpnAdapter, OpenVpnAdapterIpcEntity>(Arg.Any<OpenVpnAdapter?>())
            .Returns(_expectedOpenVpnAdapterIpcEntity);

        _expectedVpnProtocolIpcEntity = VpnProtocolIpcEntity.OpenVpnUdp;
        _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(Arg.Any<VpnProtocol>())
            .Returns(_expectedVpnProtocolIpcEntity.Value);

        _expectedVpnStatus = VpnStatus.Authenticating;
        _entityMapper.Map<VpnStatusIpcEntity, VpnStatus>(Arg.Any<VpnStatusIpcEntity>())
            .Returns(_expectedVpnStatus.Value);

        _expectedVpnError = VpnError.CertificateRevoked;
        _entityMapper.Map<VpnErrorTypeIpcEntity, VpnError>(Arg.Any<VpnErrorTypeIpcEntity>())
            .Returns(_expectedVpnError.Value);

        _expectedOpenVpnAdapter = OpenVpnAdapter.Tun;
        _entityMapper.MapNullableStruct<OpenVpnAdapterIpcEntity, OpenVpnAdapter>(Arg.Any<OpenVpnAdapterIpcEntity?>())
            .Returns(_expectedOpenVpnAdapter);

        _expectedVpnProtocol = VpnProtocol.OpenVpnTcp;
        _entityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(Arg.Any<VpnProtocolIpcEntity>())
            .Returns(_expectedVpnProtocol.Value);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _entityMapper = null;
        _mapper = null;

        _expectedVpnStatusIpcEntity = null;
        _expectedVpnErrorTypeIpcEntity = null;
        _expectedOpenVpnAdapterIpcEntity = null;
        _expectedVpnProtocolIpcEntity = null;
        _expectedVpnStatus = null;
        _expectedVpnError = null;
        _expectedOpenVpnAdapter = null;
        _expectedVpnProtocol = null;
    }

    [TestMethod]
    public void TestMapLeftToRight_UsingMultipleArgumentConstructor()
    {
        VpnStateChangedEventArgs entityToTest = new(
            status: VpnStatus.AssigningIp,
            error: VpnError.TlsError,
            endpointIp: "192.168.0.0",
            networkBlocked: true,
            vpnProtocol: VpnProtocol.OpenVpnTcp,
            networkAdapterType: OpenVpnAdapter.Tun,
            label: "Proton VPN");

        VpnStateIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(_expectedVpnStatusIpcEntity, result.Status);
        Assert.AreEqual(_expectedVpnErrorTypeIpcEntity, result.Error);
        Assert.AreEqual(entityToTest.NetworkBlocked, result.NetworkBlocked);
        Assert.AreEqual(entityToTest.State.EntryIp, result.EndpointIp);
        Assert.AreEqual(_expectedOpenVpnAdapterIpcEntity, result.OpenVpnAdapterType);
        Assert.AreEqual(_expectedVpnProtocolIpcEntity, result.VpnProtocol);
        Assert.AreEqual(entityToTest.State.Label, result.Label);
    }

    [TestMethod]
    public void TestMapLeftToRight_UsingStateArgumentConstructor()
    {
        VpnStateChangedEventArgs entityToTest = new(
            status: VpnStatus.Authenticating,
            error: VpnError.NoServers,
            server: Server.Empty(),
            networkBlocked: true);

        VpnStateIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(_expectedVpnStatusIpcEntity, result.Status);
        Assert.AreEqual(_expectedVpnErrorTypeIpcEntity, result.Error);
        Assert.AreEqual(entityToTest.NetworkBlocked, result.NetworkBlocked);
        Assert.AreEqual(entityToTest.State.EntryIp, result.EndpointIp);
        Assert.AreEqual(_expectedOpenVpnAdapterIpcEntity, result.OpenVpnAdapterType);
        Assert.AreEqual(_expectedVpnProtocolIpcEntity, result.VpnProtocol);
        Assert.AreEqual(entityToTest.State.Label, result.Label);
    }

    [TestMethod]
    public void TestMapLeftToRight_UsingVpnStateArgumentConstructor()
    {
        VpnStateChangedEventArgs entityToTest = new(
            state: new VpnState(
                status: VpnStatus.RetrievingConfiguration,
                entryIp: "172.16.0.0",
                vpnProtocol: VpnProtocol.WireGuardUdp,
                networkAdapterType: OpenVpnAdapter.Tun,
                label: "Proton VPN"
            ),
            error: VpnError.IncorrectVpnConfig,
            networkBlocked: true);

        VpnStateIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(_expectedVpnStatusIpcEntity, result.Status);
        Assert.AreEqual(_expectedVpnErrorTypeIpcEntity, result.Error);
        Assert.AreEqual(entityToTest.NetworkBlocked, result.NetworkBlocked);
        Assert.AreEqual(entityToTest.State.EntryIp, result.EndpointIp);
        Assert.AreEqual(_expectedOpenVpnAdapterIpcEntity, result.OpenVpnAdapterType);
        Assert.AreEqual(_expectedVpnProtocolIpcEntity, result.VpnProtocol);
        Assert.AreEqual(entityToTest.State.Label, result.Label);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestMapRightToLeft_ThrowsWhenNull()
    {
        VpnStateIpcEntity entityToTest = null;

        _mapper.Map(entityToTest);
    }

    [TestMethod]
    public void TestMapRightToLeft()
    {
        VpnStateIpcEntity entityToTest = new();

        VpnStateChangedEventArgs result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(_expectedVpnStatus, result.State.Status);
        Assert.AreEqual(_expectedVpnError, result.Error);
        Assert.AreEqual(entityToTest.NetworkBlocked, result.NetworkBlocked);
        Assert.AreEqual(entityToTest.EndpointIp, result.State.EntryIp);
        Assert.AreEqual(_expectedOpenVpnAdapter, result.State.NetworkAdapterType);
        Assert.AreEqual(_expectedVpnProtocol, result.State.VpnProtocol);
        Assert.AreEqual(entityToTest.Label, result.State.Label);
    }
}
