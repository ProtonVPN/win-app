﻿/*
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

using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.UI.Login.Bases;

namespace ProtonVPN.Client.UI.Login.Pages;

public class LoadingPageViewModel : LoginPageViewModelBase
{
    private readonly IUserAuthenticator _userAuthenticator;

    public string? Message => _userAuthenticator.AuthenticationStatus switch
    {
        AuthenticationStatus.LoggingIn => Localizer.Get("Main_Loading_SigningIn"),
        AuthenticationStatus.LoggingOut => Localizer.Get("Main_Loading_SigningOut"),
        _ => null
    };

    public LoadingPageViewModel(
        IUserAuthenticator userAuthenticator,
        ILoginViewNavigator parentViewNavigator,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator, viewModelHelper)
    {
        _userAuthenticator = userAuthenticator;
    }
}