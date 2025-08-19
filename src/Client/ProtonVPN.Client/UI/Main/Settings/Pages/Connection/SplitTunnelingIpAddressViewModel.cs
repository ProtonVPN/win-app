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
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.UI.Main.Settings.Connection;

public partial class SplitTunnelingIpAddressViewModel : ViewModelBase, IEquatable<SplitTunnelingIpAddressViewModel>
{
    private readonly SplitTunnelingPageViewModel _parentViewModel;
    private readonly ISettings _settings;

    [ObservableProperty]
    private bool _isActive;

    public string IpAddress { get; }

    public bool IsInactiveDueToIpv6Disabled => !_settings.IsIpv6Enabled &&
                                               NetworkAddress.TryParse(IpAddress, out NetworkAddress address) &&
                                               address.IsIpV6;

    public SplitTunnelingIpAddressViewModel(
        IViewModelHelper viewModelHelper,
        ISettings settings,
        SplitTunnelingPageViewModel parentViewModel,
        string ipAddress)
        : this(viewModelHelper, settings, parentViewModel, ipAddress, true)
    { }

    public SplitTunnelingIpAddressViewModel(
        IViewModelHelper viewModelHelper,
        ISettings settings,
        SplitTunnelingPageViewModel parentViewModel,
        string ipAddress,
        bool isActive)
        : base(viewModelHelper)
    {
        _parentViewModel = parentViewModel;
        _settings = settings;

        _isActive = isActive;
        IpAddress = ipAddress;
    }

    [RelayCommand]
    public void RemoveIpAddress()
    {
        _parentViewModel.RemoveIpAddress(this);
    }

    [RelayCommand]
    public Task ShowIpv6DisabledWarning()
    {
        return _parentViewModel.TriggerIpv6DisabledWarningAsync();
    }

    public bool Equals(SplitTunnelingIpAddressViewModel? other)
    {
        return other != null
            && string.Equals(IpAddress, other.IpAddress, StringComparison.OrdinalIgnoreCase);
    }

    partial void OnIsActiveChanged(bool value)
    {
        _parentViewModel.InvalidateIpAddressesCount();
    }
}