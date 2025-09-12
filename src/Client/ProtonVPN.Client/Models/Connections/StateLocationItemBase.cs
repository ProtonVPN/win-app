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

using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.States;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;

namespace ProtonVPN.Client.Models.Connections;

public abstract class StateLocationItemBase : HostLocationItemBase<State>
{
    public State State { get; }

    public override string Header => State.Name;

    public override string Description => Localizer.GetCountryName(State.CountryCode);

    public override string? ToolTip =>
        IsRestricted
            ? Localizer.Get("Connections_State_Restricted")
            : IsUnderMaintenance
                ? Localizer.Get("Connections_State_UnderMaintenance")
                : null;

    public override ILocationIntent LocationIntent { get; }

    public override VpnTriggerDimension VpnTriggerDimension => IsSearchItem
        ? VpnTriggerDimension.SearchState
        : VpnTriggerDimension.CountriesState;

    protected StateLocationItemBase(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainWindowOverlayActivator overlayActivator,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IConnectionGroupFactory connectionGroupFactory,
        ILocationItemFactory locationItemFactory,
        State state,
        bool showBaseLocation,
        bool isSearchItem)
        : base(localizer,
               serversLoader,
               connectionManager,
               overlayActivator,
               upsellCarouselWindowActivator,
               connectionGroupFactory,
               locationItemFactory,
               state,
               isSearchItem)
    {
        State = state;
        IsDescriptionVisible = showBaseLocation;

        LocationIntent = SingleStateLocationIntent.From(State.CountryCode, State.Name);
    }

    public void OnExpandState()
    {
        FetchSubItems();
    }

    public void OnCollapseState()
    {
        ClearSubItems();
    }

    protected override bool MatchesActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        return currentConnectionDetails is not null
            && !currentConnectionDetails.IsGateway
            && State.CountryCode == currentConnectionDetails.ExitCountryCode
            && State.Name == currentConnectionDetails.State
            && (FeatureIntent?.GetType().IsAssignableTo(currentConnectionDetails.OriginalConnectionIntent.Feature?.GetType()) ?? true);
    }
}