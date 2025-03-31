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

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Messages;
using ProtonVPN.Client.Core.Models;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Activation.Bases;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Profiles.Contracts.Messages;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Updates.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;

namespace ProtonVPN.Client.UI.Main.Settings.Pages;

public partial class CommonSettingsPageViewModel : SettingsPageViewModelBase
{
    private readonly IUpdatesManager _updatesManager;
    private readonly IApplicationThemeSelector _themeSelector;
    private readonly ILocalizationService _localizationService;
    private readonly IOverlayActivator _mainWindowOverlayActivator;
    private readonly ISettings _settings;
    private readonly ISettingsRestorer _settingsRestorer;
    private readonly IUrlsBrowser _urlsBrowser;

    private readonly IReportIssueWindowActivator _reportIssueWindowActivator;
    private readonly IDebugToolsWindowActivator _debugToolsWindowActivator;
    private readonly IConnectionManager _connectionManager;
    private readonly IRecentConnectionsManager _recentConnectionsManager;
    private readonly Lazy<ObservableCollection<Language>> _languages;

    public bool IsPaidUser => _settings.VpnPlan.IsPaid;

    public bool IsToShowDeveloperTools => _settings.IsDebugModeEnabled;

    public string ClientVersionDescription => $"{Localizer.Get("Settings_AppVersion")}: {AssemblyVersion.Get()}";

    public override string Title => Localizer.Get("Settings_Page_Title");

    public string OperatingSystemVersionDescription => OSVersion.GetString();

    public ApplicationElementTheme SelectedTheme
    {
        get => Themes.First(theme => _themeSelector.GetTheme() == theme.Theme);
        set => _themeSelector.SetTheme(value.Theme);
    }

    public Language SelectedLanguage
    {
        get => _localizationService.GetLanguage(_settings.Language);
        set => _settings.Language = value.Id;
    }

    public string DefaultConnectionState
    {
        get
        {
            if (_settings.DefaultConnection.Type == DefaultConnectionType.Recent)
            {
                IConnectionIntent? connectionIntent = _recentConnectionsManager.GetById(_settings.DefaultConnection.RecentId)?.ConnectionIntent;
                return connectionIntent is not null
                    ? connectionIntent is IConnectionProfile profile
                        ? profile.Name
                        : Localizer.GetConnectionIntentTitle(connectionIntent)
                    : string.Empty;
            }

            return Localizer.Get($"Settings_Connection_Default_{_settings.DefaultConnection.Type}");
        }
    }

    public bool IsNotificationEnabled
    {
        get => _settings.IsNotificationEnabled;
        set => _settings.IsNotificationEnabled = value;
    }

    public bool IsBetaAccessEnabled
    {
        get => _settings.IsBetaAccessEnabled;
        set => _settings.IsBetaAccessEnabled = value;
    }

    public bool IsAutomaticUpdatesEnabled
    {
        get => _settings.AreAutomaticUpdatesEnabled;
        set => _settings.AreAutomaticUpdatesEnabled = value;
    }

    public List<ApplicationElementTheme> Themes { get; }

    public ObservableCollection<Language> Languages => _languages.Value;

    public CommonSettingsPageViewModel(
        IUpdatesManager updatesManager,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        IMainViewNavigator mainViewNavigator,
        IApplicationThemeSelector themeSelector,
        ILocalizationService localizationService,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        ISettingsRestorer settingsRestorer,
        IUrlsBrowser urlsBrowser,
        IReportIssueWindowActivator reportIssueWindowActivator,
        IDebugToolsWindowActivator debugToolsWindowActivator,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager,
        IRecentConnectionsManager recentConnectionsManager,
        ISettingsViewNavigator settingsViewNavigator,
        IViewModelHelper viewModelHelper)
        : base(requiredReconnectionSettings,
               mainViewNavigator,
               settingsViewNavigator,
               mainWindowOverlayActivator,
               settings,
               settingsConflictResolver,
               connectionManager,
               viewModelHelper)
    {
        _updatesManager = updatesManager;
        _themeSelector = themeSelector;
        _localizationService = localizationService;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _settings = settings;
        _settingsRestorer = settingsRestorer;
        _urlsBrowser = urlsBrowser;
        _reportIssueWindowActivator = reportIssueWindowActivator;
        _debugToolsWindowActivator = debugToolsWindowActivator;
        _connectionManager = connectionManager;
        _recentConnectionsManager = recentConnectionsManager;

        _languages = new Lazy<ObservableCollection<Language>>(
            () => new ObservableCollection<Language>(_localizationService.GetAvailableLanguages()));

        Themes = _themeSelector.GetAvailableThemes().Select(theme => new ApplicationElementTheme(Localizer, theme)).ToList();
    }

