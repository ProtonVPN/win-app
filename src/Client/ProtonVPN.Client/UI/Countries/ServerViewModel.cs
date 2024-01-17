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
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Common.Core.Extensions;
using WinUI3Localizer;

namespace ProtonVPN.Client.UI.Countries;

public class ServerViewModel : ServerViewModelBase, ISearchableItem
{
    public string EntryCountryCode { get; private set; }
    public string EntryCountryName => Localizer.GetFormat("Countries_ViaCountry", Localizer.GetCountryName(EntryCountryCode));
    public string City { get; private set; }
    public bool IsVirtual { get; private set; }
    public bool IsSecureCore { get; private set; }
    public bool SupportsP2P { get; private set; }
    public bool IsTor { get; private set; }

    public override string ConnectButtonAutomationName => IsSecureCore
        ? $"{EntryCountryName} {ExitCountryName}"
        : ExitCountryName;

    protected override ConnectionIntent ConnectionIntent =>
        new(new ServerLocationIntent(Id, Name, ExitCountryCode, City),
            GetFeatureIntent());

    public ServerViewModel(
        ILocalizationProvider localizationProvider,
        IMainViewNavigator mainViewNavigator,
        IConnectionManager connectionManager) 
        : base(localizationProvider, mainViewNavigator, connectionManager)
    { }

    public bool MatchesSearchQuery(string query)
    {
        return Name.ContainsIgnoringCase(query);
    }

    private IFeatureIntent? GetFeatureIntent()
    {
        if (IsSecureCore)
        {
            return new SecureCoreFeatureIntent(EntryCountryCode);
        }

        if (SupportsP2P)
        {
            return new P2PFeatureIntent();
        }

        if (IsTor)
        {
            return new TorFeatureIntent();
        }

        return null;
    }

    public override void CopyPropertiesFromServer(Server server)
    {
        base.CopyPropertiesFromServer(server);

        City = server.City;
        IsVirtual = server.IsVirtual;
        IsSecureCore = server.Features.IsSupported(ServerFeatures.SecureCore);
        SupportsP2P = server.Features.IsSupported(ServerFeatures.P2P);
        IsTor = server.Features.IsSupported(ServerFeatures.Tor);
        EntryCountryCode = server.EntryCountry;
    }
}