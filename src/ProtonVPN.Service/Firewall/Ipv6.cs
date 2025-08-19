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
using System.Threading.Tasks;
using ProtonVPN.Common.Legacy.OS.Net;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.NetworkLogs;
using ProtonVPN.Service.Settings;

namespace ProtonVPN.Service.Firewall;

internal class Ipv6 : IIpv6
{
    private const string APP_NAME = "ProtonVPN";

    private readonly ILogger _logger;
    private readonly IStaticConfiguration _staticConfig;
    private readonly IServiceSettings _serviceSettings;

    public Ipv6(ILogger logger, IStaticConfiguration staticConfig, IServiceSettings serviceSettings)
    {
        _logger = logger;
        _staticConfig = staticConfig;
        _serviceSettings = serviceSettings;
    }

    public bool IsEnabled { get; private set; } = true;

    public Task DisableAsync()
    {
        return Task.Run(Disable);
    }

    public Task EnableAsync()
    {
        return Task.Run(Enable);
    }

    public Task EnableOnVPNInterfaceAsync()
    {
        return Task.Run(EnableOnVPNInterface);
    }

    public void Enable()
    {
        if (LoggingAction(NetworkUtil.EnableIPv6OnAllAdapters, "Enabling"))
        {
            IsEnabled = true;
        }
    }

    private void Disable()
    {
        if (LoggingAction(NetworkUtil.DisableIPv6OnAllAdapters, "Disabling"))
        {
            IsEnabled = false;
        }
    }

    private void EnableOnVPNInterface()
    {
        LoggingAction(NetworkUtil.EnableIPv6, "Enabling on VPN interface");
    }

    private bool LoggingAction(Action<string, string> action, string actionMessage)
    {
        try
        {
            _logger.Info<NetworkLog>($"IPv6: {actionMessage}");
            action(APP_NAME, _staticConfig.GetHardwareId(_serviceSettings.OpenVpnAdapter));
            _logger.Info<NetworkLog>($"IPv6: {actionMessage} succeeded");

            return true;
        }
        catch (NetworkUtilException e)
        {
            _logger.Error<NetworkLog>($"IPV6: {actionMessage} failed, error code {e.Code}");

            return false;
        }
    }
}