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

using ProtonVPN.Api.Contracts.HumanVerification;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.UI.Login.Overlays;

namespace ProtonVPN.Client.HumanVerification;

public class HumanVerifier : IHumanVerifier, IEventMessageReceiver<ResponseTokenMessage>
{
    private readonly IOverlayActivator _overlayActivator;
    private readonly IEventMessageSender _eventMessageSender;

    private string _resolvedToken = string.Empty;

    public HumanVerifier(IOverlayActivator overlayActivator, IEventMessageSender eventMessageSender)
    {
        _overlayActivator = overlayActivator;
        _eventMessageSender = eventMessageSender;
    }

    public async Task<string> VerifyAsync(string token)
    {
        _eventMessageSender.Send(new RequestTokenMessage(token));
        await _overlayActivator.ShowOverlayAsync<HumanVerificationOverlayViewModel>();
        return _resolvedToken;
    }

    public void Receive(ResponseTokenMessage message)
    {
        _resolvedToken = message.Token;
    }
}