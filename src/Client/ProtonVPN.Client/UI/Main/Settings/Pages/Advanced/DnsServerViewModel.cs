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
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;

namespace ProtonVPN.Client.UI.Main.Settings.Pages.Advanced;

public partial class DnsServerViewModel : ViewModelBase
{
    private readonly CustomDnsServersViewModel _parentViewModel;

    [ObservableProperty]
    private bool _isActive;

    public string IpAddress { get; }

    public DnsServerViewModel(
        CustomDnsServersViewModel parentViewModel,
        IViewModelHelper viewModelHelper,
        string ipAddress)
        : this(parentViewModel, viewModelHelper, ipAddress, true)
    { }

    public DnsServerViewModel(
        CustomDnsServersViewModel parentViewModel,
        IViewModelHelper viewModelHelper,
        string ipAddress,
        bool isActive)
        : base(viewModelHelper)
    {
        _parentViewModel = parentViewModel;

        _isActive = isActive;
        IpAddress = ipAddress;
    }

    [RelayCommand]
    public void RemoveDnsServer()
    {
        _parentViewModel.RemoveDnsServer(this);
    }

    partial void OnIsActiveChanged(bool value)
    {
        _parentViewModel.InvalidateCustomDnsServersCount();
    }
}