/*
 * Copyright (c) 2025 Proton AG
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
using NSubstitute.Core;
using ProtonVPN.Common.Legacy;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.ProcessCommunication.EntityMapping.Vpn;

namespace ProtonVPN.ProcessCommunication.EntityMapping.Tests.Vpn;

[TestClass]
public class VpnConfigMapperTest
{
    private IEntityMapper _entityMapper;
    private VpnConfigMapper _mapper;

    private SplitTunnelModeIpcEntity? _expectedSplitTunnelModeIpcEntity;
    private List<VpnProtocolIpcEntity> _expectedVpnProtocolIpcEntities;

    private SplitTunnelMode? _expectedSplitTunnelMode;
    private List<VpnProtocol> _expectedVpnProtocols;

    [TestInitialize]
    public void Initialize()
    {
        _entityMapper = Substitute.For<IEntityMapper>();
        _mapper = new(_entityMapper);

        _expectedSplitTunnelModeIpcEntity = SplitTunnelModeIpcEntity.Block;
        _entityMapper.Map<SplitTunnelMode, SplitTunnelModeIpcEntity>(Arg.Any<SplitTunnelMode>())
            .Returns(_expectedSplitTunnelModeIpcEntity.Value);

        _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(Arg.Any<VpnProtocol>())
            .Returns((CallInfo callInfo) => (VpnProtocolIpcEntity)(int)callInfo[0]);

        _expectedVpnProtocolIpcEntities = new List<VpnProtocolIpcEntity>() { VpnProtocolIpcEntity.OpenVpnUdp };
        _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(Arg.Any<IEnumerable<VpnProtocol>>())
            .Returns(_expectedVpnProtocolIpcEntities);

        _expectedSplitTunnelMode = SplitTunnelMode.Block;
        _entityMapper.Map<SplitTunnelModeIpcEntity, SplitTunnelMode>(Arg.Any<SplitTunnelModeIpcEntity>())
            .Returns(_expectedSplitTunnelMode.Value);

        _entityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(Arg.Any<VpnProtocolIpcEntity>())
            .Returns((CallInfo callInfo) => (VpnProtocol)(int)callInfo[0]);

        _expectedVpnProtocols = new List<VpnProtocol>() { VpnProtocol.OpenVpnUdp };
        _entityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(Arg.Any<IEnumerable<VpnProtocolIpcEntity>>())
            .Returns(_expectedVpnProtocols);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _entityMapper = null;
        _mapper = null;

        _expectedSplitTunnelModeIpcEntity = null;
        _expectedVpnProtocolIpcEntities = null;

        _expectedSplitTunnelMode = null;
        _expectedVpnProtocols = null;
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestMapLeftToRight_ThrowsWhenNull()
    {
        VpnConfig entityToTest = null;

        _mapper.Map(entityToTest);
    }

    [TestMethod]
    public void TestMapLeftToRight()
    {
        VpnConfig entityToTest = new(new VpnConfigParameters()
        {
            Ports = new Dictionary<VpnProtocol, IReadOnlyCollection<int>>()
            {
                { VpnProtocol.WireGuardUdp, new List<int>() { 80, 443 } },
                { VpnProtocol.OpenVpnUdp, new List<int>() { 8080, 1 } }
            },
            CustomDns = new List<string>() { "172.16.0.0" },
            SplitTunnelMode = SplitTunnelMode.Block,
            SplitTunnelIPs = new List<string>() { "192.168.0.0" },
            OpenVpnAdapter = OpenVpnAdapter.Tun,
            VpnProtocol = VpnProtocol.OpenVpnUdp,
            PreferredProtocols = new List<VpnProtocol>() { VpnProtocol.OpenVpnTcp },
            NetShieldMode = 2,
            SplitTcp = true,
            ModerateNat = true,
            PortForwarding = true,
            IsIpv6Enabled = true,
        });

        VpnConfigIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        AssertPortsAreEquivalent(entityToTest, result);
        CollectionAssert.AreEqual(entityToTest.CustomDns.ToList(), result.CustomDns);
        Assert.AreEqual(_expectedSplitTunnelModeIpcEntity, result.SplitTunnelMode);
        CollectionAssert.AreEqual(entityToTest.SplitTunnelIPs.ToList(), result.SplitTunnelIPs);
        Assert.AreEqual(entityToTest.NetShieldMode, result.NetShieldMode);
        Assert.AreEqual((int)entityToTest.VpnProtocol, (int)result.VpnProtocol);
        Assert.AreEqual(entityToTest.ModerateNat, result.ModerateNat);
        Assert.AreEqual(_expectedVpnProtocolIpcEntities, result.PreferredProtocols);
        Assert.AreEqual(entityToTest.SplitTcp, result.SplitTcp);
        Assert.AreEqual(entityToTest.PortForwarding, result.PortForwarding);
        Assert.AreEqual(entityToTest.IsIpv6Enabled, result.IsIpv6Enabled);
    }

    private void AssertPortsAreEquivalent(VpnConfig entityToTest, VpnConfigIpcEntity result)
    {
        Assert.IsNotNull(result.Ports);

        List<KeyValuePair<VpnProtocol, IReadOnlyCollection<int>>> leftEntityDictionary = entityToTest.Ports.ToList();
        List<KeyValuePair<VpnProtocolIpcEntity, int[]>> rightEntityDictionary = result.Ports.ToList();
        Assert.AreEqual(leftEntityDictionary.Count, rightEntityDictionary.Count);

        for (int keyValuePairIndex = 0; keyValuePairIndex < leftEntityDictionary.Count; keyValuePairIndex++)
        {
            Assert.AreEqual((int)leftEntityDictionary[keyValuePairIndex].Key, (int)rightEntityDictionary[keyValuePairIndex].Key);
            CollectionAssert.AreEqual(
                leftEntityDictionary[keyValuePairIndex].Value.ToList(),
                rightEntityDictionary[keyValuePairIndex].Value);
        }
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestMapRightToLeft_ThrowsWhenNull()
    {
        VpnConfigIpcEntity entityToTest = null;

        _mapper.Map(entityToTest);
    }

    [TestMethod]
    public void TestMapRightToLeft()
    {
        VpnConfigIpcEntity entityToTest = new()
        {
            Ports = new Dictionary<VpnProtocolIpcEntity, int[]>()
            {
                { VpnProtocolIpcEntity.WireGuardUdp, new int[] { 80, 443 } },
                { VpnProtocolIpcEntity.OpenVpnUdp, new int[] { 8080, 1 } }
            },
            CustomDns = new List<string>() { "172.16.0.0" },
            SplitTunnelMode = SplitTunnelModeIpcEntity.Block,
            SplitTunnelIPs = new List<string>() { "192.168.0.0" },
            NetShieldMode = 2,
            VpnProtocol = VpnProtocolIpcEntity.OpenVpnUdp,
            PreferredProtocols = new List<VpnProtocolIpcEntity>() { VpnProtocolIpcEntity.OpenVpnTcp },
            SplitTcp = true,
            ModerateNat = true,
            PortForwarding = true,
        };

        VpnConfig result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        AssertPortsAreEquivalent(entityToTest, result);
        CollectionAssert.AreEqual(entityToTest.CustomDns, result.CustomDns.ToList());
        Assert.AreEqual(_expectedSplitTunnelMode, result.SplitTunnelMode);
        CollectionAssert.AreEqual(entityToTest.SplitTunnelIPs, result.SplitTunnelIPs.ToList());
        Assert.AreEqual(entityToTest.NetShieldMode, result.NetShieldMode);
        Assert.AreEqual((int)entityToTest.VpnProtocol, (int)result.VpnProtocol);
        Assert.AreEqual(_expectedVpnProtocols, result.PreferredProtocols);
        Assert.AreEqual(entityToTest.SplitTcp, result.SplitTcp);
        Assert.AreEqual(entityToTest.ModerateNat, result.ModerateNat);
        Assert.AreEqual(entityToTest.PortForwarding, result.PortForwarding);
    }

    private void AssertPortsAreEquivalent(VpnConfigIpcEntity entityToTest, VpnConfig result)
    {
        Assert.IsNotNull(result.Ports);

        List<KeyValuePair<VpnProtocolIpcEntity, int[]>> leftEntityDictionary = entityToTest.Ports.ToList();
        List<KeyValuePair<VpnProtocol, IReadOnlyCollection<int>>> rightEntityDictionary = result.Ports.ToList();
        Assert.AreEqual(leftEntityDictionary.Count, rightEntityDictionary.Count);

        for (int keyValuePairIndex = 0; keyValuePairIndex < leftEntityDictionary.Count; keyValuePairIndex++)
        {
            Assert.AreEqual((int)leftEntityDictionary[keyValuePairIndex].Key, (int)rightEntityDictionary[keyValuePairIndex].Key);
            CollectionAssert.AreEqual(
                leftEntityDictionary[keyValuePairIndex].Value,
                rightEntityDictionary[keyValuePairIndex].Value.ToList());
        }
    }
}