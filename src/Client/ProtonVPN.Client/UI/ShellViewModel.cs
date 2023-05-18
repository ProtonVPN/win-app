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
using ProtonVPN.Common.Extensions;
using ProtonVPN.Client.Contracts.Services;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.UI.Countries;
using ProtonVPN.Client.UI.Gallery;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Client.UI.Settings;

namespace ProtonVPN.Client.UI;

public partial class ShellViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isBackEnabled;

    [ObservableProperty]
    private NavigationPageViewModelBase? _selectedNavigationPage;

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;

        NavigationPages = new ObservableCollection<NavigationPageViewModelBase>
        {
            App.GetService<HomeViewModel>(),
            App.GetService<CountriesViewModel>(),
            App.GetService<SettingsViewModel>(),
        };

        AddDebugPages();

        IsActive = true;
    }

    public PageViewModelBase? CurrentPage
        => NavigationService?.Frame?.GetPageViewModel() as PageViewModelBase;

    public INavigationService NavigationService { get; }

    public INavigationViewService NavigationViewService { get; }

    public ObservableCollection<NavigationPageViewModelBase> NavigationPages { get; }

    protected override void OnLanguageChanged()
    {
        NavigationPages.ForEach(p => p.InvalidateTitle());
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        OnPropertyChanged(nameof(CurrentPage));

        SelectedNavigationPage = CurrentPage as NavigationPageViewModelBase ?? NavigationPages.FirstOrDefault(p => p.IsHostFor(CurrentPage));
    }

    [Conditional("DEBUG")]
    private void AddDebugPages()
    {
        NavigationPages.Add(App.GetService<GalleryViewModel>());
    }
}