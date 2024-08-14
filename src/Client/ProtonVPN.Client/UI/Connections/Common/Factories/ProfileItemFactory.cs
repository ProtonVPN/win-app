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
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Activation.Custom;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Connections.Common.Items;
using ProtonVPN.Client.UI.Connections.Profiles;

namespace ProtonVPN.Client.UI.Connections.Common.Factories;

public class ProfileItemFactory
{
    private readonly ILocalizationProvider _localizer;
    private readonly IServersLoader _serversLoader;
    private readonly IConnectionManager _connectionManager;
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly IUpsellCarouselDialogActivator _upsellCarouselActivator;
    private readonly IOverlayActivator _overlayActivator;
    private readonly IProfilesManager _profilesManager;
    private readonly IProfileEditor _profileEditor;
    private readonly ISettings _settings;

    public ProfileItemFactory(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainViewNavigator mainViewNavigator,
        IUpsellCarouselDialogActivator upsellCarouselActivator,
        IOverlayActivator overlayActivator,
        IProfilesManager profilesManager,
        IProfileEditor profileEditor,
        ISettings settings)
    {
        _localizer = localizer;
        _serversLoader = serversLoader;
        _connectionManager = connectionManager;
        _mainViewNavigator = mainViewNavigator;
        _upsellCarouselActivator = upsellCarouselActivator;
        _overlayActivator = overlayActivator;
        _profilesManager = profilesManager;
        _profileEditor = profileEditor;
        _settings = settings;
    }

    public ProfileItem GetProfile(IConnectionProfile profile)
    {
        return new ProfileItem(_localizer, _serversLoader, _connectionManager, _mainViewNavigator, _upsellCarouselActivator, _overlayActivator, _profilesManager, _profileEditor, _settings, profile);
    }
}