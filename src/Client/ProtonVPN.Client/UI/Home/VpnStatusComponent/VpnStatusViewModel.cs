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
using CommunityToolkit.Mvvm.Messaging;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Messages;

namespace ProtonVPN.Client.UI.Home.VpnStatusComponent;

public partial class VpnStatusViewModel : ViewModelBase, IRecipient<ConnectionStatusChanged>, IRecipient<UserLocationChangedMessage>
{
    private readonly IConnectionService _connectionService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDisconnected))]
    [NotifyPropertyChangedFor(nameof(IsConnecting))]
    [NotifyPropertyChangedFor(nameof(IsConnected))]
    [NotifyPropertyChangedFor(nameof(IsNetShieldStatVisible))]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(IsTitleVisible))]
    [NotifyPropertyChangedFor(nameof(Subtitle))]
    private ConnectionStatus _currentConnectionStatus;

    [ObservableProperty]
    private string? _userCountry;

    [ObservableProperty]
    private string? _userIpAddress;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNetShieldStatVisible))]
    private bool _isNetShieldStatEnabled;

    [ObservableProperty]
    private bool _isSecureCoreConnection;

    public bool IsConnected => CurrentConnectionStatus == ConnectionStatus.Connected;

    public bool IsConnecting => CurrentConnectionStatus == ConnectionStatus.Connecting;

    public bool IsDisconnected => CurrentConnectionStatus == ConnectionStatus.Disconnected;

    public bool IsNetShieldStatVisible => IsConnected && IsNetShieldStatEnabled;

    public bool IsTitleVisible => !string.IsNullOrEmpty(Title);

    public string Subtitle =>
        CurrentConnectionStatus switch
        {
            ConnectionStatus.Connected or
            ConnectionStatus.Connecting => Localizer.Get("Home_VpnStatus_Subtitle_Connecting"),
            ConnectionStatus.Disconnected => Localizer.Get("Home_VpnStatus_Subtitle_Disconnected"),
            _ => string.Empty
        };

    public string Title =>
        CurrentConnectionStatus switch
        {
            ConnectionStatus.Connected => Localizer.Get("Home_VpnStatus_Title_Connected"),
            ConnectionStatus.Connecting or
            ConnectionStatus.Disconnected => string.Empty,
            _ => string.Empty
        };

    public VpnStatusViewModel(IConnectionService connectionService)
    {
        _connectionService = connectionService;

        Messenger.RegisterAll(this);

        _userCountry = "Lithuania";
        _userIpAddress = "158.6.140.191";
        _currentConnectionStatus = ConnectionStatus.Disconnected;
        _isNetShieldStatEnabled = true;
        _isSecureCoreConnection = true;
    }

    public void Receive(ConnectionStatusChanged message)
    {
        if (message?.Value is null)
        {
            return;
        }

        CurrentConnectionStatus = message.Value;

        if (CurrentConnectionStatus != ConnectionStatus.Connected)
        {
            IsSecureCoreConnection = false;
            return;
        }

        ConnectionDetails? connectionDetails = _connectionService.GetConnectionDetails();

        IsSecureCoreConnection = connectionDetails?.OriginalConnectionIntent?.Feature is SecureCoreFeatureIntent;
    }

    public void Receive(UserLocationChangedMessage message)
    {
        if (message?.Value is null)
        {
            return;
        }

        UserCountry = message.Value.Country;
        UserIpAddress = message.Value.Ip;
    }

    protected override void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Subtitle));
    }
}