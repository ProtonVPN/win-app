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

using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.UI.Connections.Common.Enums;
using ProtonVPN.Client.UI.Connections.Common.Extensions;
using ProtonVPN.Client.UI.Dialogs.Overlays;

namespace ProtonVPN.Client.UI.Connections.Common.Items;

public partial class LocationGroup : List<LocationItemBase>
{
    private readonly IOverlayActivator _overlayActivator;

    public ILocalizationProvider Localizer { get; }

    public GroupLocationType GroupType { get; }

    public int ItemsCount => this.Count(item => item.IsCounted);

    public string Header => Localizer.GetPluralFormat(
        GroupType switch
        {
            GroupLocationType.Countries => "Connections_Countries",
            GroupLocationType.Cities => "Connections_Cities",
            GroupLocationType.Servers => "Connections_Servers",
            GroupLocationType.SecureCoreCountries => "Connections_SecureCore_Countries",
            GroupLocationType.SecureCoreCountryPairs or
            GroupLocationType.SecureCoreServers => "Connections_SecureCore_Servers",
            GroupLocationType.P2PCountries => "Connections_P2P_Countries",
            GroupLocationType.P2PCities => "Connections_P2P_Cities",
            GroupLocationType.P2PServers => "Connections_P2P_Servers",
            GroupLocationType.TorCountries => "Connections_Tor_Countries",
            GroupLocationType.TorServers => "Connections_Tor_Servers",
            GroupLocationType.Gateways => "Connections_Gateways",
            GroupLocationType.GatewayServers => "Connections_Gateways_Servers",
            _ => throw new NotSupportedException($"Group type '{GroupType}' is not supported.")
        }, ItemsCount);

    public virtual bool IsInfoButtonVisible => GroupType.IsInfoButtonVisible();

    public virtual bool IsServerLoadInfoButtonVisible => !IsInfoButtonVisible && GroupType.IsServerLoadInfoButtonVisible();

    public LocationGroup(
        ILocalizationProvider localizer,
        IOverlayActivator overlayActivator,
        GroupLocationType groupType,
        IEnumerable<LocationItemBase> items)
        : base(items)
    {
        Localizer = localizer;
        _overlayActivator = overlayActivator;

        GroupType = groupType;
    }

    [RelayCommand(CanExecute = nameof(CanShowInfoOverlay))]
    protected async Task ShowInfoOverlayAsync()
    {
        switch (GroupType)
        {
            case GroupLocationType.SecureCoreCountries:
            case GroupLocationType.SecureCoreCountryPairs:
                await _overlayActivator.ShowOverlayAsync<SecureCoreOverlayViewModel>();
                break;

            case GroupLocationType.P2PCountries:
            case GroupLocationType.P2PCities:
            case GroupLocationType.P2PServers:
                await _overlayActivator.ShowOverlayAsync<P2POverlayViewModel>();
                break;

            case GroupLocationType.TorCountries:
            case GroupLocationType.TorServers:
                await _overlayActivator.ShowOverlayAsync<TorOverlayViewModel>();
                break;

            default:
                // do nothing
                break;
        }
    }

    protected bool CanShowInfoOverlay()
    {
        return IsInfoButtonVisible;
    }

    [RelayCommand(CanExecute = nameof(CanShowServerLoadInfoOverlay))]
    protected async Task ShowServerLoadInfoOverlayAsync()
    {
        await _overlayActivator.ShowOverlayAsync<ServerLoadOverlayViewModel>();
    }

    protected bool CanShowServerLoadInfoOverlay()
    {
        return IsServerLoadInfoButtonVisible;
    }
}