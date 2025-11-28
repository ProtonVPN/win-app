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

using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;
using ProtonVPN.OperatingSystems.Services.Contracts;

namespace ProtonVPN.Vpn.WireGuard;

public class WireGuardService : IWireGuardService
{
    private readonly ILogger _logger;
    private readonly IStaticConfiguration _staticConfig;
    private readonly IService _origin;

    public WireGuardService(ILogger logger, IStaticConfiguration staticConfig, IService origin)
    {
        _logger = logger;
        _staticConfig = staticConfig;
        _origin = origin;
    }

    public string Name => _origin.Name;

    public bool Exists() => _origin.IsCreated();

    public bool Running() => _origin.IsRunning();

    public bool IsStopped() => _origin.IsStopped();

    public async Task StartAsync(CancellationToken cancellationToken, VpnProtocol protocol)
    {
        if (!_origin.IsCreated())
        {
            _logger.Info<AppServiceLog>("WireGuard Service is missing. Creating.");

            _origin.Create(new ServiceCreationOptions(
                pathAndArguments: GetServiceCommandLine(protocol),
                isUnrestricted: true,
                dependencies: ["Nsi", "TcpIp"]));
        }

        if (!_origin.IsEnabled())
        {
            _origin.Enable();
        }

        UpdateServicePath(protocol);

        await _origin.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _origin.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    private void UpdateServicePath(VpnProtocol protocol)
    {
        string servicePathToExecutable = _origin.GetBinaryPath();
        if (string.IsNullOrEmpty(servicePathToExecutable))
        {
            _logger.Error<AppServiceLog>(ServicePathError);
            return;
        }

        string expectedServicePath = GetServiceCommandLine(protocol);

        if (servicePathToExecutable != expectedServicePath)
        {
            _logger.Info<AppServiceLog>($"Updating {Name} service path from {servicePathToExecutable} to {expectedServicePath}.");
            _origin.UpdatePathAndArgs(expectedServicePath);
        }
    }

    private string GetServiceCommandLine(VpnProtocol protocol)
    {
        string wireguardProtocol = protocol switch
        {
            VpnProtocol.WireGuardUdp => "udp",
            VpnProtocol.WireGuardTcp => "tcp",
            VpnProtocol.WireGuardTls => "tls",
            _ => "udp"
        };
        return $"\"{_staticConfig.WireGuard.ServicePath}\" \"{_staticConfig.WireGuard.ConfigFilePath}\" \"{wireguardProtocol}\"";
    }

    private string ServicePathError => $"Failed to receive {Name} path.";
}