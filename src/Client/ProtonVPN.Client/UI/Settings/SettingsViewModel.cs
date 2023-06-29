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
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Themes;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.OperatingSystems.Registries.Contracts;
using Windows.ApplicationModel;

namespace ProtonVPN.Client.UI.Settings;

public partial class SettingsViewModel : NavigationPageViewModelBase, IEventMessageReceiver<ThemeChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly IThemeSelector _themeSelector;
    private readonly ILocalizationService _localizationService;
    private readonly ISettings _settings;
    private readonly ISettingsRestorer _settingsRestorer;
    private readonly IRegistryEditor _registryEditor;
    private readonly Lazy<ObservableCollection<string>> _languages;
    private readonly RegistryUri _osVersionRegistryUri = new(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName");

    public ApplicationElementTheme SelectedTheme
    {
        get => _themeSelector.GetTheme();
        set => _themeSelector.SetTheme(value);
    }

    [ObservableProperty]
    private string _clientVersionDescription;

    [ObservableProperty]
    private string _operatingSystemVersionDescription;

    public string SelectedLanguage
    {
        get => _settings.Language;
        set => _settings.Language = value;
    }

    public string SelectedProtocol => Localizer.Get($"Settings_SelectedProtocol_{_settings.VpnProtocol}");

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
        ISettingsRestorer settingsRestorer,
        IRegistryEditor registryEditor)
        : base(pageNavigator, localizationProvider)
    {
        _themeSelector = themeSelector;
        _localizationService = localizationService;
        _settings = settings;
        _settingsRestorer = settingsRestorer;
        _registryEditor = registryEditor;

        _clientVersionDescription = GetClientVersionDescription();
        _operatingSystemVersionDescription = GetOperatingSystemVersionDescription();

        _languages = new Lazy<ObservableCollection<string>>(
            () => new ObservableCollection<string>(_localizationService.GetAvailableLanguages()));

        Themes = new ObservableCollection<ApplicationElementTheme>(_themeSelector.GetAvailableThemes());
    }

    private string GetClientVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            PackageVersion packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{App.APPLICATION_NAME} {version.Major}.{version.Minor}.{version.Build}";
    }

    private string GetOperatingSystemVersionDescription()
    {
        string? productName = _registryEditor.ReadString(_osVersionRegistryUri);
        return $"{productName} ({Environment.OSVersion.Version})";
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

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();
        OnPropertyChanged(nameof(SelectedLanguage));
        OnPropertyChanged(nameof(SelectedTheme));
        OnPropertyChanged(nameof(SelectedProtocol));
    }

    [RelayCommand]
    public void RestoreDefaultSettings()
    {
        _settingsRestorer.Restore();
    }
}