    [RelayCommand]
    public async Task RestoreDefaultSettingsAsync()
    {
        bool needReconnection = !_connectionManager.IsDisconnected;

        ContentDialogResult result = await _mainWindowOverlayActivator.ShowMessageAsync(
            new MessageDialogParameters
            {
                Title = Localizer.Get("Settings_RestoreDefault_Confirmation_Title"),
                Message = Localizer.Get("Settings_RestoreDefault_Confirmation_Message"),
                PrimaryButtonText = needReconnection
                    ? Localizer.Get("Settings_RestoreDefaultAndReconnect_Confirmation_Action")
                    : Localizer.Get("Settings_RestoreDefault_Confirmation_Action"),
                CloseButtonText = Localizer.Get("Common_Actions_Cancel"),
                UseVerticalLayoutForButtons = true
            });

        if (result == ContentDialogResult.Primary)
        {
            _settingsRestorer.Restore();

            if (needReconnection)
            {
                await _connectionManager.ReconnectAsync(VpnTriggerDimension.NewConnection);
            }
        }
    }

    [RelayCommand]
    public void OpenSupport()
    {
        _urlsBrowser.BrowseTo(_urlsBrowser.SupportCenter);
    }

    [RelayCommand]
    public Task ReportIssueAsync()
    {
        return _reportIssueWindowActivator.ActivateAsync();
    }

    [RelayCommand]
    private async Task NavigateToDefaultConnectionPageAsync()
    {
        await ParentViewNavigator.NavigateToDefaultConnectionSettingsViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToAutoStartupPageAsync()
    {
        await ParentViewNavigator.NavigateToAutoStartupSettingsViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToDebugLogsPageAsync()
    {
        await ParentViewNavigator.NavigateToDebugLogsSettingsViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToAboutPageAsync()
    {
        await ParentViewNavigator.NavigateToAboutViewAsync();
    }

    [RelayCommand]
    private void ShowDebugTools()
    {
        _debugToolsWindowActivator.Activate();
    }

    [RelayCommand]
    private async Task NavigateToCensorshipPageAsync()
    {
        await ParentViewNavigator.NavigateToCensorshipViewAsync();
    }

    public void Receive(ThemeChangedMessage message)
    {
        ExecuteOnUIThread(() => OnPropertyChanged(nameof(SelectedTheme)));
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(InvalidateAllProperties);
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateAllProperties);
    }

    public void Receive(ProfilesChangedMessage message)
    {
        ExecuteOnUIThread(() => OnPropertyChanged(nameof(DefaultConnectionState)));
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(DefaultConnectionState));
        OnPropertyChanged(nameof(SelectedLanguage));
        OnPropertyChanged(nameof(ClientVersionDescription));

        foreach (ApplicationElementTheme theme in Themes)
        {
            theme.OnLanguageChanged();
        }
    }

    protected override void OnSettingsChanged(string propertyName)
    {
        base.OnSettingsChanged(propertyName);

        switch (propertyName)
        {
            case nameof(ISettings.DefaultConnection):
                OnPropertyChanged(nameof(DefaultConnectionState));
                break;

            case nameof(ISettings.IsNotificationEnabled):
                OnPropertyChanged(nameof(IsNotificationEnabled));
                break;

            case nameof(ISettings.IsBetaAccessEnabled):
                OnPropertyChanged(nameof(IsBetaAccessEnabled));
                break;

            case nameof(ISettings.VpnPlan):
                OnPropertyChanged(nameof(IsPaidUser));
                break;
        }
    }
}