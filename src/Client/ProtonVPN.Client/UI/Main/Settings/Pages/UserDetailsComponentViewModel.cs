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
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Users.Contracts;
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
    private readonly ISettings _settings;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IWebAuthenticator _webAuthenticator;
    private readonly IBootstrapper _bootstrapper;
    private readonly IUserSessionsObserver _userSessionsObserver;

    public int CurrentActiveDeviceCount => _userSessionsObserver.CurrentSessionCount;

    public int MaxDevicesAllowed => _settings.MaxDevicesAllowed;

    public string Username => _settings.Username ?? _settings.UserDisplayName ?? string.Empty;

    public string VpnPlan => Localizer.GetVpnPlanName(_settings.VpnPlan.Title);

    public bool IsVpnPlan => _settings.VpnPlan.IsVpnPlan;

    public bool IsProtonPlan => _settings.VpnPlan.IsProtonPlan;

    public UserDetailsComponentViewModel(
        IUrls urls,
        ISettings settings,
        IUserAuthenticator userAuthenticator,
        IWebAuthenticator webAuthenticator,
        IBootstrapper bootstrapper,
        IUserSessionsObserver userSessionsObserver,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter) : base(localizer, logger, issueReporter)
    {
        _urls = urls;
        _settings = settings;
        _userAuthenticator = userAuthenticator;
        _webAuthenticator = webAuthenticator;
        _bootstrapper = bootstrapper;
        _userSessionsObserver = userSessionsObserver;
    }

    [RelayCommand]
    public async Task OpenMyAccountUrlAsync()
    {
        _urls.NavigateTo(await _webAuthenticator.GetMyAccountUrlAsync());
    }

    [RelayCommand]
    public async Task SignOutAsync()
    {
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
            OnPropertyChanged(nameof(MaxDevicesAllowed));
        }
    }
}