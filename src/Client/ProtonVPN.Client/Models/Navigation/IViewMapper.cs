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

using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Core.Modals;

namespace ProtonVPN.Client.Models.Navigation;

public interface IViewMapper
{
    Type GetPageType<TPageViewModel>() 
        where TPageViewModel : PageViewModelBase;

    Type GetOverlayType<TOverlayViewModel>() 
        where TOverlayViewModel : OverlayViewModelBase;

    Type GetDialogType<TPageViewModel>() 
        where TPageViewModel : PageViewModelBase;

    public Type GetPageType(Type viewModelType);

    public Type GetOverlayType(Type viewModelType);

    public Type GetDialogType(Type viewModelType);

    public PageViewModelBase GetPageViewModel(Type pageType);

    public OverlayViewModelBase GetOverlayViewModel(Type overlayType);

    public PageViewModelBase GetDialogViewModel(Type dialogType);
}