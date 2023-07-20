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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Home.Details;

namespace ProtonVPN.Client.UI.Home;

public partial class HomeViewModel : NavigationPageViewModelBase, IRecipient<ConnectionStatusChanged>
{
    private readonly IConnectionManager _connectionManager;
    private readonly ConnectionDetailsViewModel _connectionDetailsViewModel;

    private bool _openDetailsPaneAutomatically;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDetailsPaneInline))]
    private bool _isDetailsPaneOpen;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDetailsPaneInline))]
    private SplitViewDisplayMode _detailsPaneDisplayMode;

    public bool IsDetailsPaneInline => IsDetailsPaneOpen &&
        (DetailsPaneDisplayMode is SplitViewDisplayMode.Inline or SplitViewDisplayMode.CompactInline);

    public override string? Title => Localizer.Get("Home_Page_Title");

    public override bool IsBackEnabled => false;

    public bool IsConnecting => _connectionManager.ConnectionStatus == ConnectionStatus.Connecting;

    public bool IsConnected => _connectionManager.ConnectionStatus == ConnectionStatus.Connected;

    public override IconElement Icon { get; } = new House();

    public HomeViewModel(IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        IConnectionManager connectionManager,
        ConnectionDetailsViewModel connectionDetailsViewModel)
        : base(viewNavigator, localizationProvider)
    {
        _connectionManager = connectionManager;
        _connectionDetailsViewModel = connectionDetailsViewModel;
    }

    [RelayCommand]
    public void CloseDetailsPane()
    {
        IsDetailsPaneOpen = false;
    }

    public void OpenDetailsPane()
    {
        IsDetailsPaneOpen = true;

        // Details Pane is opened, reset flag
        _openDetailsPaneAutomatically = false;
    }

    public void Receive(ConnectionStatusChanged message)
    {
        OnPropertyChanged(nameof(IsConnecting));
        OnPropertyChanged(nameof(IsConnected));

        switch (_connectionManager.ConnectionStatus)
        {
            case ConnectionStatus.Connecting:
            case ConnectionStatus.Connected:
                if (_openDetailsPaneAutomatically && !IsDetailsPaneOpen)
                {
                    OpenDetailsPane();
                }
                break;

            default:
                if (IsDetailsPaneOpen)
                {
                    CloseDetailsPane();

                    // When details pane was closed due to disconnection, set flag to reopen it automatically on Connect
                    _openDetailsPaneAutomatically = true;
                }
                break;
        }
    }

    public override void OnNavigatedFrom()
    {
        base.OnNavigatedFrom();

        _connectionDetailsViewModel.IsActive = false;
    }

    public override void OnNavigatedTo(object parameter)
    {
        base.OnNavigatedTo(parameter);

        _connectionDetailsViewModel.IsActive = IsDetailsPaneOpen;
    }

    partial void OnIsDetailsPaneOpenChanged(bool value)
    {
        _connectionDetailsViewModel.IsActive = value;
    }
}