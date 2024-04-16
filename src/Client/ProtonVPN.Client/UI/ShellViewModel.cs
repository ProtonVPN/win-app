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
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Sidebar;
using ProtonVPN.Client.UI.Sidebar.Bases;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI;

public partial class ShellViewModel : ShellViewModelBase<IMainViewNavigator>, IEventMessageReceiver<LoggedInMessage>
{
    private readonly IEventMessageSender _eventMessageSender;
    private readonly ISettings _settings;

    [ObservableProperty]
    private SidebarItemViewModelBase? _selectedMenuItem;

    [ObservableProperty]
    private double _navigationPaneWidth;

    public bool IsNavigationPaneOpened
    {
        get => _settings.IsNavigationPaneOpened;
        set => _settings.IsNavigationPaneOpened = value;
    }

    public override string Title => App.APPLICATION_NAME;

    public ObservableCollection<SidebarItemViewModelBase> MenuItems { get; }

    public ObservableCollection<SidebarItemViewModelBase> FooterMenuItems { get; }

    public ShellViewModel(
        IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        IEventMessageSender eventMessageSender,
        ISettings settings,
        ILogger logger,
        IIssueReporter issueReporter,
        SidebarHomeViewModel sidebarHome,
        SidebarGatewaysHeaderViewModel sidebarGatewaysHeader,
        SidebarGatewaysViewModel sidebarGateways,
        SidebarConnectionsHeaderViewModel sidebarConnectionsHeader,
        SidebarCountriesViewModel sidebarCountries,
        SidebarFeaturesHeaderViewModel sidebarFeaturesHeader,
        SidebarNetShieldViewModel sidebarNetShield,
        SidebarKillSwitchViewModel sidebarKillSwitch,
        SidebarPortForwardingViewModel sidebarPortForwarding,
        SidebarSplitTunnelingViewModel sidebarSplitTunneling,
        SidebarGalleryViewModel sidebarGallery,
        SidebarSettingsViewModel sidebarSettings,
        SidebarSeparatorViewModel sidebarSeparator,
        SidebarAccountViewModel sidebarAccount)
        : base(viewNavigator, localizationProvider, logger, issueReporter)
    {
        _eventMessageSender = eventMessageSender;
        _settings = settings;

        MenuItems = new()
        {
            sidebarHome,
            sidebarGatewaysHeader,
            sidebarGateways,
            sidebarConnectionsHeader,
            sidebarCountries,
            sidebarFeaturesHeader,
            sidebarNetShield,
            sidebarKillSwitch,
            sidebarPortForwarding,
            sidebarSplitTunneling
        };

        FooterMenuItems = new()
        {
            sidebarGallery,
            sidebarSettings,
            sidebarSeparator,
            sidebarAccount
        };
    }

    public void OnNavigationDisplayModeChanged(NavigationViewDisplayMode displayMode)
    {
        _eventMessageSender.Send(new NavigationDisplayModeChangedMessage(displayMode));
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(() => OnPropertyChanged(nameof(IsNavigationPaneOpened)));
    }

    protected override void OnNavigated()
    {
        base.OnNavigated();

        SelectedMenuItem =
            MenuItems.OfType<SidebarNavigationItemViewModelBase>().FirstOrDefault(p => p.IsHostFor(CurrentPage)) ??
            FooterMenuItems.OfType<SidebarNavigationItemViewModelBase>().FirstOrDefault(p => p.IsHostFor(CurrentPage));
    }
}