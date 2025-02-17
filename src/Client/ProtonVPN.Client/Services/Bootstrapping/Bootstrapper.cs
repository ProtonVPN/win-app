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
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Logic.Updates.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Extensions;
using ProtonVPN.Client.Settings.Contracts.Migrations;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.IssueReporting.Installers;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.StatisticalEvents.Contracts;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;

namespace ProtonVPN.Client.Services.Bootstrapping;

public class Bootstrapper : IBootstrapper
{
    private const int EXIT_DISCONNECTION_TIMEOUT_IN_MS = 5000;
    private const int EXIT_DISCONNECTION_DELAY_IN_MS = 200;

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
    private readonly ILocalizationProvider _localizer;
    private readonly IConnectionManager _connectionManager;
    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;

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
        IMainWindowOverlayActivator mainWindowOverlayActivator)
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
        _localizer = localizer;
        _connectionManager = connectionManager;
        _mainWindowActivator = mainWindowActivator;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
    }

    public async Task StartAsync(LaunchActivatedEventArgs args)
    {
        try
        {
            IssueReportingInitializer.SetEnabled(_settings.IsShareCrashReportsEnabled);

            HandleCommandLineArguments();

            _globalSettingsMigrator.Migrate();

            HandleMainWindow();

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

    public async Task ExitAsync()
    {
        _logger.Info<AppLog>("Exiting application requested.");

        string? confirmationMessage = _localizer.GetExitOrSignOutConfirmationMessage(_connectionManager.IsDisconnected, _settings);
        if (!string.IsNullOrEmpty(confirmationMessage))
        {
            _mainWindowActivator.Activate();

            InlineTextButton advancedKillSwitchLearnMoreButton = new()
            {
                Text = _localizer.Get("Common_Links_LearnMore"),
                Url = _urlsBrowser.AdvancedKillSwitchLearnMore,
            };

            ContentDialogResult result = await _mainWindowOverlayActivator.ShowMessageAsync(
                new MessageDialogParameters
                {
                    Title = _localizer.Get("Exit_Confirmation_Title"),
                    Message = confirmationMessage,
                    MessageType = DialogMessageType.RichText,
                    PrimaryButtonText = _localizer.Get("Tray_Actions_ExitApplication"),
                    CloseButtonText = _localizer.Get("Common_Actions_Cancel"),
                    TrailingInlineButton = _settings.IsAdvancedKillSwitchActive()
                        ? advancedKillSwitchLearnMoreButton
                        : null
                });

            if (result is not ContentDialogResult.Primary) // Cancel exit
            {
                return;
            }
        }

        if (!_connectionManager.IsDisconnected)
        {
            await _mainWindowOverlayActivator.ShowLoadingMessageAsync(
                new MessageDialogParameters
                {
                    Title = _localizer.Get("Exit_Title"),
                    Message = _localizer.Get("Exit_Message"),
                    ShowLoadingAnimation = true
                }, Task.WhenAny(_connectionManager.DisconnectAsync(VpnTriggerDimension.Exit), Task.Delay(EXIT_DISCONNECTION_TIMEOUT_IN_MS)));

            // Keep a slight delay before exit so the app can process the Disconnected event message
            await Task.Delay(EXIT_DISCONNECTION_DELAY_IN_MS);
        }

        _mainWindowActivator.Exit();
        _serviceManager.Stop();
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
        if (!_userAuthenticator.HasAuthenticatedSessionData() ||
            !_settings.IsAutoLaunchEnabled ||
            _settings.AutoLaunchMode == AutoLaunchMode.OpenOnDesktop)
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
        await _serviceManager.StartAsync();
        _processCommunicationStarter.Start();
    }
}