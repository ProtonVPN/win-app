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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Gui.Contracts.Services;

namespace ProtonVPN.Gui.Contracts.ViewModels;

public abstract partial class PageViewModelBase : ObservableRecipient, INavigationAware
{
    [ObservableProperty]
    private bool _canGoBack;

    [ObservableProperty]
    private bool _isHeaderVisible;

    [ObservableProperty]
    private string? _title;

    public PageViewModelBase(INavigationService navigationService)
    {
        NavigationService = navigationService;
    }

    protected PageViewModelBase(INavigationService navigationService, string title)
        : this(navigationService)
    {
        _title = title;
        _isHeaderVisible = true;
    }

    protected PageViewModelBase(INavigationService navigationService, string title, bool canGoBack)
        : this(navigationService, title)
    {
        _canGoBack = canGoBack;
    }

    protected INavigationService NavigationService { get; }

    [RelayCommand]
    public void GoBack()
    {
        NavigationService.GoBack();
    }

    [RelayCommand]
    public void NavigateTo(string pageKey)
    {
        NavigationService.NavigateTo(pageKey);
    }

    public virtual void OnNavigatedFrom()
    {
        OnDeactivated();
    }

    public virtual void OnNavigatedTo(object parameter)
    {
        OnActivated();
    }
}