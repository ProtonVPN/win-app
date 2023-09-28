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

using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.Contracts.ViewModels;

public abstract partial class ConnectionSettingsPageViewModelBase : SettingsPageViewModelBase, IEventMessageReceiver<ConnectionStatusChanged>
{
    protected readonly IConnectionManager ConnectionManager;

    public ConnectionSettingsPageViewModelBase(
        IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager)
        : base(viewNavigator, localizationProvider, settings, settingsConflictResolver)
    {
        ConnectionManager = connectionManager;
    }

    [RelayCommand(CanExecute = nameof(CanReconnect))]
    public async Task ReconnectAsync()
    {
        SaveSettings();

        await ConnectionManager.ReconnectAsync();
    }

    public bool CanReconnect()
    {
        return ConnectionManager != null
            && !ConnectionManager.IsDisconnected
            && HasConfigurationChanged();
    }

    public void Receive(ConnectionStatusChanged message)
    {
        ReconnectCommand.NotifyCanExecuteChanged();
    }

    public override async Task<bool> OnNavigatingFromAsync()
    {
        if (!CanReconnect())
        {
            // No reconnection required, simply save settings when leaving page
            return await base.OnNavigatingFromAsync();
        }

        ContentDialogResult result = await ViewNavigator.ShowMessageAsync(
            new MessageDialogParameters
            {
                Title = Localizer.Get("Settings_Common_Discard_Title"),
                PrimaryButtonText = Localizer.Get("Settings_Common_Discard"),
                SecondaryButtonText = Localizer.Get("Settings_Common_ApplyAndReconnect"),
                CloseButtonText = Localizer.Get("Common_Actions_Cancel"),
                UseVerticalLayoutForButtons = true,
            });

        switch (result)
        {
            case ContentDialogResult.Primary:
                // Do nothing, user decided to discard settings changes.
                return true;

            case ContentDialogResult.Secondary:
                // Apply settings and trigger reconnections
                await ReconnectAsync();
                return true;

            default:
                // Cancel navigation, stays on current page without deleting changes user have made
                return false;
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        ReconnectCommand.NotifyCanExecuteChanged();
    }
}