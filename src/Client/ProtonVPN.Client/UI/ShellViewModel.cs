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
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Countries;
using ProtonVPN.Client.UI.Gallery;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Client.UI.Settings;
using ProtonVPN.Common.Extensions;

namespace ProtonVPN.Client.UI;

public partial class ShellViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isBackEnabled;

    [ObservableProperty]
    private NavigationPageViewModelBase? _selectedNavigationPage;
    private readonly IEventMessageSender _eventMessageSender;

    public ShellViewModel(IPageNavigator pageNavigator,
        IViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        IEventMessageSender eventMessageSender,
        HomeViewModel homeViewModel,
        CountriesViewModel countriesViewModel,
        SettingsViewModel settingsViewModel,
        Lazy<GalleryViewModel> galleryViewModel)
        : base(localizationProvider)
    {
        _eventMessageSender = eventMessageSender;

        PageNavigator = pageNavigator;
        PageNavigator.Navigated += OnNavigated;
        ViewNavigator = viewNavigator;

        NavigationPages = new ObservableCollection<NavigationPageViewModelBase>
        {
            homeViewModel,
            countriesViewModel,
            settingsViewModel,
        };

        AddDebugPages(galleryViewModel);
    }

    public PageViewModelBase? CurrentPage
        => PageNavigator?.Frame?.GetPageViewModel() as PageViewModelBase;

    public IPageNavigator PageNavigator { get; }

    public IViewNavigator ViewNavigator { get; }

    public ObservableCollection<NavigationPageViewModelBase> NavigationPages { get; }

    protected override void OnLanguageChanged()
    {
        NavigationPages.ForEach(p => p.InvalidateTitle());
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = PageNavigator.CanGoBack;

        OnPropertyChanged(nameof(CurrentPage));

        SelectedNavigationPage = CurrentPage as NavigationPageViewModelBase ?? NavigationPages.FirstOrDefault(p => p.IsHostFor(CurrentPage));
    }

    [Conditional("DEBUG")]
    private void AddDebugPages(Lazy<GalleryViewModel> galleryViewModel)
    {
        NavigationPages.Add(galleryViewModel.Value);
    }

    public void OnNavigationDisplayModeChanged(NavigationViewDisplayMode displayMode)
    {
        _eventMessageSender.Send(new NavigationDisplayModeChangedMessage(displayMode));
    }
}