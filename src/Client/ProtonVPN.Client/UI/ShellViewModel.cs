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
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Countries;
using ProtonVPN.Client.UI.Gallery;
using ProtonVPN.Client.UI.Gateways;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Client.UI.Settings;
using ProtonVPN.Common.Legacy.Extensions;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI;

public partial class ShellViewModel : ShellViewModelBase<IMainViewNavigator>, IEventMessageReceiver<LoggedInMessage>
{
    private readonly IEventMessageSender _eventMessageSender;
    private readonly ISettings _settings;

    [ObservableProperty]
    private NavigationPageViewModelBase? _selectedNavigationPage;

    [ObservableProperty]
    private bool _isNavigationPaneOpened;

    [ObservableProperty]
    private double _navigationPaneWidth;

    public override string Title => App.APPLICATION_NAME;

    public ObservableCollection<NavigationPageViewModelBase> NavigationPages { get; }

    public ShellViewModel(IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        IEventMessageSender eventMessageSender,
        HomeViewModel homeViewModel,
        GatewaysViewModel gatewaysViewModel,
        CountriesViewModel countriesViewModel,
        ISettings settings,
        ILogger logger,
        IIssueReporter issueReporter,
        SettingsViewModel settingsViewModel,
        Lazy<GalleryViewModel> galleryViewModel)
        : base(viewNavigator, localizationProvider, logger, issueReporter)
    {
        _eventMessageSender = eventMessageSender;
        _settings = settings;

        NavigationPages = new ObservableCollection<NavigationPageViewModelBase>
        {
            homeViewModel,
            gatewaysViewModel,
            countriesViewModel,
            settingsViewModel,
        };

        AddDebugPages(galleryViewModel);
    }

    public void OnNavigationDisplayModeChanged(NavigationViewDisplayMode displayMode)
    {
        _eventMessageSender.Send(new NavigationDisplayModeChangedMessage(displayMode));
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            IsNavigationPaneOpened = _settings.IsNavigationPaneOpened;
        });
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        NavigationPages.ForEach(p => p.InvalidateTitle());
    }

    protected override void OnNavigated()
    {
        base.OnNavigated();

        SelectedNavigationPage = CurrentPage as NavigationPageViewModelBase
                              ?? NavigationPages.FirstOrDefault(p => p.IsHostFor(CurrentPage));
    }

    private void AddDebugPages(Lazy<GalleryViewModel> galleryViewModel)
    {
        if (_settings.IsDebugModeEnabled)
        {
            NavigationPages.Add(galleryViewModel.Value);
        }
    }

    partial void OnIsNavigationPaneOpenedChanged(bool value)
    {
        _settings.IsNavigationPaneOpened = value;
    }

}