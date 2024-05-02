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
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Activation.Custom;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Connections.Common.Enums;
using ProtonVPN.Client.UI.Connections.Common.Items;

namespace ProtonVPN.Client.UI.Connections.Common.Factories;

public class LocationItemFactory
{
    private readonly ILocalizationProvider _localizer;
    private readonly IOverlayActivator _overlayActivator;
    private readonly IServersLoader _serversLoader;
    private readonly IConnectionManager _connectionManager;
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly IUpsellCarouselDialogActivator _upsellCarouselActivator;

    public LocationItemFactory(
        ILocalizationProvider localizer,
        IOverlayActivator overlayActivator,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainViewNavigator mainViewNavigator,
        IUpsellCarouselDialogActivator upsellCarouselActivator)
    {
        _localizer = localizer;
        _overlayActivator = overlayActivator;
        _serversLoader = serversLoader;
        _connectionManager = connectionManager;
        _mainViewNavigator = mainViewNavigator;
        _upsellCarouselActivator = upsellCarouselActivator;
    }

    public LocationGroup GetGroup(GroupLocationType groupType, IEnumerable<LocationItemBase> items)
    {
        return new LocationGroup(_localizer, _overlayActivator, groupType, items);
    }

    public CountryLocationItem GetCountry(string exitCountryCode)
    {
        return new CountryLocationItem(_localizer, _serversLoader, _connectionManager, _mainViewNavigator, _upsellCarouselActivator, this, exitCountryCode);
    }

    public CityLocationItem GetCity(City city)
    {
        return new CityLocationItem(_localizer, _serversLoader, _connectionManager, _mainViewNavigator, _upsellCarouselActivator, this, city);
    }

    public ServerLocationItem GetServer(Server server)
    {
        return new ServerLocationItem(_localizer, _serversLoader, _connectionManager, _mainViewNavigator, _upsellCarouselActivator, this, server);
    }

    public SecureCoreCountryLocationItem GetSecureCoreCountry(string exitCountryCode)
    {
        return new SecureCoreCountryLocationItem(_localizer, _serversLoader, _connectionManager, _mainViewNavigator, _upsellCarouselActivator, this, exitCountryCode);
    }

    public SecureCoreCountryPairLocationItem GetSecureCoreCountryPair(SecureCoreCountryPair countryPair)
    {
        return new SecureCoreCountryPairLocationItem(_localizer, _serversLoader, _connectionManager, _mainViewNavigator, _upsellCarouselActivator, this, countryPair);
    }

    public SecureCoreServerLocationItem GetSecureCoreServer(Server server)
    {
        return new SecureCoreServerLocationItem(_localizer, _serversLoader, _connectionManager, _mainViewNavigator, _upsellCarouselActivator, this, server);
    }

    public P2PCountryLocationItem GetP2PCountry(string exitCountryCode)
    {
        return new P2PCountryLocationItem(_localizer, _serversLoader, _connectionManager, _mainViewNavigator, _upsellCarouselActivator, this, exitCountryCode);
    }

    public P2PCityLocationItem GetP2PCity(City city)
    {
        return new P2PCityLocationItem(_localizer, _serversLoader, _connectionManager, _mainViewNavigator, _upsellCarouselActivator, this, city);
    }

    public P2PServerLocationItem GetP2PServer(Server server)
    {
        return new P2PServerLocationItem(_localizer, _serversLoader, _connectionManager, _mainViewNavigator, _upsellCarouselActivator, this, server);
    }

    public TorCountryLocationItem GetTorCountry(string exitCountryCode)
    {
        return new TorCountryLocationItem(_localizer, _serversLoader, _connectionManager, _mainViewNavigator, _upsellCarouselActivator, this, exitCountryCode);
    }

    public TorServerLocationItem GetTorServer(Server server)
    {
        return new TorServerLocationItem(_localizer, _serversLoader, _connectionManager, _mainViewNavigator, _upsellCarouselActivator, this, server);
    }

    public GatewayLocationItem GetGateway(string gateway)
    {
        return new GatewayLocationItem(_localizer, _serversLoader, _connectionManager, _mainViewNavigator, _upsellCarouselActivator, this, gateway);
    }

    public GatewayServerLocationItem GetGatewayServer(Server server)
    {
        return new GatewayServerLocationItem(_localizer, _serversLoader, _connectionManager, _mainViewNavigator, _upsellCarouselActivator, this, server);
    }
}