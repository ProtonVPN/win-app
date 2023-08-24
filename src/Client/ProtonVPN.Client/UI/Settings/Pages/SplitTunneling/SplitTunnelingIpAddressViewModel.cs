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
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;

namespace ProtonVPN.Client.UI.Settings.Pages.SplitTunneling;

public partial class SplitTunnelingIpAddressViewModel : ViewModelBase, IEquatable<SplitTunnelingIpAddressViewModel>
{
    private readonly SplitTunnelingViewModel _parentViewModel;

    [ObservableProperty]
    private bool _isActive;

    public string IpAddress { get; }

    public SplitTunnelingIpAddressViewModel(ILocalizationProvider localizationProvider, SplitTunnelingViewModel parentViewModel, string ipAddress)
        : this(localizationProvider, parentViewModel, ipAddress, true)
    { }

    public SplitTunnelingIpAddressViewModel(ILocalizationProvider localizationProvider, SplitTunnelingViewModel parentViewModel, string ipAddress, bool isActive)
        : base(localizationProvider)
    {
        _parentViewModel = parentViewModel;

        _isActive = isActive;
        IpAddress = ipAddress;
    }

    [RelayCommand]
    public void RemoveIpAddress()
    {
        _parentViewModel.RemoveIpAddress(this);
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