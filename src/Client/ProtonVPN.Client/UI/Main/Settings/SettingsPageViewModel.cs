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

using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;

namespace ProtonVPN.Client.UI.Main.Settings;

public partial class SettingsPageViewModel : PageViewModelBase<IMainViewNavigator, ISettingsViewNavigator>,
    IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    private readonly IConnectionManager _connectionManager;

    public SettingsPageViewModel(
        IMainViewNavigator parentViewNavigator,
        ISettingsViewNavigator childViewNavigator,
        IConnectionManager connectionManager,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator, childViewNavigator, viewModelHelper)
    {
        _connectionManager = connectionManager;
    }

    public Task<bool> CloseAsync()
    {
        return ChildViewNavigator.GetCurrentPageContext() is SettingsPageViewModelBase settingsPage
            ? settingsPage.CloseAsync()
            : Task.FromResult(true);
    }

    public override void OnNavigatedFrom()
    {
        base.OnNavigatedFrom();

        ForceNavigationToCommonSettingsAsync();
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (IsActive && _connectionManager.IsConnected && _connectionManager.CurrentConnectionIntent is IConnectionProfile)
        {
            // When connected to a profile, force navigation to common settings page. Some settings might be overridden by the profile.
            ExecuteOnUIThread(async () => await ForceNavigationToCommonSettingsAsync());
        }
    }

    private async Task ForceNavigationToCommonSettingsAsync()
    {
        if (ChildViewNavigator.GetCurrentPageContext() is SettingsPageViewModelBase settingsPage)
        {
            await ChildViewNavigator.NavigateToCommonSettingsViewAsync(true);
            settingsPage.RequestResetContentScroll();
        }
    }
}