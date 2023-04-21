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
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Gui.Enums;
using ProtonVPN.Gui.Messages;

namespace ProtonVPN.Gui.ViewModels.Components;

public partial class VpnStatusViewModel : ObservableRecipient, IRecipient<VpnStateChangedMessage>, IRecipient<UserLocationChangedMessage>
{
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

    public VpnStatusViewModel()
    {
        Messenger.RegisterAll(this);

        _userCountry = "Lithuania";
        _userIpAddress = "158.6.140.191";
        _currentConnectionStatus = ConnectionStatus.Disconnected;
        _isNetShieldStatEnabled = true;
        _isSecureCoreConnection = true;
    }

    public bool IsConnected => CurrentConnectionStatus == ConnectionStatus.Connected;

    public bool IsConnecting => CurrentConnectionStatus == ConnectionStatus.Connecting;

    public bool IsDisconnected => CurrentConnectionStatus == ConnectionStatus.Disconnected;

    public bool IsNetShieldStatVisible => IsConnected && IsNetShieldStatEnabled;

    public bool IsTitleVisible => !string.IsNullOrEmpty(Title);

    public string Subtitle =>
        CurrentConnectionStatus switch
        {
            ConnectionStatus.Connected => "Browse the Internet safely",
            ConnectionStatus.Connecting => "Protecting your digital identity",
            ConnectionStatus.Disconnected => "You're unprotected",
            _ => string.Empty
        };

    public string Title =>
        CurrentConnectionStatus switch
        {
            ConnectionStatus.Connected => "Protected",
            ConnectionStatus.Connecting => string.Empty,
            ConnectionStatus.Disconnected => string.Empty,
            _ => string.Empty
        };

    public void Receive(VpnStateChangedMessage message)
    {
        if (message?.Value is null)
        {
            return;
        }

        switch (message.Value.Status)
        {
            case VpnStatus.Disconnected:
            case VpnStatus.Disconnecting:
            case VpnStatus.ActionRequired:
                CurrentConnectionStatus = ConnectionStatus.Disconnected;
                break;

            case VpnStatus.Pinging:
            case VpnStatus.Connecting:
            case VpnStatus.Reconnecting:
            case VpnStatus.Waiting:
            case VpnStatus.Authenticating:
            case VpnStatus.RetrievingConfiguration:
            case VpnStatus.AssigningIp:
                CurrentConnectionStatus = ConnectionStatus.Connecting;
                break;

            case VpnStatus.Connected:
                CurrentConnectionStatus = ConnectionStatus.Connected;
                break;
        }

        IsSecureCoreConnection = message.Value.Server?.IsSecureCore() ?? false;
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
}