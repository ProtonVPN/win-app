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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Contracts.ViewModels;

namespace ProtonVPN.Client.Models.Navigation;

public interface IViewNavigator
{
    bool CanGoBack { get; }

    bool CanNavigate { get; set; }

    Window Window { get; set; }

    Frame Frame { get; set; }

    event NavigatedEventHandler Navigated;

    Task<bool> GoBackAsync();

    Task<bool> NavigateToAsync(string pageKey, object? parameter = null, bool clearNavigation = false, bool forceNavigation = false);

    Task<bool> NavigateToAsync<TPageViewModel>(object? parameter = null, bool clearNavigation = false, bool forceNavigation = false)
        where TPageViewModel : PageViewModelBase;

    void CloseCurrentWindow();

    void ResetContent();
}