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

using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.Connection;

namespace ProtonVPN.Vpn.Tests.Connection;

[TestClass]
public class VpnEndpointCandidatesTest
{
    private const string SERVER_LABEL = "0";

    private VpnEndpointCandidates _candidates;

    [TestInitialize]
    public void Initialize()
    {
        _candidates = new VpnEndpointCandidates();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _candidates = null;
    }

    [TestMethod]
    public void NextIp_ShouldAdvanceThroughRelayOnlyHosts_WhenTopLevelIpIsEmpty()
    {
        // Arrange: Guest Hole style hosts with an empty top-level Ip, relying solely on RelayIpByProtocol.
        VpnHost firstHost = CreateRelayOnlyHost("first.protonvpn.net", "51.75.169.79");
        VpnHost secondHost = CreateRelayOnlyHost("second.protonvpn.net", "51.158.231.46");

        _candidates.Set([firstHost, secondHost]);

        VpnConfig config = CreateConfig();

        // Act
        VpnEndpoint firstEndpoint = _candidates.NextIp(config);
        VpnEndpoint secondEndpoint = _candidates.NextIp(config);
        VpnEndpoint thirdEndpoint = _candidates.NextIp(config);

        // Assert
        firstEndpoint.Server.Should().Be(firstHost);
        secondEndpoint.Server.Should().Be(secondHost);
        thirdEndpoint.IsEmpty.Should().BeTrue();
    }

    [TestMethod]
    public void NextIp_ShouldNotLoopForever_WhenAllRelayOnlyHostsAreExhausted()
    {
        // Arrange
        VpnHost host = CreateRelayOnlyHost("only.protonvpn.net", "51.75.169.79");
        _candidates.Set([host]);

        VpnConfig config = CreateConfig();

        // Act
        VpnEndpoint firstEndpoint = _candidates.NextIp(config);
        VpnEndpoint secondEndpoint = _candidates.NextIp(config);

        // Assert
        firstEndpoint.Server.Should().Be(host);
        secondEndpoint.IsEmpty.Should().BeTrue();
    }

    [TestMethod]
    public void NextIp_ShouldAdvance_WhenHostHasRegularNonEmptyIp()
    {
        // Arrange: legacy/regular hosts without RelayIpByProtocol still rely on the top-level Ip.
        VpnHost firstHost = CreateRegularHost("first.protonvpn.net", "10.0.0.1");
        VpnHost secondHost = CreateRegularHost("second.protonvpn.net", "10.0.0.2");

        _candidates.Set([firstHost, secondHost]);

        VpnConfig config = CreateConfig();

        // Act
        VpnEndpoint firstEndpoint = _candidates.NextIp(config);
        VpnEndpoint secondEndpoint = _candidates.NextIp(config);
        VpnEndpoint thirdEndpoint = _candidates.NextIp(config);

        // Assert
        firstEndpoint.Server.Should().Be(firstHost);
        secondEndpoint.Server.Should().Be(secondHost);
        thirdEndpoint.IsEmpty.Should().BeTrue();
    }

    [TestMethod]
    public void NextIp_ShouldNotLoopForever_WhenAllRegularHostsAreExhausted()
    {
        // Arrange
        VpnHost host = CreateRegularHost("only.protonvpn.net", "10.0.0.1");
        _candidates.Set([host]);

        VpnConfig config = CreateConfig();

        // Act
        VpnEndpoint firstEndpoint = _candidates.NextIp(config);
        VpnEndpoint secondEndpoint = _candidates.NextIp(config);

        // Assert
        firstEndpoint.Server.Should().Be(host);
        secondEndpoint.IsEmpty.Should().BeTrue();
    }

    [TestMethod]
    public void NextIp_ShouldAdvanceThroughBoth_WhenServerListMixesRegularAndRelayOnlyHosts()
    {
        // Arrange
        VpnHost regularHost = CreateRegularHost("first.protonvpn.net", "10.0.0.1");
        VpnHost relayOnlyHost = CreateRelayOnlyHost("second.protonvpn.net", "51.158.231.46");

        _candidates.Set([regularHost, relayOnlyHost]);

        VpnConfig config = CreateConfig();

        // Act
        VpnEndpoint firstEndpoint = _candidates.NextIp(config);
        VpnEndpoint secondEndpoint = _candidates.NextIp(config);
        VpnEndpoint thirdEndpoint = _candidates.NextIp(config);

        // Assert
        firstEndpoint.Server.Should().Be(regularHost);
        secondEndpoint.Server.Should().Be(relayOnlyHost);
        thirdEndpoint.IsEmpty.Should().BeTrue();
    }

    [TestMethod]
    public void NextIp_ShouldReturnEmpty_WhenNoHostsAreSet()
    {
        // Arrange
        _candidates.Set([]);

        VpnConfig config = CreateConfig();

        // Act
        VpnEndpoint endpoint = _candidates.NextIp(config);

        // Assert
        endpoint.IsEmpty.Should().BeTrue();
    }

    [TestMethod]
    public void NextIp_ShouldSkipSecondHost_WhenBothHostsResolveToSameIp_WithRegularHostFirst()
    {
        // Arrange
        const string sharedIp = "51.158.231.46";
        VpnHost firstHost = CreateRegularHost("first.protonvpn.net", sharedIp);
        VpnHost secondHost = CreateRelayOnlyHost("second.protonvpn.net", sharedIp);

        _candidates.Set([firstHost, secondHost]);

        VpnConfig config = CreateConfig();

        // Act
        VpnEndpoint firstEndpoint = _candidates.NextIp(config);
        VpnEndpoint secondEndpoint = _candidates.NextIp(config);

        // Assert
        firstEndpoint.Server.Should().Be(firstHost);
        secondEndpoint.IsEmpty.Should().BeTrue();
    }


    [TestMethod]
    public void NextIp_ShouldSkipSecondHost_WhenBothHostsResolveToSameIp_WithRelayOnlyHostFirst()
    {
        // Arrange
        const string sharedIp = "51.158.231.46";
        VpnHost firstHost = CreateRelayOnlyHost("second.protonvpn.net", sharedIp);
        VpnHost secondHost = CreateRegularHost("first.protonvpn.net", sharedIp);

        _candidates.Set([firstHost, secondHost]);

        VpnConfig config = CreateConfig();

        // Act
        VpnEndpoint firstEndpoint = _candidates.NextIp(config);
        VpnEndpoint secondEndpoint = _candidates.NextIp(config);

        // Assert
        firstEndpoint.Server.Should().Be(firstHost);
        secondEndpoint.IsEmpty.Should().BeTrue();
    }

    private static VpnHost CreateRegularHost(string name, string ip)
    {
        return new VpnHost(name, ip, SERVER_LABEL, null, string.Empty, false, null);
    }

    private static VpnHost CreateRelayOnlyHost(string name, string wireGuardTlsRelayIp)
    {
        Dictionary<VpnProtocol, string> relayIpByProtocol = new()
        {
            { VpnProtocol.WireGuardTls, wireGuardTlsRelayIp }
        };

        return new VpnHost(name, string.Empty, SERVER_LABEL, null, string.Empty, false, relayIpByProtocol);
    }

    private static VpnConfig CreateConfig()
    {
        return new VpnConfig(new VpnConfigParameters
        {
            VpnProtocol = VpnProtocol.Smart,
            PreferredProtocols = [VpnProtocol.WireGuardTls, VpnProtocol.OpenVpnTcp]
        });
    }
}
