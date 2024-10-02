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
using ProtonVPN.Client.Logic.Profiles.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Recents.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Models.Connections.Recents;

namespace ProtonVPN.Client.Factories;

public class ConnectionItemFactory : IConnectionItemFactory
{
    private readonly ILocalizationProvider _localizer;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IServersLoader _serversLoader;
    private readonly IConnectionManager _connectionManager;
    private readonly IRecentConnectionsManager _recentConnectionsManager;
    private readonly IProfilesManager _profilesManager;
    private readonly IUpsellCarouselWindowActivator _upsellCarouselWindowActivator;

    public ConnectionItemFactory(
        ILocalizationProvider localizer,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IRecentConnectionsManager recentConnectionsManager,
        IProfilesManager profilesManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator)
    {
        _localizer = localizer;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _serversLoader = serversLoader;
        _connectionManager = connectionManager;
        _recentConnectionsManager = recentConnectionsManager;
        _profilesManager = profilesManager;
        _upsellCarouselWindowActivator = upsellCarouselWindowActivator;
    }

    public RecentConnectionItem GetRecent(IRecentConnection recentConnection)
    {
        return new RecentConnectionItem(_localizer, _serversLoader, _connectionManager, _upsellCarouselWindowActivator, _recentConnectionsManager, recentConnection);
    }

    public void GetProfile(IConnectionProfile profile)
    {
        throw new NotImplementedException();
    }
}