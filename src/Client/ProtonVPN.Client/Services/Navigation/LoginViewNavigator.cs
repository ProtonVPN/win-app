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
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Mapping;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Core.Services.Navigation.Bases;
using ProtonVPN.Client.UI.Login.Pages;
using ProtonVPN.Client.Common.Dispatching;

namespace ProtonVPN.Client.Services.Navigation;

public class LoginViewNavigator : ViewNavigatorBase, ILoginViewNavigator,
    IEventMessageReceiver<AuthenticationStatusChanged>
{
    private readonly IUserAuthenticator _userAuthenticator;

    public override bool IsNavigationStackEnabled => true;

    public LoginViewNavigator(
        ILogger logger,
        IPageViewMapper pageViewMapper,
        IUIThreadDispatcher uiThreadDispatcher,
        IUserAuthenticator userAuthenticator)
        : base(logger, pageViewMapper, uiThreadDispatcher)
    {
        _userAuthenticator = userAuthenticator;
    }

    public Task<bool> NavigateToLoadingViewAsync()
    {
        return NavigateToAsync<LoadingPageViewModel>();
    }

    public Task<bool> NavigateToSignInViewAsync()
    {
        return NavigateToAsync<SignInPageViewModel>();
    }

    public Task<bool> NavigateToSignInViewAsync(SignInForm form)
    {
        return NavigateToAsync<SignInPageViewModel>(form);
    }

    public Task<bool> NavigateToTwoFactorViewAsync()
    {
        return NavigateToAsync<TwoFactorPageViewModel>();
    }

    public override Task<bool> NavigateToDefaultAsync()
    {
        return _userAuthenticator.AuthenticationStatus switch
        {
            AuthenticationStatus.LoggingIn or
            AuthenticationStatus.LoggingOut => NavigateToLoadingViewAsync(),
            _ => NavigateToSignInViewAsync()
        };
    }

    public void Receive(AuthenticationStatusChanged message)
    {
        UIThreadDispatcher.TryEnqueue(async () => await NavigateToDefaultAsync());
    }
}