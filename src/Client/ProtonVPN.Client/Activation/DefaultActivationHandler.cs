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

using Microsoft.UI.Xaml;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Client.UI.Login;

namespace ProtonVPN.Client.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly ISettings _settings;
    private readonly IMainViewNavigator _viewNavigator;
    private readonly IUserAuthenticator _userAuthenticator;

    public DefaultActivationHandler(ISettings settings, IMainViewNavigator viewNavigator, IUserAuthenticator userAuthenticator)
    {
        _settings = settings;
        _viewNavigator = viewNavigator;
        _userAuthenticator = userAuthenticator;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        // None of the ActivationHandlers has handled the activation.
        return _viewNavigator.Frame?.Content == null;
    }

    protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        if (string.IsNullOrEmpty(_settings.Username))
        {
            _viewNavigator.NavigateTo<LoginViewModel>(args.Arguments);
        }
        else
        {
            _viewNavigator.NavigateTo<HomeViewModel>(args.Arguments);
            await _userAuthenticator.InvokeAutoLoginEventAsync();
        }
    }
}