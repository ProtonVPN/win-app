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
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Themes;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.UI.ReportIssue;
using ProtonVPN.Client.UI.Settings.Pages;
using ProtonVPN.Common.Core.Helpers;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Settings;

public partial class SettingsViewModel : NavigationPageViewModelBase,
    IEventMessageReceiver<ThemeChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<PortForwardingPortChanged>,
    IEventMessageReceiver<PortForwardingStatusChanged>,
    IEventMessageReceiver<ConnectionStatusChanged>
{
    private readonly IThemeSelector _themeSelector;
    private readonly ILocalizationService _localizationService;
    private readonly IOverlayActivator _overlayActivator;
    private readonly ISettings _settings;
    private readonly ISettingsRestorer _settingsRestorer;
    private readonly IUrls _urls;
    private readonly IDialogActivator _dialogActivator;
    private readonly IReportIssueViewNavigator _reportIssueViewNavigator;
    private readonly IConnectionManager _connectionManager;
    private readonly IPortForwardingManager _portForwardingManager;
    private readonly Lazy<ObservableCollection<string>> _languages;


    public bool IsToShowDeveloperTools => _settings.IsDebugModeEnabled;

    public string ClientVersionDescription => $"{App.APPLICATION_NAME} {AssemblyVersion.Get()}";
    public string OperatingSystemVersionDescription => OSVersion.GetString();

    public ApplicationElementTheme SelectedTheme
    {
        get => _themeSelector.GetTheme();
        set => _themeSelector.SetTheme(value);
    }

    public string SelectedLanguage
    {
        get => _settings.Language;
        set => _settings.Language = value;
    }

    public string ConnectionProtocolState => Localizer.Get($"Settings_SelectedProtocol_{_settings.VpnProtocol}");

    public ImageSource NetShieldFeatureIconSource => NetShieldViewModel.GetFeatureIconSource(_settings.IsNetShieldEnabled);

    public string NetShieldFeatureState => Localizer.GetToggleValue(_settings.IsNetShieldEnabled);

    public ImageSource KillSwitchFeatureIconSource => KillSwitchViewModel.GetFeatureIconSource(_settings.IsKillSwitchEnabled, _settings.KillSwitchMode);

    public string KillSwitchFeatureState => Localizer.GetToggleValue(_settings.IsKillSwitchEnabled);

    public ImageSource PortForwardingFeatureIconSource => PortForwardingViewModel.GetFeatureIconSource(_settings.IsPortForwardingEnabled);

    public string PortForwardingFeatureState => Localizer.GetToggleValue(_settings.IsPortForwardingEnabled);

    public string? PortForwardingStatusMessage => GetPortForwardingStatusMessage();

    public ImageSource SplitTunnelingFeatureIconSource => SplitTunnelingViewModel.GetFeatureIconSource(_settings.IsSplitTunnelingEnabled);

    public string SplitTunnelingFeatureState => Localizer.GetToggleValue(_settings.IsSplitTunnelingEnabled);

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

    public ObservableCollection<ApplicationElementTheme> Themes { get; }

    public ObservableCollection<string> Languages => _languages.Value;

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
        IDialogActivator dialogActivator,
        IReportIssueViewNavigator reportIssueViewNavigator,
        IConnectionManager connectionManager,
        IPortForwardingManager portForwardingManager,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(viewNavigator, localizationProvider, logger, issueReporter)
    {
        _themeSelector = themeSelector;
        _localizationService = localizationService;
        _overlayActivator = overlayActivator;
        _settings = settings;
        _settingsRestorer = settingsRestorer;
        _urls = urls;
        _dialogActivator = dialogActivator;
        _reportIssueViewNavigator = reportIssueViewNavigator;
        _connectionManager = connectionManager;
        _portForwardingManager = portForwardingManager;

        _languages = new Lazy<ObservableCollection<string>>(
            () => new ObservableCollection<string>(_localizationService.GetAvailableLanguages()));

        Themes = new ObservableCollection<ApplicationElementTheme>(_themeSelector.GetAvailableThemes());
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
        _dialogActivator.ShowDialog<ReportIssueShellViewModel>();

        await _reportIssueViewNavigator.NavigateToCategorySelectionAsync();
    }

    public void Receive(SettingChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            switch (message.PropertyName)
            {
                case nameof(ISettings.IsNetShieldEnabled):
                    OnPropertyChanged(nameof(NetShieldFeatureState));
                    OnPropertyChanged(nameof(NetShieldFeatureIconSource));
                    break;

                case nameof(ISettings.IsKillSwitchEnabled):
                    OnPropertyChanged(nameof(KillSwitchFeatureState));
                    OnPropertyChanged(nameof(KillSwitchFeatureIconSource));
                    break;

                case nameof(ISettings.KillSwitchMode):
                    OnPropertyChanged(nameof(KillSwitchFeatureIconSource));
                    break;

                case nameof(ISettings.IsPortForwardingEnabled):
                    OnPropertyChanged(nameof(PortForwardingFeatureState));
                    OnPropertyChanged(nameof(PortForwardingFeatureIconSource));
                    OnPropertyChanged(nameof(PortForwardingStatusMessage));
                    break;

                case nameof(ISettings.IsSplitTunnelingEnabled):
                    OnPropertyChanged(nameof(SplitTunnelingFeatureState));
                    OnPropertyChanged(nameof(SplitTunnelingFeatureIconSource));
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
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(SelectedTheme));
        });
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            InvalidateAllProperties();
        });
    }

    public void Receive(PortForwardingPortChanged message)
    {
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(PortForwardingStatusMessage));
        });
    }

    public void Receive(PortForwardingStatusChanged message)
    {
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(PortForwardingStatusMessage));
        });
    }

    public void Receive(ConnectionStatusChanged message)
    {
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(PortForwardingStatusMessage));
        });
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(ConnectionProtocolState));
        OnPropertyChanged(nameof(NetShieldFeatureState));
        OnPropertyChanged(nameof(KillSwitchFeatureState));
        OnPropertyChanged(nameof(PortForwardingFeatureState));
        OnPropertyChanged(nameof(SplitTunnelingFeatureState));
        OnPropertyChanged(nameof(VpnAcceleratorSettingsState));
        OnPropertyChanged(nameof(SelectedLanguage));
        OnPropertyChanged(nameof(SelectedTheme));
    }

    private string? GetPortForwardingStatusMessage()
    {
        if (!_connectionManager.IsConnected || !_settings.IsPortForwardingEnabled)
        {
            return null;
        }

        int? activePort = _portForwardingManager.ActivePort;
        if (activePort is not null)
        {
            return $"{Localizer.Get("Settings_Features_PortForwarding_ActivePort")} {activePort}";
        }

        return _portForwardingManager.IsFetchingPort
            ? $"{Localizer.Get("Settings_Features_PortForwarding_ActivePort")} " +
              $"{Localizer.Get("Settings_Features_PortForwarding_Loading")}" 
            : null;
    }
}