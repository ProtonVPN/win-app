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
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Countries;
using ProtonVPN.Client.UI.Gallery;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Client.UI.Login;
using ProtonVPN.Client.UI.Settings;
using ProtonVPN.Common.Legacy.Extensions;

namespace ProtonVPN.Client.UI;

public partial class ShellViewModel : ShellViewModelBase<IMainViewNavigator>
{
    private readonly IEventMessageSender _eventMessageSender;

    [ObservableProperty]
    private NavigationPageViewModelBase? _selectedNavigationPage;

    [ObservableProperty]
    private bool _isLoggedIn;

    public override string? Title => App.APPLICATION_NAME;

    public ObservableCollection<NavigationPageViewModelBase> NavigationPages { get; }

    public ShellViewModel(IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        IEventMessageSender eventMessageSender,
        HomeViewModel homeViewModel,
        CountriesViewModel countriesViewModel,
        SettingsViewModel settingsViewModel,
        Lazy<GalleryViewModel> galleryViewModel)
        : base(viewNavigator, localizationProvider)
    {
        _eventMessageSender = eventMessageSender;

        NavigationPages = new ObservableCollection<NavigationPageViewModelBase>
        {
            homeViewModel,
            countriesViewModel,
            settingsViewModel,
        };

        AddDebugPages(galleryViewModel);
    }

    public void OnNavigationDisplayModeChanged(NavigationViewDisplayMode displayMode)
    {
        _eventMessageSender.Send(new NavigationDisplayModeChangedMessage(displayMode));
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        NavigationPages.ForEach(p => p.InvalidateTitle());
    }

    protected override void OnNavigated(object sender, NavigationEventArgs e)
    {
        base.OnNavigated(sender, e);

        IsLoggedIn = CurrentPage is not LoginViewModel;

        SelectedNavigationPage = CurrentPage as NavigationPageViewModelBase
                              ?? NavigationPages.FirstOrDefault(p => p.IsHostFor(CurrentPage));
    }

    [Conditional("DEBUG")]
    private void AddDebugPages(Lazy<GalleryViewModel> galleryViewModel)
    {
        NavigationPages.Add(galleryViewModel.Value);
    }
}