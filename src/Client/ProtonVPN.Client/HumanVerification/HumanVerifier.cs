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
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.HumanVerification;

namespace ProtonVPN.Client.HumanVerification;

public class HumanVerifier : IHumanVerifier, IEventMessageReceiver<ResponseTokenMessage>
{
    private readonly IDialogActivator _dialogActivator;
    private readonly IEventMessageSender _eventMessageSender;

    private string _resolvedToken;

    public HumanVerifier(IDialogActivator dialogActivator, IEventMessageSender eventMessageSender)
    {
        _dialogActivator = dialogActivator;
        _eventMessageSender = eventMessageSender;
    }

    public async Task<string> VerifyAsync(string token)
    {
        _eventMessageSender.Send(new RequestTokenMessage(token));
        await _dialogActivator.ShowAsync(typeof(HumanVerificationViewModel).FullName!);
        return _resolvedToken;
    }

    public void Receive(ResponseTokenMessage message)
    {
        _resolvedToken = message.Token;
    }
}