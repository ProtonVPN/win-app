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
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models.Navigation;

namespace ProtonVPN.Client.Contracts.ViewModels;

public abstract partial class PageViewModelBase : ActivatableViewModelBase, INavigationAware,
    IEventMessageReceiver<NavigationDisplayModeChangedMessage>
{
    [ObservableProperty]
    private bool _isNavigationPaneCollapsed;

    public PageViewModelBase(IPageNavigator pageNavigator, ILocalizationProvider localizationProvider)
        : base(localizationProvider)
    {
        PageNavigator = pageNavigator;
    }

    public Type PageType => GetType();

    public virtual string? Title { get; }

    public virtual bool IsBackEnabled => true;

    protected IPageNavigator PageNavigator { get; }

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    public void GoBack()
    {
        PageNavigator.GoBack();
    }

    public bool CanGoBack()
    {
        return IsBackEnabled;
    }

    [RelayCommand]
    public void NavigateTo(string pageKey)
    {
        PageNavigator.NavigateTo(pageKey);
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

    public void Receive(NavigationDisplayModeChangedMessage message)
    {
        IsNavigationPaneCollapsed = message.Value == NavigationViewDisplayMode.Minimal;
    }
}
