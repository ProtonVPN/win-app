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

using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Services.Bootstrapping;
using ProtonVPN.Client.Services.Browsing;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Settings.Pages;

public partial class UserDetailsComponentViewModel : PageViewModelBase,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly IUrls _urls;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IConnectionManager _connectionManager;
    private readonly ISettings _settings;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IWebAuthenticator _webAuthenticator;
    private readonly IBootstrapper _bootstrapper;

    public string Username => _settings.Username ?? _settings.UserDisplayName ?? string.Empty;

    public string VpnPlan => Localizer.GetVpnPlanName(_settings.VpnPlan.Title);

    public bool IsVpnPlan => _settings.VpnPlan.IsVpnPlan;

    public bool IsProtonPlan => _settings.VpnPlan.IsProtonPlan;

    public UserDetailsComponentViewModel(
        IUrls urls,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IConnectionManager connectionManager,
        ISettings settings,
        IUserAuthenticator userAuthenticator,
        IWebAuthenticator webAuthenticator,
        IBootstrapper bootstrapper,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter) : base(localizer, logger, issueReporter)
    {
        _urls = urls;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _connectionManager = connectionManager;
        _settings = settings;
        _userAuthenticator = userAuthenticator;
        _webAuthenticator = webAuthenticator;
        _bootstrapper = bootstrapper;
    }

    [RelayCommand]
    public async Task OpenMyAccountUrlAsync()
    {
        _urls.NavigateTo(await _webAuthenticator.GetMyAccountUrlAsync());
    }

    [RelayCommand]
    public async Task SignOutAsync()
    {
        ContentDialogResult result = await _mainWindowOverlayActivator.ShowMessageAsync(
            new MessageDialogParameters
            {
                Title = Localizer.GetFormat("Home_Account_SignOut_Confirmation_Title", Username),
                Message = Localizer.GetExitOrSignOutConfirmationMessage(_connectionManager.IsDisconnected, _settings),
                MessageType = DialogMessageType.RichText,
                PrimaryButtonText = Localizer.Get("Home_Account_SignOut"),
                CloseButtonText = Localizer.Get("Common_Actions_Cancel"),
            });

        if (result is not ContentDialogResult.Primary)
        {
            return;
        }

        await _userAuthenticator.LogoutAsync(LogoutReason.UserAction);
    }

    [RelayCommand]
    public async Task ExitApplicationAsync()
    {
        await _bootstrapper.ExitAsync();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(VpnPlan));
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.VpnPlan))
        {
            OnPropertyChanged(nameof(IsVpnPlan));
            OnPropertyChanged(nameof(IsProtonPlan));
            OnPropertyChanged(nameof(VpnPlan));
        }
    }
}