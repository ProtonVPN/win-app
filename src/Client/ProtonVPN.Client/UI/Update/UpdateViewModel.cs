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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Updates.Contracts;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Common.Legacy.OS.Processes;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppUpdateLogs;
using ProtonVPN.Update.Contracts;

namespace ProtonVPN.Client.UI.Update;

public partial class UpdateViewModel : ViewModelBase,
    IEventMessageReceiver<ClientUpdateStateChangedMessage>,
    IEventMessageReceiver<ConnectionStatusChanged>,
    IEventMessageReceiver<SettingChangedMessage>
{
    private const int APP_EXIT_TIMEOUT_IN_SECONDS = 3;

    private readonly ILogger _logger;
    private readonly IOsProcesses _osProcesses;
    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly ISettings _settings;
    private readonly IConfiguration _config;
    private readonly IVpnServiceSettingsUpdater _vpnServiceSettingsUpdater;
    private readonly IOverlayActivator _overlayActivator;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
    private bool _isUpdating;

    [ObservableProperty]
    private bool _isToShowUpdateComponent;

    [ObservableProperty]
    private bool _isToShowUpdateButtonLabel = true;

    [ObservableProperty]
    private string _updateButtonLabel;

    private ConnectionStatus _connectionStatus;
    private AppUpdateStateContract? _clientUpdateState;

    private bool IsToShowConfirmationDialog => _connectionStatus != ConnectionStatus.Disconnected ||
                                               _settings.KillSwitchMode == KillSwitchMode.Advanced;

    public IconElement Icon => new ImageIcon
    {
        Source = ResourceHelper.GetIllustration("UpdateProtonVpnIllustrationSource")
    };

    public UpdateViewModel(
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IOsProcesses osProcesses,
        IMainWindowActivator mainWindowActivator,
        ISettings settings,
        IConfiguration config,
        IVpnServiceSettingsUpdater vpnServiceSettingsUpdater,
        IOverlayActivator overlayActivator) : base(localizationProvider)
    {
        _logger = logger;
        _osProcesses = osProcesses;
        _mainWindowActivator = mainWindowActivator;
        _settings = settings;
        _config = config;
        _vpnServiceSettingsUpdater = vpnServiceSettingsUpdater;
        _overlayActivator = overlayActivator;
    }

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    public async Task UpdateAsync()
    {
        if (!await IsAllowedToDisconnectAsync())
        {
            return;
        }

        await UpdateInternalAsync();
    }

    private bool CanUpdate()
    {
        return !IsUpdating && _clientUpdateState != null && _clientUpdateState.IsReady;
    }

    private async Task<bool> IsAllowedToDisconnectAsync()
    {
        if (IsToShowConfirmationDialog)
        {
            MessageDialogParameters parameters = new()
            {
                Title = Localizer.GetFormat("Home_Update_Confirmation_Title"),
                Message = Localizer.Get("Home_Update_Confirmation_Message"),
                PrimaryButtonText = Localizer.Get("Common_Actions_Update"),
                CloseButtonText = Localizer.Get("Common_Actions_Cancel"),
            };
            
            ContentDialogResult result = await _overlayActivator.ShowMessageAsync(parameters);
            if (result is not ContentDialogResult.Primary)
            {
                return false;
            }
        }

        return true;
    }

    private async Task UpdateInternalAsync()
    {
        if (_clientUpdateState?.Status == AppUpdateStatus.AutoUpdated)
        {
            RestartApp();
        }
        else
        {
            await UpdateManuallyAsync();
        }
    }

    private void RestartApp()
    {
        string cmd = $"/c Timeout /t {APP_EXIT_TIMEOUT_IN_SECONDS} >nul & \"{_config.ClientLauncherExePath}\"";
        using IOsProcess process = _osProcesses.CommandLineProcess(cmd);
        process.Start();
        _mainWindowActivator.Exit();
    }

    private async Task UpdateManuallyAsync()
    {
        if (_clientUpdateState == null)
        {
            return;
        }

        IsUpdating = true;

        LogUpdateStartingMessage();

        if (_settings.KillSwitchMode == KillSwitchMode.Advanced)
        {
            await _vpnServiceSettingsUpdater.DisableKillSwitchAsync();
        }

        try
        {
            _osProcesses.ElevatedProcess(_clientUpdateState.FilePath, _clientUpdateState.FileArguments).Start();
            _mainWindowActivator.Exit();
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // Privileges were not granted
            if (_settings.KillSwitchMode == KillSwitchMode.Advanced)
            {
                await _vpnServiceSettingsUpdater.EnableAdvancedKillSwitchAsync();
            }

            IsUpdating = false;
        }
    }

    private void LogUpdateStartingMessage()
    {
        string fileName = GetUpdateFileName();
        string message = $"Closing the app and starting installer '{fileName}'. " +
                         $"Current app version: {_config.ClientVersion}, OS: {Environment.OSVersion.VersionString}";

        _logger.Info<AppUpdateStartLog>(message);
    }

    private string GetUpdateFileName()
    {
        string fileName;
        string filePath = _clientUpdateState.FilePath;
        try
        {
            fileName = Path.GetFileNameWithoutExtension(filePath);
        }
        catch (Exception e)
        {
            _logger.Error<AppUpdateLog>($"Failed to parse file name of path '{filePath}'.", e);
            fileName = filePath;
        }

        return fileName;
    }

    public void Receive(ClientUpdateStateChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            _clientUpdateState = message.State;
            IsToShowUpdateComponent = message.IsUpdateAvailable;
            if (message.IsUpdateAvailable)
            {
                UpdateButtonLabel = Localizer.Get(message.State?.Status == AppUpdateStatus.AutoUpdated
                    ? "Home_Update_UpdateReady"
                    : "Home_Update_UpdateAvailable");
            }

            UpdateCommand.NotifyCanExecuteChanged();
        });
    }

    public void Receive(ConnectionStatusChanged message)
    {
        ExecuteOnUIThread(() =>
        {
            _connectionStatus = message.ConnectionStatus;
        });
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.IsNavigationPaneOpened) && message.NewValue is not null)
        {
            IsToShowUpdateButtonLabel = (bool)message.NewValue;
        }
    }
}