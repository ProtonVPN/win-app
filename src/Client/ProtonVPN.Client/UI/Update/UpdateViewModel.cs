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
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Updates.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Update.Contracts;

namespace ProtonVPN.Client.UI.Update;

public partial class UpdateViewModel : ViewModelBase,
    IEventMessageReceiver<ClientUpdateStateChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IUpdatesManager _updatesManager;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IConnectionManager _connectionManager;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
    [NotifyPropertyChangedFor(nameof(CanUpdate))]
    private AppUpdateStateContract? _lastUpdateState;

    [ObservableProperty]
    private bool _isUpdateAvailable;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
    [NotifyPropertyChangedFor(nameof(CanUpdate))]
    private bool _isUpdating;

    public bool CanUpdate => !IsUpdating && LastUpdateState != null && LastUpdateState.IsReady;

    public UpdateViewModel(
        ISettings settings,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        IUpdatesManager updatesManager,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IConnectionManager connectionManager)
        : base(localizationProvider, logger, issueReporter)
    {
        _settings = settings;
        _updatesManager = updatesManager;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _connectionManager = connectionManager;
    }

    public void Receive(ClientUpdateStateChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            LastUpdateState = message.State;
        });
    }

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private async Task UpdateAsync()
    {
        if (!await IsAllowedToDisconnectAsync())
        {
            return;
        }

        IsUpdating = true;
        await _updatesManager.UpdateAsync();
        IsUpdating = false;
    }

    private async Task<bool> IsAllowedToDisconnectAsync()
    {
        bool isConfirmationNeeded = !_connectionManager.IsDisconnected ||
                                    (_settings.KillSwitchMode == KillSwitchMode.Advanced &&
                                     _settings.IsKillSwitchEnabled);
        if (isConfirmationNeeded)
        {
            MessageDialogParameters parameters = new()
            {
                Title = Localizer.GetFormat("Home_Update_Confirmation_Title"),
                Message = Localizer.Get("Home_Update_Confirmation_Message"),
                PrimaryButtonText = Localizer.Get("Common_Actions_Update"),
                CloseButtonText = Localizer.Get("Common_Actions_Cancel"),
            };

            ContentDialogResult result = await _mainWindowOverlayActivator.ShowMessageAsync(parameters);
            if (result is not ContentDialogResult.Primary)
            {
                return false;
            }
        }

        return true;
    }
}