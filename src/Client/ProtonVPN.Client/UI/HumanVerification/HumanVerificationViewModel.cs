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
using Microsoft.UI.Xaml;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.HumanVerification;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Themes;

namespace ProtonVPN.Client.UI.HumanVerification;

public partial class HumanVerificationViewModel : OverlayViewModelBase, IEventMessageReceiver<RequestTokenMessage>, IEventMessageReceiver<ThemeChangedMessage>
{
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IThemeSelector _themeSelector;

    private string _token = string.Empty;
    
    public HumanVerificationViewModel(ILocalizationProvider localizationProvider, IMainViewNavigator viewNavigator, IEventMessageSender eventMessageSender, IThemeSelector themeSelector) 
        : base(localizationProvider, viewNavigator)
    {
        _eventMessageSender = eventMessageSender;
        _themeSelector = themeSelector;
    }

    public bool IsDarkTheme => _themeSelector.GetTheme().ApplicationTheme == ApplicationTheme.Dark;

    //TODO: use configuration for api url
    public string Url => $"https://vpn-api.proton.me/core/v4/captcha?Token={_token}{(IsDarkTheme ? "&Dark" : string.Empty)}";

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

    public void Receive(ThemeChangedMessage message)
    {
        OnPropertyChanged(nameof(IsDarkTheme));
        OnPropertyChanged(nameof(Url));
    }
}