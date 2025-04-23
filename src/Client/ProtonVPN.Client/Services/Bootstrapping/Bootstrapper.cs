﻿/*
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

using Microsoft.Windows.AppLifecycle;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Logic.Updates.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Migrations;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.IssueReporting.Installers;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.StatisticalEvents.Contracts;
using Windows.ApplicationModel.Activation;
using LaunchActivatedEventArgs = Microsoft.UI.Xaml.LaunchActivatedEventArgs;

namespace ProtonVPN.Client.Services.Bootstrapping;

public class Bootstrapper : IBootstrapper
{
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IClientInstallsStatisticalEventSender _clientInstallsStatisticalEventSender;
    private readonly IProcessCommunicationStarter _processCommunicationStarter;
    private readonly ISettingsRestorer _settingsRestorer;
    private readonly IServiceManager _serviceManager;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IUpdatesManager _updatesManager;
    private readonly IGlobalSettingsMigrator _globalSettingsMigrator;
    private readonly ISettings _settings;
    private readonly ILogger _logger;
    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly IVpnPlanUpdater _vpnPlanUpdater;

    public Bootstrapper(
        IUrlsBrowser urlsBrowser,
        IClientInstallsStatisticalEventSender clientInstallsStatisticalEventSender,
        IProcessCommunicationStarter processCommunicationStarter,
        ISettingsRestorer settingsRestorer,
        IServiceManager serviceManager,
        IUserAuthenticator userAuthenticator,
        IUpdatesManager updatesManager,
        IGlobalSettingsMigrator settingsMigrator,
        ISettings settings,
        ILogger logger,
        ILocalizationProvider localizer,
        IConnectionManager connectionManager,
        IMainWindowActivator mainWindowActivator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IVpnPlanUpdater vpnPlanUpdater)
    {
        _urlsBrowser = urlsBrowser;
        _clientInstallsStatisticalEventSender = clientInstallsStatisticalEventSender;
        _processCommunicationStarter = processCommunicationStarter;
        _settingsRestorer = settingsRestorer;
        _serviceManager = serviceManager;
        _userAuthenticator = userAuthenticator;
        _updatesManager = updatesManager;
        _globalSettingsMigrator = settingsMigrator;
        _settings = settings;
        _logger = logger;
        _mainWindowActivator = mainWindowActivator;
        _vpnPlanUpdater = vpnPlanUpdater;
    }

    public async Task StartAsync(LaunchActivatedEventArgs args)
    {
        try
        {
            IssueReportingInitializer.SetEnabled(_settings.IsShareCrashReportsEnabled);

            AppInstance.GetCurrent().Activated += OnCurrentAppInstanceActivated;

            HandleCommandLineArguments();

            _globalSettingsMigrator.Migrate();

            HandleMainWindow();

            StartServiceAsync().FireAndForget();

            await TryAuthenticateAsync();
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("Error occured during the app start up process.", e);
        }
    }

    private void OnCurrentAppInstanceActivated(object? sender, AppActivationArguments e)
    {
        switch (e.Kind)
        {
            case ExtendedActivationKind.Protocol:
                HandleProtocolActivationArguments(e.Data as ProtocolActivatedEventArgs);
                break;
            default:
                _logger.Info<AppLog>($"Handle {e.Kind} activation - Activate window");
                _mainWindowActivator.Activate();
                break;
        }
    }

    private void HandleProtocolActivationArguments(ProtocolActivatedEventArgs? args)
    {
        _logger.Info<AppLog>("Handle protocol activation - Activate window and refresh vpn plan");

        // TODO: Investigate why protocol activation arguments are always null
        _mainWindowActivator.Activate();
        _vpnPlanUpdater.UpdateAsync();
    }

    private void HandleCommandLineArguments()
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
                    _settings.Language = args[languageArgumentIndex];
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
                _settings.AreAutomaticUpdatesEnabled = false;
            }
            else if (arg.EqualsIgnoringCase("-ExitAppOnClose"))
            {
                _mainWindowActivator.DisableHandleClosedEvent();
            }
        }

        HandleProtonInstallerArguments(args);
    }

    private void HandleProtonInstallerArguments(string[] args)
    {
        bool isCleanInstall = false;
        bool isMailInstalled = false;
        bool isDriveInstalled = false;
        bool isPassInstalled = false;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (arg.EqualsIgnoringCase("-CleanInstall"))
            {
                isCleanInstall = true;
            }
            else if (arg.EqualsIgnoringCase("-MailInstalled"))
            {
                isMailInstalled = true;
            }
            else if (arg.EqualsIgnoringCase("-DriveInstalled"))
            {
                isDriveInstalled = true;
            }
            else if (arg.EqualsIgnoringCase("-PassInstalled"))
            {
                isPassInstalled = true;
            }
        }

        if (isCleanInstall)
        {
            _clientInstallsStatisticalEventSender.Send(
                isMailInstalled: isMailInstalled,
                isDriveInstalled: isDriveInstalled,
                isPassInstalled: isPassInstalled);
        }
    }

    private void HandleMainWindow()
    {
        bool hasAuthenticatedSessionData = _userAuthenticator.HasAuthenticatedSessionData();
        bool isAutoLaunchEnabled = _settings.IsAutoLaunchEnabled;
        bool isAutoLaunchModeOpenOnDesktop = _settings.AutoLaunchMode == AutoLaunchMode.OpenOnDesktop;

        _logger.Info<AppLog>($"Handle main window start condtions - HasAuthenticatedSessionData: {hasAuthenticatedSessionData}, IsAutoLaunchEnabled: {isAutoLaunchEnabled}, IsAutoLaunchModeOpenOnDesktop: {isAutoLaunchModeOpenOnDesktop}");

        if (!hasAuthenticatedSessionData || !isAutoLaunchEnabled || isAutoLaunchModeOpenOnDesktop)
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
        try
        {
            await _serviceManager.StartAsync();
        }
        catch
        {
        }

        try
        {
            _processCommunicationStarter.Start();
        }
        catch
        {
        }

        _updatesManager.Initialize();
    }
}