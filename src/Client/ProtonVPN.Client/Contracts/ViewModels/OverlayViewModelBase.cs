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
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;

namespace ProtonVPN.Client.Contracts.ViewModels;

public abstract partial class OverlayViewModelBase : ViewModelBase
{
    protected readonly IViewNavigator ViewNavigator;
    protected readonly IOverlayActivator OverlayActivator;

    protected OverlayViewModelBase(
        ILocalizationProvider localizationProvider, 
        IViewNavigator viewNavigator, 
        IOverlayActivator overlayActivator)
        : base(localizationProvider)
    {
        ViewNavigator = viewNavigator;
        OverlayActivator = overlayActivator;
    }

    private string OverlayKey => GetType().FullName!;

    [RelayCommand]
    public void CloseOverlay()
    {
        OverlayActivator.CloseOverlay(OverlayKey, ViewNavigator.Window);
    }

    public async Task ShowOverlayAsync()
    {
        await OverlayActivator.ShowOverlayAsync(OverlayKey, ViewNavigator.Window);
    }
}