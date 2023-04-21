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
using Microsoft.UI.Xaml;

using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.Helpers;
using ProtonVPN.Gui.Models;
using ProtonVPN.Gui.ViewModels.Bases;
using Windows.ApplicationModel;

namespace ProtonVPN.Gui.ViewModels.Pages;

public partial class SettingsViewModel : PageViewModelBase
{
    private readonly IThemeSelectorService _themeSelectorService;
    private ApplicationElementTheme _selectedTheme;
    private string _versionDescription;

    public SettingsViewModel(IThemeSelectorService themeSelectorService, INavigationService navigationService)
        : base(navigationService, "Settings")
    {
        _themeSelectorService = themeSelectorService;

        _versionDescription = GetVersionDescription();

        Themes = new ObservableCollection<ApplicationElementTheme>()
        {
            new ApplicationElementTheme(ElementTheme.Light, "Settings_Theme_Light".GetLocalized()),
            new ApplicationElementTheme(ElementTheme.Dark, "Settings_Theme_Dark".GetLocalized()),
            new ApplicationElementTheme(ElementTheme.Default, "Settings_Theme_Default".GetLocalized())
        };

        _selectedTheme = Themes.FirstOrDefault(t => t.Theme == _themeSelectorService.Theme) ?? Themes.Last();
    }

    public ApplicationElementTheme SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (SetProperty(ref _selectedTheme, value))
            {
                _themeSelectorService.SetThemeAsync(value.Theme);
            }
        }
    }

    public ObservableCollection<ApplicationElementTheme> Themes { get; }

    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }

    private static string GetVersionDescription()
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

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}