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
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Legacy.Models.Activation.Custom;
using ProtonVPN.Client.Legacy.Models.Navigation;
using ProtonVPN.Client.Legacy.UI.Connections.Common.Factories;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.Legacy.UI.Connections.Common.Items;

public abstract partial class ServerLocationItemBase : LocationItemBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LoadPercent))]
    private double _load;

    public Server Server { get; }

    public string LoadPercent => $"{Load:P0}";

    public override object FirstSortProperty => IsUnderMaintenance;

    public override object SecondSortProperty => Load;

    public override string? ToolTip =>
        IsRestricted
            ? Localizer.Get("Connections_Server_Restricted")
            : IsUnderMaintenance
                ? Localizer.Get("Connections_Server_UnderMaintenance")
                : null;

    public bool IsVirtual => Server.IsVirtual;

    public bool IsFree => Server.Tier == ServerTiers.Free;

    public bool SupportsP2P => Server.Features.IsSupported(ServerFeatures.P2P);

    public bool SupportsTor => Server.Features.IsSupported(ServerFeatures.Tor);

    public override ILocationIntent LocationIntent => new ServerLocationIntent(Server.Id, Server.Name, Server.ExitCountry, Server.State, Server.City);

    protected ServerLocationItemBase(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainViewNavigator mainViewNavigator,
        IUpsellCarouselDialogActivator upsellCarouselActivator,
        LocationItemFactory locationItemFactory,
        Server server)
        : base(localizer,
               serversLoader,
               connectionManager,
               mainViewNavigator,
               upsellCarouselActivator,
               locationItemFactory,
               server.Name)
    {
        Server = server;

        Load = server.Load / 100d;

        InvalidateIsUnderMaintenance();
    }

    public override bool MatchesSearchQuery(string searchQuery)
    {
        return IsFree
            ? !string.IsNullOrWhiteSpace(searchQuery) && Header.ContainsIgnoringCase(searchQuery)
            : base.MatchesSearchQuery(searchQuery);
    }

    public override void InvalidateIsUnderMaintenance()
    {
        IsUnderMaintenance = Server.IsLocationUnderMaintenance();
    }

    public override void InvalidateIsActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        IsActiveConnection = currentConnectionDetails is not null
                          && Server.Id == currentConnectionDetails.ServerId;
    }
}