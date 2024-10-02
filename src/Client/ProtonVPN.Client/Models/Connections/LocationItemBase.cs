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
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Contracts.Services.Activation;

namespace ProtonVPN.Client.UI.Main.Sidebar.Connections.Bases.Models;

public abstract partial class LocationItemBase : ConnectionItemBase
{
    public override string SecondaryCommandAutomationId => $"Navigate_to_{AutomationName}";

    public abstract ILocationIntent LocationIntent { get; }

    public abstract IFeatureIntent? FeatureIntent { get; }

    protected LocationItemBase(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator)
        : base(localizer,
               serversLoader,
               connectionManager,
               upsellCarouselWindowActivator)
    { }

    public override IConnectionIntent GetConnectionIntent()
    {
        return new ConnectionIntent(LocationIntent, FeatureIntent);
    }
}
