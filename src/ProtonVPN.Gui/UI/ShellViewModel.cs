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
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Common.UI.Gallery;
using ProtonVPN.Common.UI.Gallery.Pages;
using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.Contracts.ViewModels;
using ProtonVPN.Gui.Helpers;
using ProtonVPN.Gui.Models;
using ProtonVPN.Gui.UI.Countries;
using ProtonVPN.Gui.UI.Home;
using ProtonVPN.Gui.UI.Settings;

namespace ProtonVPN.Gui.UI;

public partial class ShellViewModel : ObservableRecipient
{
    [ObservableProperty]
    private bool _isBackEnabled;

    [ObservableProperty]
    private NavigationPage _selectedPage;

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;

        Pages = new ObservableCollection<NavigationPage>
        {
            new NavigationPage("Home", "E80F", typeof(HomeViewModel)),
            new NavigationPage("Countries", "E909", typeof(CountriesViewModel)),
            new NavigationPage("Settings", "E713", typeof(SettingsViewModel))
        };

        AddDebugPages();

        _selectedPage = Pages.First();
    }

    public PageViewModelBase? CurrentPage => NavigationService?.Frame?.GetPageViewModel() as PageViewModelBase;

    public INavigationService NavigationService { get; }

    public INavigationViewService NavigationViewService { get; }

    public ObservableCollection<NavigationPage> Pages { get; }

    [Conditional("DEBUG")]
    private void AddDebugPages()
    {
        NavigationPage galleryPage = new("Gallery", "EB3C", typeof(GalleryPage));
        galleryPage.Children.Add(
            new NavigationPage("Colors", "", typeof(ColorsPage)));
        galleryPage.Children.Add(
            new NavigationPage("Typography", "", typeof(TypographyPage)));
        galleryPage.Children.Add(
            new NavigationPage("Inputs", "", typeof(InputsPage)));
        galleryPage.Children.Add(
            new NavigationPage("Text Fields", "", typeof(TextFieldsPage)));
        galleryPage.Children.Add(
            new NavigationPage("Map", "", typeof(MapPage)));
        galleryPage.Children.Add(
            new NavigationPage("VPN Specific", "", typeof(VpnSpecificPage)));

        Pages.Add(galleryPage);
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (CurrentPage != null)
        {
            SelectedPage = Pages.FirstOrDefault(p => p.PageKeyType == CurrentPage.GetType());
        }

        OnPropertyChanged(nameof(CurrentPage));
    }
}