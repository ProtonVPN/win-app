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

using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Dialogs.Overlays.Welcome;

namespace ProtonVPN.Client.Handlers;

public class WelcomeOverlayHandler : IHandler,
    IEventMessageReceiver<HomePageDisplayedAfterLoginMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IOverlayActivator _overlayActivator;
    private readonly IUserAuthenticator _userAuthenticator;

    public WelcomeOverlayHandler(ISettings settings, IOverlayActivator overlayActivator, IUserAuthenticator userAuthenticator)
    {
        _settings = settings;
        _overlayActivator = overlayActivator;
        _userAuthenticator = userAuthenticator;
    }

    public void Receive(HomePageDisplayedAfterLoginMessage message)
    {
        if (!_settings.VpnPlan.IsB2B && !_settings.WasWelcomeOverlayDisplayed)
        {
            _settings.WasWelcomeOverlayDisplayed = true;
            _overlayActivator.ShowOverlayAsync<WelcomeOverlayViewModel>();
        }
        else if (_settings.VpnPlan.IsB2B && !_settings.WasWelcomeB2BOverlayDisplayed)
        {
            _settings.WasWelcomeB2BOverlayDisplayed = true;
            _overlayActivator.ShowOverlayAsync<WelcomeToVpnB2BOverlayViewModel>();
        }
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        if (!_userAuthenticator.IsLoggedIn)
        {
            return;
        }

        if (_settings.VpnPlan.IsPlus && !_settings.WasWelcomePlusOverlayDisplayed)
        {
            _settings.WasWelcomePlusOverlayDisplayed = true;
            _overlayActivator.ShowOverlayAsync<WelcomeToVpnPlusOverlayViewModel>();
        }
        else if (_settings.VpnPlan.IsUnlimited && !_settings.WasWelcomeUnlimitedOverlayDisplayed)
        {
            _settings.WasWelcomeUnlimitedOverlayDisplayed = true;
            _overlayActivator.ShowOverlayAsync<WelcomeToVpnUnlimitedOverlayViewModel>();
        }
    }
}