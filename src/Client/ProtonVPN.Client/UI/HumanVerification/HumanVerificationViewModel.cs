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

using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.HumanVerification;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Navigation;

namespace ProtonVPN.Client.UI.HumanVerification;

public partial class HumanVerificationViewModel : OverlayViewModelBase, IEventMessageReceiver<RequestTokenMessage>
{
    private readonly IEventMessageSender _eventMessageSender;

    private string _token = string.Empty;
    
    public HumanVerificationViewModel(ILocalizationProvider localizationProvider, IMainViewNavigator viewNavigator, IEventMessageSender eventMessageSender) 
        : base(localizationProvider, viewNavigator)
    {
        _eventMessageSender = eventMessageSender;
    }

    //TODO: use configuration for api url
    public string Url => $"https://vpn-api.proton.me/core/v4/captcha?Token={_token}";

    [RelayCommand]
    public void TriggerVerificationTokenMessage(string token)
    {
        _eventMessageSender.Send(new ResponseTokenMessage(token));
        ViewNavigator.CloseOverlay();
    }

    public void Receive(RequestTokenMessage message)
    {
        _token = message.Token;
    }
}