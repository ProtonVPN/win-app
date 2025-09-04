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

using System.Collections.Generic;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Configurations.Contracts.WireGuard;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.OperatingSystemLogs;
using ProtonVPN.OperatingSystems.NRPT.Contracts;
using ProtonVPN.Vpn.OpenVpn.DnsServers;

namespace ProtonVPN.Vpn.NRPT;

public class NrptWrapper : INrptWrapper
{
    private readonly INrptInvoker _nrptInvoker;
    private readonly ILogger _logger;
    private readonly IIssueReporter _issueReporter;
    private readonly IWireGuardDnsServersCreator _wireGuardDnsServersCreator;
    private readonly IOpenVpnDnsServersCreator _openVpnDnsServersCreator;

    private IReadOnlyCollection<string> _customDns = [];
    private VpnProtocol _vpnProtocol;
    private bool _isIpv6Supported;

    public NrptWrapper(INrptInvoker nrptInvoker,
        ILogger logger,
        IIssueReporter issueReporter,
        IWireGuardDnsServersCreator wireGuardDnsServersCreator,
        IOpenVpnDnsServersCreator openVpnDnsServersCreator)
    {
        _nrptInvoker = nrptInvoker;
        _logger = logger;
        _issueReporter = issueReporter;
        _wireGuardDnsServersCreator = wireGuardDnsServersCreator;
        _openVpnDnsServersCreator = openVpnDnsServersCreator;
    }

    public void SetConnectionConfig(IReadOnlyCollection<string> customDns,
        VpnProtocol vpnProtocol, bool isIpv6Supported)
    {
        _customDns = customDns;
        _vpnProtocol = vpnProtocol;
        _isIpv6Supported = isIpv6Supported;
    }

    /// <returns>If the NRPT rule was added successfully</returns>
    public bool CreateRule()
    {
        IDnsServersCreator dnsServersCreator = GetDnsServersCreator();
        string nameServers = dnsServersCreator.GetDnsServers(_customDns, _isIpv6Supported);

        if (string.IsNullOrWhiteSpace(nameServers))
        {
            const string title = "No DNS servers when creating NRPT rule. No NRPT rule will be created.";
            string details = $"[{dnsServersCreator.GetType().Name}] IPv6: {_isIpv6Supported}, Custom DNS servers: {_customDns.Count}";
            _logger.Error<OperatingSystemNrptLog>($"{title} {details}");
            _issueReporter.CaptureError(title, details);
            return false;
        }

        return _nrptInvoker.CreateRule(nameServers);
    }

    private IDnsServersCreator GetDnsServersCreator()
    {
        return _vpnProtocol.IsOpenVpn() ? _openVpnDnsServersCreator : _wireGuardDnsServersCreator;
    }

    /// <returns>If the NRPT rule was removed successfully</returns>
    public bool DeleteRule()
    {
        return _nrptInvoker.DeleteRule();
    }
}
