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

using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.Models;

namespace ProtonVPN.Client.Models.Connections.Gateways;

public class GatewayLocationItem : HostLocationItemBase
{
    public string Gateway { get; }

    public override ConnectionGroupType GroupType => ConnectionGroupType.Gateways;

    public override string Header => Gateway;

    public override string? ToolTip =>
        IsRestricted
            ? Localizer.Get("Connections_Gateway_Restricted")
            : IsUnderMaintenance
                ? Localizer.Get("Connections_Gateway_UnderMaintenance")
                : null;

    public override ILocationIntent LocationIntent { get; }

    public override IFeatureIntent? FeatureIntent { get; } = new B2BFeatureIntent();

    public GatewayLocationItem(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainWindowOverlayActivator overlayActivator,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IConnectionGroupFactory connectionGroupFactory,
        ILocationItemFactory locationItemFactory,
        string gateway)
        : base(localizer,
               serversLoader,
               connectionManager,
               overlayActivator,
               upsellCarouselWindowActivator,
               connectionGroupFactory,
               locationItemFactory)
    {
        Gateway = gateway;

        LocationIntent = new GatewayLocationIntent(gateway);
    }

    public void OnExpandGateway()
    {
        FetchSubItems();
    }

    public void OnCollapseGateway()
    {
        ClearSubItems();
    }

    protected override bool MatchesActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        return currentConnectionDetails is not null
            && currentConnectionDetails.IsGateway
            && Gateway == currentConnectionDetails.GatewayName
            && (FeatureIntent?.GetType().IsAssignableTo(currentConnectionDetails.OriginalConnectionIntent.Feature?.GetType()) ?? true);
    }

    protected override IEnumerable<ConnectionItemBase> GetSubItems()
    {
        return ServersLoader.GetServersByGateway(Gateway)
                            .Select(LocationItemFactory.GetGatewayServer);
    }
}