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

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts.Messages;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Activation.Custom;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Themes;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.UI.Settings.Pages;
using ProtonVPN.Client.UI.Settings.Pages.About;
using ProtonVPN.Client.UI.Settings.Pages.DefaultConnections;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using Language = ProtonVPN.Client.Localization.Contracts.Language;

namespace ProtonVPN.Client.UI.Settings;

public partial class SettingsViewModel : NavigationPageViewModelBase,
    IEventMessageReceiver<ThemeChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<ProfilesChangedMessage>
{
    private readonly IThemeSelector _themeSelector;
    private readonly ILocalizationService _localizationService;
    private readonly IOverlayActivator _overlayActivator;
    private readonly ISettings _settings;
    private readonly ISettingsRestorer _settingsRestorer;
    private readonly IUrls _urls;
    private readonly IReportIssueDialogActivator _reportIssueDialogActivator;
    private readonly IUpsellCarouselDialogActivator _upsellCarouselDialogActivator;
    private readonly IConnectionManager _connectionManager;
    private readonly IProfilesManager _profilesManager;
    private readonly Lazy<ObservableCollection<Language>> _languages;

    public bool IsPaidUser => _settings.VpnPlan.IsPaid;

    public bool IsToShowDeveloperTools => _settings.IsDebugModeEnabled;

    public string ClientVersionDescription => $"{Localizer.Get("Settings_AppVersion")}: {AssemblyVersion.Get()}";

    public string OperatingSystemVersionDescription => OSVersion.GetString();

    public ApplicationElementTheme SelectedTheme
    {
        get => _themeSelector.GetTheme();
        set => _themeSelector.SetTheme(value);
    }

    public Language SelectedLanguage
    {
        get => _localizationService.GetLanguage(_settings.Language);
        set => _settings.Language = value.Id;
    }

    public string DefaultConnectionState => _settings.DefaultConnection.Type == DefaultConnectionType.Profile
        ? _profilesManager.GetById(_settings.DefaultConnection.ProfileId)?.Name ?? string.Empty
        : Localizer.Get($"Settings_Connection_Default_{_settings.DefaultConnection.Type}");

    public string ConnectionProtocolState => Localizer.Get($"Settings_SelectedProtocol_{_settings.VpnProtocol}");

    public string VpnAcceleratorSettingsState => Localizer.GetToggleValue(_settings.IsVpnAcceleratorEnabled);

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

    public ObservableCollection<ApplicationElementTheme> Themes => new(_themeSelector.GetAvailableThemes());

    public ObservableCollection<Language> Languages => _languages.Value;

    public override bool IsBackEnabled => false;

    public override IconElement Icon { get; } = new CogWheel();
    public override string? Title => Localizer.Get("Settings_Page_Title");

    public SettingsViewModel(
        IThemeSelector themeSelector,
        IMainViewNavigator viewNavigator,
        ILocalizationService localizationService,
        ILocalizationProvider localizationProvider,
        IOverlayActivator overlayActivator,
        ISettings settings,
        ISettingsRestorer settingsRestorer,
        IUrls urls,
        IReportIssueDialogActivator reportIssueDialogActivator,
        IUpsellCarouselDialogActivator upsellCarouselDialogActivator,
        IConnectionManager connectionManager,
        ILogger logger,
        IIssueReporter issueReporter,
        IProfilesManager profilesManager)
        : base(viewNavigator, localizationProvider, logger, issueReporter)
    {
        _themeSelector = themeSelector;
        _localizationService = localizationService;
        _overlayActivator = overlayActivator;
        _settings = settings;
        _settingsRestorer = settingsRestorer;
        _urls = urls;
        _reportIssueDialogActivator = reportIssueDialogActivator;
        _upsellCarouselDialogActivator = upsellCarouselDialogActivator;
        _connectionManager = connectionManager;
        _profilesManager = profilesManager;

        _languages = new Lazy<ObservableCollection<Language>>(
            () => new ObservableCollection<Language>(_localizationService.GetAvailableLanguages()));
    }

    [RelayCommand]
    public async Task RestoreDefaultSettingsAsync()
    {
        bool needReconnection = !_connectionManager.IsDisconnected;

        ContentDialogResult result = await _overlayActivator.ShowMessageAsync(
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
                await _connectionManager.ReconnectAsync();
            }
        }
    }

    [RelayCommand]
    public void OpenSupport()
    {
        _urls.NavigateTo(_urls.SupportCenter);
    }

    [RelayCommand]
    public async Task ReportIssueAsync()
    {
        await _reportIssueDialogActivator.ShowDialogAsync();
    }

    [RelayCommand]
    private async Task NavigateToDefaultConnectionPageAsync()
    {
        await ViewNavigator.NavigateToAsync<DefaultConnectionViewModel>();
    }

    [RelayCommand]
    private async Task NavigateToProtocolPageAsync()
    {
        await ViewNavigator.NavigateToAsync<ProtocolViewModel>();
    }

    [RelayCommand]
    private async Task NavigateToVpnAcceleratorPageAsync()
    {
        if (!IsPaidUser)
        {
            _upsellCarouselDialogActivator.ShowDialog(ModalSources.VpnAccelerator);
            return;
        }

        await ViewNavigator.NavigateToAsync<VpnAcceleratorViewModel>();
    }

    [RelayCommand]
    private async Task NavigateToAdvancedSettingsPageAsync()
    {
        await ViewNavigator.NavigateToAsync<AdvancedSettingsViewModel>();
    }

    [RelayCommand]
    private async Task NavigateToAutoStartupPageAsync()
    {
        await ViewNavigator.NavigateToAsync<AutoStartupViewModel>();
    }

    [RelayCommand]
    private async Task NavigateToDebugLogsPageAsync()
    {
        await ViewNavigator.NavigateToAsync<DebugLogsViewModel>();
    }

    [RelayCommand]
    private async Task NavigateToAboutPageAsync()
    {
        await ViewNavigator.NavigateToAsync<AboutViewModel>();
    }

    [RelayCommand]
    private async Task NavigateToDeveloperToolsPageAsync()
    {
        await ViewNavigator.NavigateToAsync<DeveloperToolsViewModel>();
    }

    [RelayCommand]
    private async Task NavigateToCensorshipPageAsync()
    {
        await ViewNavigator.NavigateToAsync<CensorshipViewModel>();
    }

    public void Receive(SettingChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            switch (message.PropertyName)
            {
                case nameof(ISettings.DefaultConnection):
                    OnPropertyChanged(nameof(DefaultConnectionState));
                    break;

                case nameof(ISettings.VpnProtocol):
                    OnPropertyChanged(nameof(ConnectionProtocolState));
                    break;

                case nameof(ISettings.IsVpnAcceleratorEnabled):
                    OnPropertyChanged(nameof(VpnAcceleratorSettingsState));
                    break;

                case nameof(ISettings.IsNotificationEnabled):
                    OnPropertyChanged(nameof(IsNotificationEnabled));
                    break;

                case nameof(ISettings.IsBetaAccessEnabled):
                    OnPropertyChanged(nameof(IsBetaAccessEnabled));
                    break;
            }
        });
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
        OnPropertyChanged(nameof(ConnectionProtocolState));
        OnPropertyChanged(nameof(VpnAcceleratorSettingsState));
        OnPropertyChanged(nameof(SelectedLanguage));
        OnPropertyChanged(nameof(SelectedTheme));
        OnPropertyChanged(nameof(ClientVersionDescription));
        OnPropertyChanged(nameof(Themes));
        OnPropertyChanged(nameof(SelectedTheme));
    }
}