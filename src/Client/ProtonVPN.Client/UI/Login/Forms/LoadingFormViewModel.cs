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

using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Login.Forms;

public class LoadingFormViewModel : PageViewModelBase<ILoginViewNavigator>,
    IEventMessageReceiver<AuthenticationStatusChanged>
{
    private readonly IUserAuthenticator _userAuthenticator;

    public override bool IsBackEnabled => false;

    public string? Message => _userAuthenticator.AuthenticationStatus switch
    {
        AuthenticationStatus.LoggingIn => Localizer.Get("Main_Loading_SigningIn"),
        AuthenticationStatus.LoggingOut => Localizer.Get("Main_Loading_SigningOut"),
        _ => null
    };

    public bool IsLoading => _userAuthenticator.AuthenticationStatus
        is AuthenticationStatus.LoggingIn
        or AuthenticationStatus.LoggingOut;

    public LoadingFormViewModel(
        ILoginViewNavigator loginViewNavigator,
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        IUserAuthenticator userAuthenticator)
        : base(loginViewNavigator, localizationProvider, logger, issueReporter)
    {
        _userAuthenticator = userAuthenticator;
    }

    public void Receive(AuthenticationStatusChanged message)
    {
        ExecuteOnUIThread(InvalidateLoadingScreen);
    }

    private void InvalidateLoadingScreen()
    {
        OnPropertyChanged(nameof(IsLoading));
        OnPropertyChanged(nameof(Message));
    }
}