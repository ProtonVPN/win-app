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

using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Extensions;

namespace ProtonVPN.Client.Services.SignoutHandling;

public class SignOutHandler : ISignOutHandler
{
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly ILocalizationProvider _localizer;
    private readonly ISettings _settings;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IConnectionManager _connectionManager;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;

    public SignOutHandler(
        IUrlsBrowser urlsBrowser,
        ILocalizationProvider localizer,
        ISettings settings,
        IUserAuthenticator userAuthenticator,
        IConnectionManager connectionManager,
        IMainWindowOverlayActivator mainWindowOverlayActivator)
    {
        _urlsBrowser = urlsBrowser;
        _localizer = localizer;
        _settings = settings;
        _userAuthenticator = userAuthenticator;
        _connectionManager = connectionManager;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
    }

    public async Task SignOutAsync()
    {
        InlineTextButton advancedKillSwitchLearnMoreButton = new()
        {
            Text = _localizer.Get("Common_Links_LearnMore"),
            Url = _urlsBrowser.AdvancedKillSwitchLearnMore,
        };

        ContentDialogResult result = await _mainWindowOverlayActivator.ShowMessageAsync(
            new MessageDialogParameters
            {
                Title = _localizer.GetFormat("Home_Account_SignOut_Confirmation_Title", _settings.GetUsername()),
                Message = _localizer.GetExitOrSignOutConfirmationMessage(_connectionManager.IsDisconnected, _settings),
                MessageType = DialogMessageType.RichText,
                PrimaryButtonText = _localizer.Get("Home_Account_SignOut"),
                CloseButtonText = _localizer.Get("Common_Actions_Cancel"),
                TrailingInlineButton = _settings.IsAdvancedKillSwitchActive()
                    ? advancedKillSwitchLearnMoreButton
                    : null
            });

        if (result is not ContentDialogResult.Primary)
        {
            return;
        }

        await _userAuthenticator.LogoutAsync(LogoutReason.UserAction);
    }
}