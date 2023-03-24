using System.Collections.ObjectModel;
using System.Reflection;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    public ObservableCollection<ApplicationElementTheme> Themes
    {
        get;
    }

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
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}