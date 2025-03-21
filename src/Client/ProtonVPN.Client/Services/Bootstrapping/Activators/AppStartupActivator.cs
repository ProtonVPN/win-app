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

public class AppStartupActivator : IAppStartupActivator
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public AppStartupActivator(
        IConfiguration configuration,
        ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;

        CleanupLegacyEntries();
    }

    public void Register()
    {
        TryRegisterStartup(App.APPLICATION_NAME, _configuration.ClientLauncherExePath);
    }

    public void Unregister()
    {
        TryUnregisterStartup(App.APPLICATION_NAME);
    }

    private void CleanupLegacyEntries()
    {
        string[] legacyEntries = ["ProtonVPN", "ProtonVPN.Client"];

        foreach (string legacyEntry in legacyEntries)
        {
            TryUnregisterStartup(legacyEntry);
        }
    }

    private void TryRegisterStartup(string applicationName, string clientExePath)
    {
        try
        {
            _logger.Debug<AppLog>($"Register '{applicationName}' startup activation");

            ActivationRegistrationManager.RegisterForStartupActivation(applicationName, clientExePath);
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>($"Error while registering '{applicationName}' startup activation", e);
        }
    }

    private void TryUnregisterStartup(string applicationName)
    {
        try
        {
            _logger.Debug<AppLog>($"Unregister '{applicationName}' startup activation");

            ActivationRegistrationManager.UnregisterForStartupActivation(applicationName);
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>($"Error while unregistering '{applicationName}' startup activation", e);
        }
    }
}