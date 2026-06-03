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

using System.IO;
using System.Security.AccessControl;
using System.Text;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Configurations.Contracts.WireGuard;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.Vpn.Common;

namespace ProtonVPN.Vpn.WireGuard;

public class WireGuardConfigFileCreator : IWireGuardConfigFileCreator
{
    private const string IPV4_ALLOWED_IP = "0.0.0.0/0";
    private const string IPV6_ALLOWED_IP = "::/0";

    // SYSTEM: Full Control; Administrators: Delete only; inheritance blocked.
    // Only processes running under the SYSTEM account can read.
    private const string SECURE_SDDL = "O:SYG:SYD:P(A;;FA;;;SY)(A;;SD;;;BA)";

    private readonly IStaticConfiguration _staticConfig;
    private readonly IX25519KeyGenerator _x25519KeyGenerator;
    private readonly IWireGuardDnsServersCreator _wireGuardDnsServersCreator;

    public WireGuardConfigFileCreator(
        IStaticConfiguration staticConfig,
        IX25519KeyGenerator x25519KeyGenerator,
        IWireGuardDnsServersCreator wireGuardDnsServersCreator)
    {
        _staticConfig = staticConfig;
        _x25519KeyGenerator = x25519KeyGenerator;
        _wireGuardDnsServersCreator = wireGuardDnsServersCreator;
    }

    public void Create(VpnEndpoint endpoint, VpnCredentials credentials, VpnConfig vpnConfig)
    {
        CreateDirectory();
        string configContent = GenerateConfig(endpoint, credentials, vpnConfig);
        WriteConfigSecurely(_staticConfig.WireGuard.ConfigFilePath, configContent);
    }

    private void CreateDirectory()
    {
        string? directoryPath = Path.GetDirectoryName(_staticConfig.WireGuard.ConfigFilePath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    private string GenerateConfig(VpnEndpoint endpoint, VpnCredentials credentials, VpnConfig vpnConfig)
    {
        bool isIpv6Supported = vpnConfig.IsIpv6Enabled && endpoint.Server.IsIpv6Supported;
        string address = GetClientAddress(isIpv6Supported);
        string allowedIps = GetAllowedIpAddresses(isIpv6Supported);
        string privateKey = GetX25519SecretKey(credentials.ClientKeyPair.SecretKey).Base64;
        string dns = _wireGuardDnsServersCreator.GetDnsServers(vpnConfig.CustomDns, isIpv6Supported);

        StringBuilder sb = new StringBuilder()
            .AppendLine("[Interface]")
            .AppendLine($"PrivateKey = {privateKey}")
            .AppendLine($"Address = {address}")
            .AppendLine($"DNS = {dns}")
            .AppendLine("[Peer]")
            .AppendLine($"PublicKey = {endpoint.Server.X25519PublicKey.Base64}")
            .AppendLine($"AllowedIPs = {allowedIps}")
            .AppendLine($"Endpoint = {endpoint.Server.Ip}:{endpoint.Port}");

        return sb.ToString();
    }

    private static void WriteConfigSecurely(string path, string content)
    {
        // Delete first so FileMode.CreateNew applies the FileSecurity
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        FileSecurity fileSecurity = new();
        fileSecurity.SetSecurityDescriptorSddlForm(SECURE_SDDL);

        FileInfo fileInfo = new(path);
        using FileStream stream = fileInfo.Create(
            FileMode.CreateNew,
            FileSystemRights.WriteData,
            FileShare.None,
            bufferSize: 4096,
            FileOptions.None,
            fileSecurity);
        using StreamWriter writer = new(stream);
        writer.Write(content);
    }

    private SecretKey GetX25519SecretKey(SecretKey secretKey)
    {
        return _x25519KeyGenerator.FromEd25519SecretKey(secretKey);
    }

    private string GetClientAddress(bool isIpv6Supported)
    {
        string ipv4 = $"{_staticConfig.WireGuard.DefaultClientIpv4Address}/32";

        return isIpv6Supported
            ? $"{ipv4}, {_staticConfig.WireGuard.DefaultClientIpv6Address}/128"
            : ipv4;
    }

    private static string GetAllowedIpAddresses(bool isIpv6Supported)
    {
        return isIpv6Supported
            ? $"{IPV4_ALLOWED_IP}, {IPV6_ALLOWED_IP}"
            : IPV4_ALLOWED_IP;
    }
}