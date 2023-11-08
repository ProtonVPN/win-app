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
using ProtonVPN.Client.Models.Navigation;

namespace ProtonVPN.Client.UI.Countries;

public partial class ServerViewModel : ViewModelBase
{
    private readonly IMainViewNavigator _mainViewNavigator;

    public string Name { get; init; } = string.Empty;

    public double Load { get; init; }

    public string LoadPercent { get; init; }

    public bool IsVirtual { get; init; }

    public bool SupportsP2P { get; init; }

    public bool IsUnderMaintenance { get; init; }

    public bool IsActive { get; init; }

    public ServerViewModel(ILocalizationProvider localizationProvider, IMainViewNavigator mainViewNavigator) : base(
        localizationProvider)
    {
        _mainViewNavigator = mainViewNavigator;
    }

    [RelayCommand]
    public async Task ConnectAsync(ServerViewModel serverViewModel)
    {
        // TODO: connect to serverViewModel
    }
}