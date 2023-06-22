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
using ProtonVPN.Client.Contracts.Services;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;

namespace ProtonVPN.Client.UI.Dialogs.Overlays;

public partial class ProtocolOverlayViewModel : OverlayViewModelBase
{
    private readonly INavigationService _navigationService;

    public Uri LearnMoreUri { get; } = new Uri(@"https://protonvpn.com/support/how-to-change-vpn-protocols/");

    public ProtocolOverlayViewModel(ILocalizationProvider localizationProvider, IDialogService dialogService, INavigationService navigationService)
        : base(localizationProvider, dialogService)
    {
        _navigationService = navigationService;
    }

    [RelayCommand]
    public void NavigateTo(string pageKey)
    {
        CloseOverlay();

        _navigationService.NavigateTo(pageKey);
    }
}