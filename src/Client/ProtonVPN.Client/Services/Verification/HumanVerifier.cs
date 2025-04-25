/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Services.Verification.Messages;
using ProtonVPN.Client.Common.Dispatching;

namespace ProtonVPN.Client.Services.Verification;

public class HumanVerifier : IHumanVerifier,
    IEventMessageReceiver<ResponseTokenMessage>
{
    private readonly IMainWindowOverlayActivator _overlayActivator;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;

    private string _resolvedToken = string.Empty;

    public HumanVerifier(
        IMainWindowOverlayActivator overlayActivator,
        IEventMessageSender eventMessageSender,
        IUIThreadDispatcher uiThreadDispatcher)
    {
        _overlayActivator = overlayActivator;
        _eventMessageSender = eventMessageSender;
        _uiThreadDispatcher = uiThreadDispatcher;
    }

    public async Task<string> VerifyAsync(string token)
    {
        _eventMessageSender.Send(new RequestTokenMessage(token));

        await _uiThreadDispatcher.TryEnqueueAsync(_overlayActivator.ShowHumanVerificationOverlayAsync);

        return _resolvedToken;
    }

    public void Receive(ResponseTokenMessage message)
    {
        _resolvedToken = message.Token;
    }
}