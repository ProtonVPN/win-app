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

using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Logic.Servers.Tests;

[TestClass]
public class ServerAvailabilityTest
{
    private static readonly IList<VpnProtocol> _preferredProtocols = [VpnProtocol.WireGuardUdp];

    [TestMethod]
    public void IsAvailable_ReturnsTrue_WhenTheLogicalIsUpAndItsNodesReportMaintenance()
    {
        Server server = CreateServer(status: 1, CreatePhysicalServer(status: 0));

        // A node status is only refreshed by a logicals fetch, so honouring it here would hold the
        // server out of service for up to the full logicals interval on stale data.
        Assert.IsTrue(server.IsAvailable(_preferredProtocols));
    }

    [TestMethod]
    public void IsAvailable_ReturnsFalse_WhenTheLogicalIsUnderMaintenance()
    {
        Server server = CreateServer(status: 0, CreatePhysicalServer(status: 1));

        Assert.IsFalse(server.IsAvailable(_preferredProtocols));
    }

    [TestMethod]
    public void IsAvailable_ReturnsFalse_WhenNoNodeHasAnEntryAddressForThePreferredProtocols()
    {
        Server server = CreateServer(status: 1, CreatePhysicalServer(status: 1, entryIp: string.Empty));

        // Dropping the node status must not drop the entry address check with it.
        Assert.IsFalse(server.IsAvailable(_preferredProtocols));
    }

    [TestMethod]
    public void IsAvailable_ReturnsFalse_WhenTheLogicalHasNoNodes()
    {
        Server server = CreateServer(status: 1);

        // Higher tier servers are published without their nodes, they must not look connectable.
        Assert.IsFalse(server.IsAvailable(_preferredProtocols));
    }

    [TestMethod]
    public void IsAvailable_ReturnsTrue_WhenOnlyARelayAddressMatchesAPreferredProtocol()
    {
        PhysicalServer physicalServer = CreatePhysicalServer(status: 1, entryIp: string.Empty);
        physicalServer.RelayIpByProtocol.Add(VpnProtocol.WireGuardUdp, "1.2.3.4");

        Assert.IsTrue(physicalServer.IsAvailable(_preferredProtocols));
        Assert.IsFalse(physicalServer.IsAvailable([VpnProtocol.OpenVpnTcp]));
    }

    private static Server CreateServer(sbyte status, params PhysicalServer[] physicalServers)
    {
        return new Server
        {
            Id = "server-id",
            Name = "CH#1",
            City = "Zurich",
            State = string.Empty,
            EntryCountry = "CH",
            ExitCountry = "CH",
            HostCountry = string.Empty,
            Domain = "node-ch-01.protonvpn.net",
            GatewayName = string.Empty,
            Status = status,
            StatusReference = new StatusReference(),
            EntryLocation = new GeoLocation(),
            ExitLocation = new GeoLocation(),
            Servers = [.. physicalServers],
        };
    }

    private static PhysicalServer CreatePhysicalServer(sbyte status, string entryIp = "1.2.3.4")
    {
        return new PhysicalServer
        {
            Id = "physical-server-id",
            EntryIp = entryIp,
            ExitIp = entryIp,
            Domain = "node-ch-01.protonvpn.net",
            Label = "0",
            Status = status,
            X25519PublicKey = string.Empty,
            Signature = string.Empty,
        };
    }
}
