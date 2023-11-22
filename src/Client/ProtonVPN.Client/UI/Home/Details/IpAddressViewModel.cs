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
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;

namespace ProtonVPN.Client.UI.Home.Details;

public partial class IpAddressViewModel : ViewModelBase, IRecipient<ConnectionDetailsChanged>
{
    private const string HIDDEN_IP_ADDRESS = "***.***.***.***";

    private string _hiddenUserIpAddress;

    [ObservableProperty]
    private string _userIpAddress;

    [ObservableProperty]
    private string _serverIpAddress;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ShowUserIPAddressCommand))]
    [NotifyCanExecuteChangedFor(nameof(HideUserIPAddressCommand))]
    private bool _isUserIpVisible;

    public IpAddressViewModel(ILocalizationProvider localizationProvider)
        : base(localizationProvider)
    {
        _userIpAddress = HIDDEN_IP_ADDRESS;
        _isUserIpVisible = false;
    }

    [RelayCommand(CanExecute = nameof(CanHideUserIPAddress))]
    public void HideUserIPAddress()
    {
        UserIpAddress = HIDDEN_IP_ADDRESS;
        IsUserIpVisible = false;
    }

    private bool CanHideUserIPAddress()
    {
        return IsUserIpVisible;
    }

    [RelayCommand(CanExecute = nameof(CanShowUserIPAddress))]
    public void ShowUserIPAddress()
    {
        UserIpAddress = _hiddenUserIpAddress;
        IsUserIpVisible = true;
    }

    private bool CanShowUserIPAddress()
    {
        return !IsUserIpVisible;
    }

    public void Receive(ConnectionDetailsChanged message)
    {
        _hiddenUserIpAddress = message.ClientIpAddress;
        ServerIpAddress = message.ServerIpAddress;
    }
}