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
using ProtonVPN.Client.Contracts;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Updates.Contracts;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.UI.Sidebar.Bases;
using ProtonVPN.Common.Legacy.OS.Processes;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppUpdateLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.Update.Contracts;

namespace ProtonVPN.Client.UI.Sidebar;

public partial class SidebarUpdateViewModel : SidebarInteractiveItemViewModelBase,
    IEventMessageReceiver<ClientUpdateStateChangedMessage>
{
    private const int APP_EXIT_TIMEOUT_IN_SECONDS = 3;

    private readonly IOsProcesses _osProcesses;
    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly IConfiguration _config;
    private readonly IVpnServiceSettingsUpdater _vpnServiceSettingsUpdater;
    private readonly IOverlayActivator _overlayActivator;
    private readonly IConnectionManager _connectionManager;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEnabled))]
    [NotifyPropertyChangedFor(nameof(Header))]
    [NotifyPropertyChangedFor(nameof(Status))]
    [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
    private AppUpdateStateContract? _updateState;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVisible))]
    private bool _isUpdateAvailable;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEnabled))]
    [NotifyPropertyChangedFor(nameof(Status))]
    [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
    private bool _isUpdating;

    public override IconElement? Icon { get; } = new ImageIcon
    {
        Source = ResourceHelper.GetIllustration("UpdateProtonVpnIllustrationSource")
    };

    public override string Header => Localizer.Get(UpdateState?.Status == AppUpdateStatus.AutoUpdated
        ? "Home_Update_UpdateReady"
        : "Home_Update_UpdateAvailable");

    public override bool IsVisible => IsUpdateAvailable;

    public override bool IsEnabled => !IsUpdating && UpdateState != null && UpdateState.IsReady;

    public override string Status => IsUpdating
        ? Localizer.Get("Common_States_Updating")
        : GetUpdateVersion();

    public override string AutomationId => "Sidebar_Update";

    public SidebarUpdateViewModel(
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        IOsProcesses osProcesses,
        IMainWindowActivator mainWindowActivator,
        ISettings settings,
        IConfiguration config,
        IVpnServiceSettingsUpdater vpnServiceSettingsUpdater,
        IOverlayActivator overlayActivator,
        IConnectionManager connectionManager)
        : base(localizationProvider, logger, issueReporter, settings)
    {
        _osProcesses = osProcesses;
        _mainWindowActivator = mainWindowActivator;
        _config = config;
        _vpnServiceSettingsUpdater = vpnServiceSettingsUpdater;
        _overlayActivator = overlayActivator;
        _connectionManager = connectionManager;
    }

    public override async Task<bool> InvokeAsync()
    {
        if (!await IsAllowedToDisconnectAsync())
        {
            return false;
        }

        await UpdateInternalAsync();
        return true;
    }

    public void Receive(ClientUpdateStateChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            UpdateState = message.State;
            IsUpdateAvailable = message.IsUpdateAvailable;
        });
    }

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private async Task UpdateAsync()
    {
        await InvokeAsync();
    }

    private bool CanUpdate()
    {
        return IsEnabled;
    }

    private async Task<bool> IsAllowedToDisconnectAsync()
    {
        bool isConfirmationNeeded = !_connectionManager.IsDisconnected || Settings.KillSwitchMode == KillSwitchMode.Advanced;
        if (isConfirmationNeeded)
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
        if (UpdateState?.Status == AppUpdateStatus.AutoUpdated)
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
        if (UpdateState == null)
        {
            return;
        }

        IsUpdating = true;

        LogUpdateStartingMessage();

        if (Settings.IsKillSwitchEnabled && Settings.KillSwitchMode == KillSwitchMode.Advanced)
        {
            await _vpnServiceSettingsUpdater.SendAsync(KillSwitchModeIpcEntity.Off);
        }

        try
        {
            _osProcesses.ElevatedProcess(UpdateState.FilePath, UpdateState.FileArguments).Start();
            _mainWindowActivator.Exit();
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // Privileges were not granted
            if (Settings.IsKillSwitchEnabled && Settings.KillSwitchMode == KillSwitchMode.Advanced)
            {
                await _vpnServiceSettingsUpdater.SendAsync(KillSwitchModeIpcEntity.Hard);
            }

            IsUpdating = false;
        }
    }

    private void LogUpdateStartingMessage()
    {
        string fileName = GetUpdateFileName();
        string message = $"Closing the app and starting installer '{fileName}'. " +
                         $"Current app version: {_config.ClientVersion}, OS: {Environment.OSVersion.VersionString}";

        Logger.Info<AppUpdateStartLog>(message);
    }

    private string GetUpdateFileName()
    {
        string fileName;
        string filePath = UpdateState?.FilePath ?? string.Empty;
        try
        {
            fileName = Path.GetFileNameWithoutExtension(filePath);
        }
        catch (Exception e)
        {
            Logger.Error<AppUpdateLog>($"Failed to parse file name of path '{filePath}'.", e);
            fileName = filePath;
        }

        return fileName;
    }

    private string GetUpdateVersion()
    {
        string? version = UpdateState?.Version?.ToString();

        if (string.IsNullOrEmpty(version) || version.StartsWith("0"))
        {
            return string.Empty;
        }

        return version;
    }
}