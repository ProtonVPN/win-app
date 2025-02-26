/*
 * Copyright (c) 2024 Proton AG
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

using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;

namespace ProtonVPN.Client.UI.Overlays.Welcome;

public class WelcomeToVpnPlusOverlayViewModel : OverlayViewModelBase<IMainWindowOverlayActivator>,
    IEventMessageReceiver<ServerListChangedMessage>
{
    private readonly IServerCountCache _serverCountCache;

    public int TotalCountries => _serverCountCache.GetCountryCount();

    public WelcomeToVpnPlusOverlayViewModel(
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IServerCountCache serverCountCache,
        IViewModelHelper viewModelHelper)
        : base(mainWindowOverlayActivator, viewModelHelper)
    {
        _serverCountCache = serverCountCache;
    }

    public void Receive(ServerListChangedMessage message)
    {
        ExecuteOnUIThread(() => OnPropertyChanged(nameof(TotalCountries)));
    }
}