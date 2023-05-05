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
using ProtonVPN.Gui.Messages;

namespace ProtonVPN.Gui.Contracts.ViewModels;

public abstract partial class PageViewModelBase : ViewModelBase, INavigationAware
{
    public PageViewModelBase(INavigationService navigationService)
    {
        NavigationService = navigationService;
    }

    public Type PageType => GetType();

    public virtual string? Title { get; }

    public virtual bool CanGoBack => true;

    public virtual bool IsHeaderVisible => true;

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
        IsActive = false;
    }

    public virtual void OnNavigatedTo(object parameter)
    {
        IsActive = true;
    }

    public void InvalidateTitle()
    {
        OnPropertyChanged(nameof(Title));
    }

    protected override void OnLanguageChanged()
    {
        InvalidateTitle();
    }
}
