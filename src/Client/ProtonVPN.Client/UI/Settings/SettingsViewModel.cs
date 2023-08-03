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
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Helpers;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Parameters;
using ProtonVPN.Client.Models.Themes;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.UI.ReportIssue;
using ProtonVPN.Client.UI.ReportIssue.Steps;

namespace ProtonVPN.Client.UI.Settings;

public partial class SettingsViewModel : NavigationPageViewModelBase, IEventMessageReceiver<ThemeChangedMessage>, IEventMessageReceiver<SettingChangedMessage>
{
    private readonly IThemeSelector _themeSelector;
    private readonly ILocalizationService _localizationService;
    private readonly ISettings _settings;
    private readonly ISettingsRestorer _settingsRestorer;
    private readonly IUrls _urls;
    private readonly IDialogActivator _dialogActivator;
    private readonly IReportIssueViewNavigator _reportIssueViewNavigator;
    private readonly Lazy<ObservableCollection<string>> _languages;

    public string ClientVersionDescription => $"{App.APPLICATION_NAME} {EnvironmentHelper.GetClientVersionDescription()}";

    public string OperatingSystemVersionDescription => EnvironmentHelper.GetOSVersionDescription();

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

    public string NetShieldFeatureState => Localizer.Get($"Common_States_Off"); // TODO

    public string KillSwitchFeatureState => Localizer.Get($"Common_States_Off"); // TODO

    public string PortForwardingFeatureState => Localizer.Get($"Common_States_Off"); // TODO

    public string SplitTunnelingFeatureState => Localizer.Get($"Common_States_Off"); // TODO

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

    public bool IsHardwareAccelerationEnabled
    {
        get => _settings.IsHardwareAccelerationEnabled;
        set => _settings.IsHardwareAccelerationEnabled = value;
    }

    public ObservableCollection<ApplicationElementTheme> Themes { get; }

    public ObservableCollection<string> Languages => _languages.Value;

    public override bool IsBackEnabled => false;

    public override IconElement Icon { get; } = new CogWheel();

    public override string? Title => Localizer.Get("Settings_Page_Title");

    public SettingsViewModel(IThemeSelector themeSelector,
        IMainViewNavigator viewNavigator,
        ILocalizationService localizationService,
        ILocalizationProvider localizationProvider,
        ISettings settings,
        ISettingsRestorer settingsRestorer,
        IUrls urls,
        IDialogActivator dialogActivator,
        IReportIssueViewNavigator reportIssueViewNavigator)
        : base(viewNavigator, localizationProvider)
    {
        _themeSelector = themeSelector;
        _localizationService = localizationService;
        _settings = settings;
        _settingsRestorer = settingsRestorer;
        _urls = urls;
        _dialogActivator = dialogActivator;
        _reportIssueViewNavigator = reportIssueViewNavigator;

        _languages = new Lazy<ObservableCollection<string>>(
            () => new ObservableCollection<string>(_localizationService.GetAvailableLanguages()));

        Themes = new ObservableCollection<ApplicationElementTheme>(_themeSelector.GetAvailableThemes());
    }

    [RelayCommand]
    public async Task RestoreDefaultSettingsAsync()
    {
        ContentDialogResult result = await ViewNavigator.ShowMessageAsync(
            new MessageDialogParameters
            {
                Title = Localizer.Get("Settings_RestoreDefault_Confirmation_Title"),
                Message = Localizer.Get("Settings_RestoreDefault_Confirmation_Message"),
                PrimaryButtonText = Localizer.Get("Settings_RestoreDefault_Confirmation_Action"),
                CloseButtonText = Localizer.Get("Common_Actions_Cancel"),
            });

        if (result == ContentDialogResult.Primary)
        {
            _settingsRestorer.Restore();
        }
    }

    [RelayCommand]
    public void OpenSupport()
    {
        _urls.NavigateTo(_urls.SupportCenter);
    }

    [RelayCommand]
    public void ReportIssue()
    {
        _dialogActivator.ShowDialog<ReportIssueShellViewModel>();

        _reportIssueViewNavigator.NavigateTo<CategorySelectionViewModel>();
    }

    public void Receive(SettingChangedMessage message)
    {
        switch (message.PropertyName)
        {
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

            case nameof(ISettings.IsHardwareAccelerationEnabled):
                OnPropertyChanged(nameof(IsHardwareAccelerationEnabled));
                break;
        }
    }

    public void Receive(ThemeChangedMessage message)
    {
        OnPropertyChanged(nameof(SelectedTheme));
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
}