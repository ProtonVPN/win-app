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

using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Contracts.Services.Lifecycle;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Extensions;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.OperatingSystems.Processes.Contracts;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;

namespace ProtonVPN.Client.Services.Lifecycle;

public class AppExitInvoker : IAppExitInvoker
{
    private const int EXIT_DISCONNECTION_TIMEOUT_IN_MS = 5000;
    private const int EXIT_DISCONNECTION_DELAY_IN_MS = 200;

    private readonly ILogger _logger;
    private readonly IConfiguration _config;
    private readonly ICommandLineCaller _commandLineCaller;
    private readonly ILocalizationProvider _localizer;
    private readonly IConnectionManager _connectionManager;
    private readonly ISettings _settings;
    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;

    public AppExitInvoker(ILogger logger,
        IConfiguration config,
        ICommandLineCaller commandLineCaller,
        ILocalizationProvider localizer,
        IConnectionManager connectionManager,
        ISettings settings,
        IMainWindowActivator mainWindowActivator,
        IUrlsBrowser urlsBrowser,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IUIThreadDispatcher uiThreadDispatcher)
    {
        _logger = logger;
        _config = config;
        _commandLineCaller = commandLineCaller;
        _localizer = localizer;
        _connectionManager = connectionManager;
        _settings = settings;
        _mainWindowActivator = mainWindowActivator;
        _urlsBrowser = urlsBrowser;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _uiThreadDispatcher = uiThreadDispatcher;
    }

    public async Task RestartAsync()
    {
        await ForceExitAsync();
        Program.ReleaseMutex();
        StartNewAppProcess();
    }

    private void StartNewAppProcess()
    {
        string cmd = $"/c start \"\" \"{_config.ClientLauncherExePath}\"";
        _commandLineCaller.Execute(cmd);
    }

    public async Task ExitWithConfirmationAsync()
    {
        await _uiThreadDispatcher.TryEnqueueAsync(() => InternalExitAsync(isToAskForExitConfirmation: true));
    }

    public async Task ForceExitAsync()
    {
        await _uiThreadDispatcher.TryEnqueueAsync(() => InternalExitAsync(isToAskForExitConfirmation: false));
    }

    private async Task InternalExitAsync(bool isToAskForExitConfirmation)
    {
        _logger.Info<AppStopLog>($"Exiting application requested (Ask for exit confirmation: {isToAskForExitConfirmation})");

        if (isToAskForExitConfirmation)
        {
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
        }

        if (!_connectionManager.IsDisconnected)
        {
            _logger.Info<AppStopLog>("Awaiting disconnection before continuing the exit procedure");
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
    }
}