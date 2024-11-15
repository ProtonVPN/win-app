/*
 * Copyright (c) 2024 Proton AG
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

using System.Threading.Tasks;

namespace ProtonVPN.Client.Core.Bases;

public interface INavigationAware
{
    /// <summary>
    /// Called when parent frame navigates from this page BEFORE the navigation happened.
    /// </summary>
    /// <returns>Returns true to proceed with navigation, false to cancel it</returns>
    Task<bool> CanNavigateFromAsync();

    /// <summary>
    /// Called when parent frame navigates from this page.
    /// Navigation has already happened, page is no longer visible.
    /// </summary>
    void OnNavigatedFrom();

    /// <summary>
    /// Called when parent frame navigates to this page.
    /// Navigation has already happened, page is visible.
    /// </summary>
    /// <param name="parameter">Can be used to pass any type of argument</param>
    /// <param name="isBackNavigation">Indicates whether the user navigated to this page using the back button</param>
    void OnNavigatedTo(object parameter, bool isBackNavigation);
}