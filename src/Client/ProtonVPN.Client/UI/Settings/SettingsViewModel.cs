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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Contracts.Services;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models;
using Windows.ApplicationModel;

namespace ProtonVPN.Client.UI.Settings;

public partial class SettingsViewModel : NavigationPageViewModelBase
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILocalizationService _localizationService;

    private readonly Lazy<ObservableCollection<string>> _languages;

    [ObservableProperty]
    private ApplicationElementTheme _selectedTheme;

    private string? _selectedLanguage = null;
    private string _versionDescription;

    public string SelectedLanguage
    {
        get => _selectedLanguage ??= _localizationService.GetCurrentLanguage();
        set
        {
            if (SetProperty(ref _selectedLanguage, value))
            {
                _localizationService.SetLanguageAsync(value);
            }
        }
    }

    public override bool IsBackEnabled => false;

    public ObservableCollection<ApplicationElementTheme> Themes { get; }
    public ObservableCollection<string> Languages => _languages.Value;

    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }

    public override IconElement Icon { get; } = new CogWheel();

    public override string? Title => Localizer.Get("Settings_Page_Title");

    public SettingsViewModel(IThemeSelectorService themeSelectorService,
        INavigationService navigationService,
        ILocalizationService localizationService,
        ILocalizationProvider localizationProvider)
        : base(navigationService, localizationProvider)
    {
        _themeSelectorService = themeSelectorService;
        _localizationService = localizationService;

        _versionDescription = GetVersionDescription();

        Themes = new ObservableCollection<ApplicationElementTheme>()
        {
            new ApplicationElementTheme(ElementTheme.Light),
            new ApplicationElementTheme(ElementTheme.Dark),
            new ApplicationElementTheme(ElementTheme.Default)
        };

        _selectedTheme = Themes.FirstOrDefault(t => t.Theme == _themeSelectorService.Theme) ?? Themes.Last();

        _languages = new Lazy<ObservableCollection<string>>(
            () => new ObservableCollection<string>(_localizationService.GetAvailableLanguages()));
    }

    private string GetVersionDescription()
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

        return $"{App.APPLICATION_NAME} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    partial void OnSelectedThemeChanged(ApplicationElementTheme value)
    {
        _themeSelectorService.SetThemeAsync(value?.Theme ?? ElementTheme.Default);
    }
}