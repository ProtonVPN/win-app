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

using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Core.Services.Navigation.Bases;

namespace ProtonVPN.Client.Core.Bases.ViewModels;

public abstract partial class HostViewModelBase<TChildViewNavigator> : ActivatableViewModelBase, INavigationHost
    where TChildViewNavigator : IViewNavigator
{
    protected readonly TChildViewNavigator ChildViewNavigator;

    public bool CanChildGoBack => ChildViewNavigator.CanGoBack;

    protected HostViewModelBase(
        TChildViewNavigator childViewNavigator,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        ChildViewNavigator = childViewNavigator;
        ChildViewNavigator.Navigated += OnChildNavigated;
    }

    public Task<bool> ChildGoBackAsync(bool forceNavigation = false)
    {
        return ChildViewNavigator.GoBackAsync(forceNavigation);
    }

    protected virtual void OnChildNavigation(NavigationEventArgs e)
    {
        OnPropertyChanged(nameof(CanChildGoBack));
        ChildNavigateBackCommand.NotifyCanExecuteChanged();
    }

    private void OnChildNavigated(object sender, NavigationEventArgs e)
    {
        OnChildNavigation(e);
    }

    [RelayCommand(CanExecute = nameof(CanChildGoBack))]
    private Task<bool> ChildNavigateBackAsync()
    {
        return ChildGoBackAsync();
    }
}