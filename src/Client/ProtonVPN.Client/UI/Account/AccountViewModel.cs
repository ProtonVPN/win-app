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
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Account;

public partial class AccountViewModel : ViewModelBase,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IConnectionManager _connectionManager;
    private readonly IOverlayActivator _overlayActivator;
    private readonly IWebAuthenticator _webAuthenticator;
    private readonly IUrls _urls;

    public string Username => _settings.Username ?? _settings.UserDisplayName ?? string.Empty;

    public string VpnPlan => Localizer.GetVpnPlanName(_settings.VpnPlanTitle);

    public AccountViewModel(
        ILocalizationProvider localizationProvider,
        ISettings settings,
        IUserAuthenticator userAuthenticator,
        IConnectionManager connectionManager,
        IOverlayActivator overlayActivator,
        IWebAuthenticator webAuthenticator,
        IUrls urls,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(localizationProvider, logger, issueReporter)
    {
        _settings = settings;
        _userAuthenticator = userAuthenticator;
        _connectionManager = connectionManager;
        _overlayActivator = overlayActivator;
        _webAuthenticator = webAuthenticator;
        _urls = urls;
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(VpnPlan));
        });
    }

    [RelayCommand]
    public async Task SignOutAsync()
    {
        if (!_connectionManager.IsDisconnected)
        {
            ContentDialogResult result = await _overlayActivator.ShowMessageAsync(
                new MessageDialogParameters
                {
                    Title = Localizer.GetFormat("Home_Account_SignOut_Confirmation_Title", Username),
                    Message = Localizer.Get("Home_Account_SignOut_Confirmation_Message"),
                    PrimaryButtonText = Localizer.Get("Home_Account_SignOut"),
                    CloseButtonText = Localizer.Get("Common_Actions_Cancel"),
                });

            if (result is not ContentDialogResult.Primary) // Cancel sign out
            {
                return;
            }
        }

        await _userAuthenticator.LogoutAsync(LogoutReason.UserAction);
    }

    [RelayCommand]
    public async Task GoToMyAccountAsync()
    {
        _urls.NavigateTo(await _webAuthenticator.GetMyAccountUrlAsync());
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(VpnPlan));
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(VpnPlan));
        });
    }
}