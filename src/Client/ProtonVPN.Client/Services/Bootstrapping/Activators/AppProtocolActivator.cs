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

using Microsoft.Windows.AppLifecycle;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Services.Bootstrapping.Activators;

public class AppProtocolActivator : IAppProtocolActivator
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly string[] _supportedSchemes;

    public AppProtocolActivator(
        IConfiguration configuration,
        ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;

        _supportedSchemes =
        [
            _configuration.ProtocolActivationScheme,
            _configuration.LegacyProtocolActivationScheme
        ];

        Register();
    }

    public void Register()
    {
        string clientExePath = _configuration.ClientLauncherExePath;

        foreach (string scheme in _supportedSchemes)
        {
            TryRegisterScheme(scheme, clientExePath);
        }
    }

    public void Unregister()
    {
        string clientExePath = _configuration.ClientLauncherExePath;

        foreach (string scheme in _supportedSchemes)
        {
            TryUnregisterScheme(scheme, clientExePath);
        }
    }

    private void TryRegisterScheme(string scheme, string clientExePath)
    {
        try
        {
            _logger.Debug<AppLog>($"Register '{scheme}://' protocol activation with path {clientExePath}");

            ActivationRegistrationManager.RegisterForProtocolActivation(scheme, clientExePath + ",0", App.APPLICATION_NAME, clientExePath);
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>($"Error while registering '{scheme}://' protocol activation", e);
        }
    }

    private void TryUnregisterScheme(string scheme, string clientExePath)
    {
        try
        {
            _logger.Debug<AppLog>($"Unregister '{scheme}://' protocol activation with path {clientExePath}");

            ActivationRegistrationManager.UnregisterForProtocolActivation(scheme, clientExePath);
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>($"Error while unregistering '{scheme}://' protocol activation", e);
        }
    }
}