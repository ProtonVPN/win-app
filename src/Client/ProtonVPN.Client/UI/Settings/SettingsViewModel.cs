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
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Themes;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;

namespace ProtonVPN.Client.UI.Settings;

public partial class SettingsViewModel : NavigationPageViewModelBase, IEventMessageReceiver<ThemeChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly IThemeSelector _themeSelector;
    private readonly ILocalizationService _localizationService;
    private readonly ISettings _settings;
    private readonly ISettingsRestorer _settingsRestorer;
    private readonly Lazy<ObservableCollection<string>> _languages;

    [ObservableProperty]
    private string _clientVersionDescription;

    [ObservableProperty]
    private string _operatingSystemVersionDescription;

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

    public string SelectedProtocol => Localizer.Get($"Settings_SelectedProtocol_{_settings.VpnProtocol}");

    public string NetShieldFeatureState => Localizer.Get($"Common_States_Off"); // TODO
    public string KillSwitchFeatureState => Localizer.Get($"Common_States_Off"); // TODO
    public string PortForwardingFeatureState => Localizer.Get($"Common_States_Off"); // TODO
    public string SplitTunnelingFeatureState => Localizer.Get($"Common_States_Off"); // TODO
    public string VpnAcceleratorSettingsState => Localizer.Get($"Common_States_Off"); // TODO

    public bool IsNotificationEnabled { get; set; }  // TODO
    public bool IsBetaAccessEnabled { get; set; }  // TODO
    public bool IsHardwareAccelerationEnabled { get; set; }  // TODO

    public ObservableCollection<ApplicationElementTheme> Themes { get; }
    public ObservableCollection<string> Languages => _languages.Value;

    public override bool IsBackEnabled => false;
    public override IconElement Icon { get; } = new CogWheel();
    public override string? Title => Localizer.Get("Settings_Page_Title");

    public SettingsViewModel(IThemeSelector themeSelector,
        IPageNavigator pageNavigator,
        ILocalizationService localizationService,
        ILocalizationProvider localizationProvider,
        ISettings settings,
        ISettingsRestorer settingsRestorer)
        : base(pageNavigator, localizationProvider)
    {
        _themeSelector = themeSelector;
        _localizationService = localizationService;
        _settings = settings;
        _settingsRestorer = settingsRestorer;

        _clientVersionDescription = GetClientVersionDescription();
        _operatingSystemVersionDescription = Environment.OSVersion.Version.ToString();

        _languages = new Lazy<ObservableCollection<string>>(
            () => new ObservableCollection<string>(_localizationService.GetAvailableLanguages()));

        Themes = new ObservableCollection<ApplicationElementTheme>(_themeSelector.GetAvailableThemes());
    }

    private string GetClientVersionDescription()
    {
        Version version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0,0,0,0);
        return $"{App.APPLICATION_NAME} {version.Major}.{version.Minor}.{version.Build}";
    }

    public void Receive(ThemeChangedMessage message)
    {
        OnPropertyChanged(nameof(SelectedTheme));
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.VpnProtocol))
        {
            OnPropertyChanged(nameof(SelectedProtocol));
        }
    }

    [RelayCommand]
    public void RestoreDefaultSettings()
    {
        _settingsRestorer.Restore();
    }

    [RelayCommand]
    public void OpenSupport()
    {
        // TODO
    }

    [RelayCommand]
    public void ReportIssue()
    {
        // TODO
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();
        OnPropertyChanged(nameof(SelectedLanguage));
        OnPropertyChanged(nameof(SelectedTheme));
        OnPropertyChanged(nameof(SelectedProtocol));
    }
}