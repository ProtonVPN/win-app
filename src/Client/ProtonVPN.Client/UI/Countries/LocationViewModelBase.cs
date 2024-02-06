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

using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Home;

namespace ProtonVPN.Client.UI.Countries;

public abstract partial class LocationViewModelBase : ViewModelBase
{
    protected readonly IMainViewNavigator MainViewNavigator;
    protected readonly IConnectionManager ConnectionManager;

    protected LocationViewModelBase(
        ILocalizationProvider localizationProvider,
        IMainViewNavigator mainViewNavigator,
        IConnectionManager connectionManager) : base(localizationProvider)
    {
        MainViewNavigator = mainViewNavigator;
        ConnectionManager = connectionManager;

        ConnectionDetails = ConnectionManager.CurrentConnectionDetails;
    }

    public ConnectionDetails? ConnectionDetails { get; }

    public abstract bool IsActiveConnection { get; }

    protected abstract ConnectionIntent ConnectionIntent { get; }

    [RelayCommand]
    public async Task ConnectAsync()
    {
        await MainViewNavigator.NavigateToAsync<HomeViewModel>();
        await ConnectionManager.ConnectAsync(ConnectionIntent);
    }
}