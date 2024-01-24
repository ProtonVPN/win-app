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

using Microsoft.UI.Xaml;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Logic.Updates.Contracts;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Migrations;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Bootstrapping;

public class Bootstrapper : IBootstrapper
{
    private readonly IProcessCommunicationStarter _processCommunicationStarter;
    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly ISettingsRestorer _settingsRestorer;
    private readonly IServiceManager _serviceManager;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IUpdatesManager _updatesManager;
    private readonly ISettingsMigrator _settingsMigrator;
    private readonly ISettings _settings;
    private readonly ILogger _logger;

    public Bootstrapper(
        IProcessCommunicationStarter processCommunicationStarter,
        IMainWindowActivator mainWindowActivator,
        ISettingsRestorer settingsRestorer,
        IServiceManager serviceManager,
        IUserAuthenticator userAuthenticator,
        IUpdatesManager updatesManager,
        ISettingsMigrator settingsMigrator,
        ISettings settings,
        ILogger logger)
    {
        _processCommunicationStarter = processCommunicationStarter;
        _mainWindowActivator = mainWindowActivator;
        _settingsRestorer = settingsRestorer;
        _serviceManager = serviceManager;
        _userAuthenticator = userAuthenticator;
        _updatesManager = updatesManager;
        _settingsMigrator = settingsMigrator;
        _settings = settings;
        _logger = logger;
    }

    public async Task StartAsync(LaunchActivatedEventArgs args)
    {
        try
        {
            ParseAndRunCommandLineArguments();

            _settingsMigrator.Migrate();

            _mainWindowActivator.Show();
            _updatesManager.Initialize();

            await Task.WhenAll(
                TryAuthenticateAsync(),
                StartServiceAsync());
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("Error occured during the app start up process.", e);
        }
    }

    private async Task TryAuthenticateAsync()
    {
        if (string.IsNullOrEmpty(_settings.Username))
        {
            await _userAuthenticator.CreateUnauthSessionAsync();
        }
        else
        {
            await _userAuthenticator.AutoLoginUserAsync();
        }
    }

    private async Task StartServiceAsync()
    {
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        await _serviceManager.StartAsync();
        await _processCommunicationStarter.StartAsync(cancellationToken);
    }

    private void ParseAndRunCommandLineArguments()
    {
        string[] args = Environment.GetCommandLineArgs();
        foreach (string arg in args)
        {
            if (arg.EqualsIgnoringCase("-RestoreDefaultSettings"))
            {
                _settingsRestorer.Restore();
            }
            else if (arg.EqualsIgnoringCase("/DisableAutoUpdate"))
            {
                _settings.AreAutomaticUpdatesEnabled = false;
            }
        }
    }
}