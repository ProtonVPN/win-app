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

using System;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Abstract;
using ProtonVPN.Common.Legacy.OS.Services;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;

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

    public bool Exists() => _origin.Exists();

    public void Create(string pathAndArgs, bool unrestricted)
    {
        _origin.Create(pathAndArgs, unrestricted);
    }

    public bool Running() => _origin.Running();

    public bool IsStopped() => _origin.IsStopped();

    public bool Enabled() => _origin.Enabled();

    public void Enable() => _origin.Enabled();

    public Task<Result> StartAsync(CancellationToken cancellationToken, VpnProtocol protocol)
    {
        if (!_origin.Exists())
        {
            _logger.Info<AppServiceLog>("WireGuard Service is missing. Installing.");
            _origin.Create(GetServiceCommandLine(protocol), unrestricted: true);
        }

        if (!_origin.Enabled())
        {
            _origin.Enable();
        }

        UpdateServicePath(protocol);

        return _origin.StartAsync(cancellationToken);
    }

    private void UpdateServicePath(VpnProtocol protocol)
    {
        string servicePathToExecutable = GetServicePathToExecutable();
        if (string.IsNullOrEmpty(servicePathToExecutable))
        {
            _logger.Error<AppServiceLog>(ServicePathError);
            return;
        }

        if (servicePathToExecutable != GetServiceCommandLine(protocol))
        {
            _logger.Info<AppServiceLog>($"{Name} path {servicePathToExecutable} points to an old version. " +
                                        $"Updating to the current one.");
            _origin.UpdatePathAndArgs(GetServiceCommandLine(protocol));
        }
    }

    public async Task<Result> StopAsync(CancellationToken cancellationToken)
    {
        return await _origin.StopAsync(cancellationToken);
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

    private string GetServicePathToExecutable()
    {
        try
        {
            ManagementObject wmiService = new($"Win32_Service.Name='{Name}'");
            wmiService.Get();

            return (string)wmiService["PathName"];
        }
        catch (Exception e)
        {
            _logger.Error<AppServiceLog>(ServicePathError, e);
            return null;
        }
    }

    private string ServicePathError => $"Failed to receive {Name} path.";
}