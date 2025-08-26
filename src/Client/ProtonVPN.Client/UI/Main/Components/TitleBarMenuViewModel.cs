/*
 * Copyright (c) 2025 Proton AG
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
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Commands;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Contracts.Services.Lifecycle;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Models.ReportIssue;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Feedback.Contracts;
using ProtonVPN.Client.Logic.Updates.Contracts;
using ProtonVPN.Client.Mappers;
using ProtonVPN.Client.Services.SignoutHandling;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.UI.Main.Components;

public partial class TitleBarMenuViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<AuthenticationStatusChanged>,
    IEventMessageReceiver<ClientUpdateStateChangedMessage>
{
    public const string REPORT_ISSUE_CATEGORY_PLACEHOLDER = "what-is-the-issue-placeholder";

    private readonly IAppExitInvoker _appExitInvoker;
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly ISignOutHandler _signoutHandler;
    private readonly IWebAuthenticator _webAuthenticator;
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly IMainWindowViewNavigator _mainWindowViewNavigator;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IReportIssueWindowActivator _reportIssueWindowActivator;
    private readonly IReportIssueViewNavigator _reportIssueViewNavigator;
    private readonly IReportIssueDataProvider _reportIssueDataProvider;
    private readonly IUpdatesManager _updatesManager;
    private readonly ISettingsViewNavigator _settingsViewNavigator;
    private readonly ISettings _settings;
    private readonly IDebugToolsWindowActivator _debugToolsWindowActivator;

    public bool IsUpdateAvailable => _updatesManager.IsUpdateAvailable;

    public bool IsVisible => _userAuthenticator.IsLoggedIn && _mainWindowViewNavigator.GetCurrentPageContext() is MainPageViewModel;

    public bool IsDebugModeEnabled => _settings.IsDebugModeEnabled;

    public SmartObservableCollection<IssueCategory> ReportIssueCategories { get; } = [];

    public IAsyncRelayCommand UpdateCommand { get; }

    public TitleBarMenuViewModel(
        IAppExitInvoker appExitInvoker,
        IUrlsBrowser urlsBrowser,
        ISignOutHandler signoutHandler,
        IWebAuthenticator webAuthenticator,
        IMainViewNavigator mainViewNavigator,
        IMainWindowViewNavigator mainWindowViewNavigator,
        IUserAuthenticator userAuthenticator,
        IReportIssueWindowActivator reportIssueWindowActivator,
        IReportIssueViewNavigator reportIssueViewNavigator,
        IReportIssueDataProvider reportIssueDataProvider,
        IUpdatesManager updatesManager,
        ISettingsViewNavigator settingsViewNavigator,
        ISettings settings,
        IDebugToolsWindowActivator debugToolsWindowActivator,
        IUpdateClientCommand updateClientCommand,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _appExitInvoker = appExitInvoker;
        _urlsBrowser = urlsBrowser;
        _signoutHandler = signoutHandler;
        _webAuthenticator = webAuthenticator;
        _mainViewNavigator = mainViewNavigator;
        _mainWindowViewNavigator = mainWindowViewNavigator;
        _userAuthenticator = userAuthenticator;
        _reportIssueWindowActivator = reportIssueWindowActivator;
        _reportIssueViewNavigator = reportIssueViewNavigator;
        _reportIssueDataProvider = reportIssueDataProvider;
        _updatesManager = updatesManager;
        _settingsViewNavigator = settingsViewNavigator;
        _settings = settings;
        _debugToolsWindowActivator = debugToolsWindowActivator;

        UpdateCommand = updateClientCommand.Command;

        _mainWindowViewNavigator.Navigated += OnMainWindowNavigated;
    }

    private void OnMainWindowNavigated(object sender, NavigationEventArgs e)
    {
        ExecuteOnUIThread(() => OnPropertyChanged(nameof(IsVisible)));
    }

    public void Receive(AuthenticationStatusChanged message)
    {
        ExecuteOnUIThread(() => OnPropertyChanged(nameof(IsVisible)));
    }

    public void Receive(ClientUpdateStateChangedMessage message)
    {
        ExecuteOnUIThread(() => OnPropertyChanged(nameof(IsUpdateAvailable)));
    }

    protected override async void OnActivated()
    {
        base.OnActivated();

        await InvalidateReportIssueCategoriesAsync();
    }

    protected override async void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        await InvalidateReportIssueCategoriesAsync();
    }

    private async Task InvalidateReportIssueCategoriesAsync()
    {
        List<IssueCategory> categories = (await _reportIssueDataProvider.GetCategoriesAsync())
            .Select(ReportIssueMapper.Map)
            .Prepend(new IssueCategory()
            {
                Key = REPORT_ISSUE_CATEGORY_PLACEHOLDER,
                Name = Localizer.Get("Home_TitleBar_Menu_Help_WhatsTheIssue")
            })
            .ToList();

        ReportIssueCategories.Reset(categories);
    }

    [RelayCommand]
    private async Task OpenMyAccountUrlAsync()
    {
        _urlsBrowser.BrowseTo(await _webAuthenticator.GetMyAccountUrlAsync());
    }

    [RelayCommand]
    private Task NavigateToSettingsAsync()
    {
        return _mainViewNavigator.NavigateToSettingsViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToAboutAsync()
    {
        await _mainViewNavigator.NavigateToSettingsViewAsync();
        await _settingsViewNavigator.NavigateToAboutViewAsync();
    }

    [RelayCommand]
    private Task SignoutAsync()
    {
        return _signoutHandler.SignOutAsync();
    }

    [RelayCommand]
    private async Task ExitApplicationAsync()
    {
        await _appExitInvoker.ExitWithConfirmationAsync();
    }

    [RelayCommand]
    private Task HandleCategoryClick(IssueCategory category)
    {
        _reportIssueWindowActivator.Activate();

        return _reportIssueViewNavigator.NavigateToCategoryViewAsync(category);
    }

    [RelayCommand(CanExecute = nameof(CanShowDebugTools))]
    private void ShowDebugTools()
    {
        _debugToolsWindowActivator.Activate();
    }

    private bool CanShowDebugTools()
    {
        return IsDebugModeEnabled;
    }
}