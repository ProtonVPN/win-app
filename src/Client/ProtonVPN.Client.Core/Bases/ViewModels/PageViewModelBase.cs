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

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Core.Services.Navigation.Bases;

namespace ProtonVPN.Client.Core.Bases.ViewModels;

public abstract class PageViewModelBase : ActivatableViewModelBase, INavigationAware
{
    public virtual string Title => string.Empty;

    protected PageViewModelBase(IViewModelHelper viewModelHelper) : base(viewModelHelper)
    {
    }

    public event EventHandler? ResetContentScrollRequested;

    public void InvalidateTitle()
    {
        OnPropertyChanged(nameof(Title));
    }

    public virtual Task<bool> CanNavigateFromAsync()
    {
        return Task.FromResult(true);
    }

    public virtual void OnNavigatedFrom()
    { }

    public virtual void OnNavigatedTo(object parameter, bool isBackNavigation)
    {
        if (!isBackNavigation)
        {
            RequestResetContentScroll();
        }
    }

    protected override void OnLanguageChanged()
    {
        InvalidateTitle();
    }

    public void RequestResetContentScroll()
    {
        ExecuteOnUIThread(() => ResetContentScrollRequested?.Invoke(this, EventArgs.Empty));
    }
}

public abstract partial class PageViewModelBase<TParentViewNavigator> : PageViewModelBase, INavigatorAware
    where TParentViewNavigator : IViewNavigator
{
    protected readonly TParentViewNavigator ParentViewNavigator;

    public bool CanGoBack => ParentViewNavigator.CanGoBack;

    public bool CanNavigate => ParentViewNavigator.CanNavigate;

    public PageViewModelBase(
        TParentViewNavigator parentViewNavigator,
        IViewModelHelper viewModelHelper) : base(viewModelHelper)
    {
        ParentViewNavigator = parentViewNavigator;
        ParentViewNavigator.Navigated += OnNavigated;
    }

    public Task<bool> InvokeAsync(object? parameter = null, bool clearNavigation = false, bool forceNavigation = false)
    {
        return ParentViewNavigator.NavigateToAsync(this, parameter, clearNavigation, forceNavigation);
    }

    public Task<bool> GoBackAsync(bool forceNavigation = false)
    {
        return ParentViewNavigator.GoBackAsync(forceNavigation);
    }

    protected virtual void OnNavigation(NavigationEventArgs e)
    {
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanNavigate));
        NavigateBackCommand.NotifyCanExecuteChanged();
        NavigateToPageCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanNavigate))]
    private Task<bool> NavigateToPageAsync()
    {
        return InvokeAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private Task<bool> NavigateBackAsync()
    {
        return GoBackAsync();
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        OnNavigation(e);
    }
}

public abstract partial class PageViewModelBase<TParentViewNavigator, TChildViewNavigator> : PageViewModelBase<TParentViewNavigator>, INavigationHost
    where TParentViewNavigator : IViewNavigator
    where TChildViewNavigator : IViewNavigator
{
    protected readonly TChildViewNavigator ChildViewNavigator;

    public bool CanChildGoBack => ChildViewNavigator.CanGoBack;

    protected PageViewModelBase(
        TParentViewNavigator parentViewNavigator,
        TChildViewNavigator childViewNavigator,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator, viewModelHelper)
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