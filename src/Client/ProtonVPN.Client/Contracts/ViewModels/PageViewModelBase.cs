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
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Contracts.ViewModels;

public abstract partial class PageViewModelBase : ActivatableViewModelBase, IEventMessageReceiver<NavigationDisplayModeChangedMessage>
{
    [ObservableProperty]
    private bool _isNavigationPaneCollapsed;

    public virtual bool IsBackEnabled => true;

    public event EventHandler ResetContentScrollRequested;

    public Type PageType => GetType();

    public virtual string? Title { get; }

    protected PageViewModelBase(ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(localizationProvider, logger, issueReporter)
    { }

    public void InvalidateTitle()
    {
        OnPropertyChanged(nameof(Title));
    }

    public void Receive(NavigationDisplayModeChangedMessage message)
    {
        IsNavigationPaneCollapsed = message.Value == NavigationViewDisplayMode.Minimal;
    }

    protected override void OnLanguageChanged()
    {
        InvalidateTitle();
    }

    protected void RequestResetContentScroll()
    {
        ExecuteOnUIThread(() => ResetContentScrollRequested?.Invoke(this, EventArgs.Empty));        
    }
}

public abstract partial class PageViewModelBase<TViewNavigator> : PageViewModelBase, INavigationAware
    where TViewNavigator : IViewNavigator
{
    protected TViewNavigator ViewNavigator { get; }

    public PageViewModelBase(TViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(localizationProvider, logger, issueReporter)
    {
        ViewNavigator = viewNavigator;
    }

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    public async Task GoBackAsync()
    {
        await ViewNavigator.GoBackAsync();
    }

    public bool CanGoBack()
    {
        return IsBackEnabled;
    }

    [RelayCommand]
    public async Task<bool> NavigateToAsync(string pageKey)
    {
        return await ViewNavigator.NavigateToAsync(pageKey);
    }

    public async Task<bool> NavigateToAsync<TPageViewModel>()
        where TPageViewModel : PageViewModelBase
    {
        return await ViewNavigator.NavigateToAsync<TPageViewModel>();
    }

    public virtual Task<bool> OnNavigatingFromAsync()
    {
        return Task.FromResult(true);
    }

    public virtual void OnNavigatedFrom()
    {
        IsActive = false;
    }

    public virtual void OnNavigatedTo(object parameter, bool isBackNavigation)
    {
        IsActive = true;

        InvalidateAllProperties();

        if (!isBackNavigation)
        {
            RequestResetContentScroll();
        }
    }
}