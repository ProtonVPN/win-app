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
    private const int MAX_SERVICE_STOP_ATTEMPTS = 3;

    private readonly ILogger _logger;
    private readonly IStaticConfiguration _staticConfig;
    private readonly IService _origin;

    public WireGuardService(ILogger logger, IStaticConfiguration staticConfig, IServiceFactory serviceFactory)
    {
        _logger = logger;
        _staticConfig = staticConfig;
        _origin = serviceFactory.Get(staticConfig.WireGuard.ServiceName);
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

    public async Task StopAsync()
    {
        if (!_origin.IsCreated() || _origin.IsStopped())
        {
            return;
        }

        int attemptCount = 0;
        bool result = false;

        while (attemptCount < MAX_SERVICE_STOP_ATTEMPTS)
        {
            result = await _origin.StopAsync(CancellationToken.None);
            await Task.Delay(100);

            if (result)
            {
                break;
            }

            attemptCount++;
        }

        if (!result)
        {
            _logger.Error<AppServiceStartFailedLog>($"Failed to stop WireGuard service after {MAX_SERVICE_STOP_ATTEMPTS} attempts. Trying to kill.");
            _origin.Kill();
        }
    }

    public bool Kill()
    {
        return _origin.Kill();
    }

    private void UpdateServicePath(VpnProtocol protocol)
    {
        string? servicePathToExecutable = _origin.GetBinaryPath();
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