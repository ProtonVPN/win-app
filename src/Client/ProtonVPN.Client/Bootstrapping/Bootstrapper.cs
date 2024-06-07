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
using ProtonVPN.Client.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Logic.Updates.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
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
    private readonly IGlobalSettingsMigrator _globalSettingsMigrator;
    private readonly IGlobalSettings _globalSettings;
    private readonly ILogger _logger;

    public Bootstrapper(
        IProcessCommunicationStarter processCommunicationStarter,
        IMainWindowActivator mainWindowActivator,
        ISettingsRestorer settingsRestorer,
        IServiceManager serviceManager,
        IUserAuthenticator userAuthenticator,
        IUpdatesManager updatesManager,
        IGlobalSettingsMigrator settingsMigrator,
        IGlobalSettings globalSettings,
        ILogger logger)
    {
        _processCommunicationStarter = processCommunicationStarter;
        _mainWindowActivator = mainWindowActivator;
        _settingsRestorer = settingsRestorer;
        _serviceManager = serviceManager;
        _userAuthenticator = userAuthenticator;
        _updatesManager = updatesManager;
        _globalSettingsMigrator = settingsMigrator;
        _globalSettings = globalSettings;
        _logger = logger;
    }

    public async Task StartAsync(LaunchActivatedEventArgs args)
    {
        try
        {
            ParseAndRunCommandLineArguments();

            _globalSettingsMigrator.Migrate();

            await HandleMainWindowAsync();
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

    private async Task HandleMainWindowAsync()
    {
        await _mainWindowActivator.InitializeAsync();

        if (!_userAuthenticator.HasAuthenticatedSessionData() || 
            !_globalSettings.IsAutoLaunchEnabled || 
            _globalSettings.AutoLaunchMode == AutoLaunchMode.OpenOnDesktop)
        {
            _mainWindowActivator.Activate();
        }
    }

    private async Task TryAuthenticateAsync()
    {
        if (_userAuthenticator.HasAuthenticatedSessionData())
        {
            await _userAuthenticator.AutoLoginUserAsync();
        }
        else
        {
            await _userAuthenticator.CreateUnauthSessionAsync();
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
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (arg.EqualsIgnoringCase("-Language"))
            {
                int languageArgumentIndex = i + 1;
                if (languageArgumentIndex < args.Length)
                {
                    _globalSettings.Language = args[languageArgumentIndex];
                    i++;
                    continue;
                }
            }
            else if (arg.EqualsIgnoringCase("-RestoreDefaultSettings"))
            {
                _settingsRestorer.Restore();
            }
            else if (arg.EqualsIgnoringCase("-DisableAutoUpdate"))
            {
                _globalSettings.AreAutomaticUpdatesEnabled = false;
            }
            else if (arg.EqualsIgnoringCase("-ExitAppOnClose"))
            {
                _mainWindowActivator.DisableHandleClosedEvents();
            }
        }
    }
}