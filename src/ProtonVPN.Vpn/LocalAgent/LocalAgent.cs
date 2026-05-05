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

using System;
using System.Net;
using Newtonsoft.Json;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Go;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectionLogs;
using ProtonVPN.Logging.Contracts.Events.LocalAgentLogs;
using ProtonVPN.Vpn.Config;
using ProtonVPN.Vpn.Connection;
using ProtonVPN.Vpn.Gateways;
using ProtonVPN.Vpn.LocalAgent.Contracts;

namespace ProtonVPN.Vpn.LocalAgent;

internal class LocalAgent : ILocalAgent
{
    private const int MINIMUM_NETSHIELD_STATS_TIMEOUT_IN_SECONDS = 20;
    private const int DEFAULT_PORT = 65432;

    private readonly ILogger _logger;
    private readonly IGatewayCache _gatewayCache;

    public VpnError LastError { get; private set; }
    private VpnConfig? _vpnConfig;
    private bool _isTlsChannelActive;
    private DateTime _lastNetShieldStatsRequestDate = DateTime.MinValue;

    public LocalAgent(
        ILogger logger,
        IGatewayCache gatewayCache)
    {
        _logger = logger;
        _gatewayCache = gatewayCache;
    }

    public bool ConnectToTlsChannel(LocalAgentConnectParams localAgentConnectParams)
    {
        IPAddress? gatewayIPAddress = _gatewayCache.Get();
        if (gatewayIPAddress == null)
        {
            _logger.Error<ConnectionErrorLog>("Default gateway is missing. Disconnecting.");
            LastError = VpnError.Unknown;
            return false;
        }

        using GoString clientCertPem = localAgentConnectParams.ClientCertPem.ToGoString();
        using GoString clientKeyPem = localAgentConnectParams.ClientSecretPem.ToGoString();
        using GoString serverCaPem = VpnCertConfig.ROOT_CA.ToGoString();
        using GoString host = $"{gatewayIPAddress}:{DEFAULT_PORT}".ToGoString();
        using GoString featuresJson = GetFeatures(localAgentConnectParams).ToGoString();
        using GoString certServerName = localAgentConnectParams.Server.Name.ToGoString();

        string result = PInvoke.Connect(
            clientCertPem,
            clientKeyPem,
            serverCaPem,
            host,
            certServerName,
            featuresJson,
            connectivity: true,
            keepAliveSeconds: 60,
            // Zero falls back to the default value of 9
            keepAliveMaxCount: 0).ConvertToString();

        if (result == "")
        {
            _logger.Info<LocalAgentLog>("Channel opened.");
        }
        else
        {
            _logger.Error<LocalAgentLog>("Failed to connect to TLS channel: " + result);
            LastError = GetVpnError(result);
            return false;
        }

        _isTlsChannelActive = true;

        return true;
    }

    public void CloseTlsChannel()
    {
        if (_isTlsChannelActive)
        {
            _isTlsChannelActive = false;
            PInvoke.Close();
            _logger.Info<LocalAgentLog>("Channel closed.");
        }
    }

    private static string GetFeatures(LocalAgentConnectParams localAgentConnectParams)
    {
        return GetFeaturesJson(new FeaturesContract
        {
            Bouncing = localAgentConnectParams.Server.Label,
            SplitTcp = localAgentConnectParams.VpnConfig.SplitTcp,
            NetShieldLevel = localAgentConnectParams.VpnConfig.NetShieldMode,
            PortForwarding = localAgentConnectParams.VpnConfig.PortForwarding,
            RandomizedNat = !localAgentConnectParams.VpnConfig.ModerateNat,
        });
    }

    private static VpnError GetVpnError(string result)
    {
        return result.Contains("private key does not match public key")
            ? VpnError.ClientKeyMismatch
            : VpnError.Unknown;
    }

    public void SetFeatures(VpnFeatures vpnFeatures)
    {
        if (!_isTlsChannelActive)
        {
            return;
        }

        UpdateVpnConfig(vpnFeatures);
        using GoString goFeatures = GetFeatures(vpnFeatures).ToGoString();
        PInvoke.SetFeatures(goFeatures);
    }

    private void UpdateVpnConfig(VpnFeatures vpnFeatures)
    {
        if (_vpnConfig == null)
        {
            return;
        }

        _vpnConfig = new VpnConfig(new()
        {
            Ports = _vpnConfig.Ports,
            CustomDns = _vpnConfig.CustomDns,
            SplitTunnelMode = _vpnConfig.SplitTunnelMode,
            SplitTunnelIPs = _vpnConfig.SplitTunnelIPs,
            OpenVpnAdapter = _vpnConfig.OpenVpnAdapter,
            VpnProtocol = _vpnConfig.VpnProtocol,
            PreferredProtocols = _vpnConfig.PreferredProtocols,
            NetShieldMode = vpnFeatures.NetShieldMode,
            SplitTcp = vpnFeatures.SplitTcp,
            PortForwarding = vpnFeatures.PortForwarding,
            IsIpv6Enabled = _vpnConfig.IsIpv6Enabled,
            WireGuardConnectionTimeout = _vpnConfig.WireGuardConnectionTimeout,
            DnsBlockMode = _vpnConfig.DnsBlockMode,
        });
    }

    public void RequestNetShieldStats()
    {
        if (_lastNetShieldStatsRequestDate.AddSeconds(MINIMUM_NETSHIELD_STATS_TIMEOUT_IN_SECONDS) < DateTime.UtcNow
            && _isTlsChannelActive)
        {
            _lastNetShieldStatsRequestDate = DateTime.UtcNow;
            PInvoke.SendGetStatus(true);
        }
    }

    private static string GetFeatures(VpnFeatures vpnFeatures)
    {
        return GetFeaturesJson(new FeaturesContract
        {
            SplitTcp = vpnFeatures.SplitTcp,
            NetShieldLevel = vpnFeatures.NetShieldMode,
            PortForwarding = vpnFeatures.PortForwarding,
            RandomizedNat = !vpnFeatures.ModerateNat,
        });
    }

    private static string GetFeaturesJson(FeaturesContract contract)
    {
        return JsonConvert.SerializeObject(contract, Formatting.None, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });
    }
}