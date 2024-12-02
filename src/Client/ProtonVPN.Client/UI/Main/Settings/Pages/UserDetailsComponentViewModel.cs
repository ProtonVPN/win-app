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
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Services.Bootstrapping;
using ProtonVPN.Client.Services.Browsing;
using ProtonVPN.Client.Services.SignoutHandling;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Extensions;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Settings.Pages;

public partial class UserDetailsComponentViewModel : PageViewModelBase,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly ISignOutHandler _signoutHandler;
    private readonly IUrls _urls;
    private readonly ISettings _settings;
    private readonly IWebAuthenticator _webAuthenticator;
    private readonly IBootstrapper _bootstrapper;

    public string Username => _settings.GetUsername();

    public string VpnPlan => Localizer.GetVpnPlanName(_settings.VpnPlan.Title);

    public bool IsVpnPlan => _settings.VpnPlan.IsVpnPlan;

    public bool IsProtonPlan => _settings.VpnPlan.IsProtonPlan;

    public UserDetailsComponentViewModel(
        ISignOutHandler signoutHandler,
        IUrls urls,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IConnectionManager connectionManager,
        ISettings settings,
        IWebAuthenticator webAuthenticator,
        IBootstrapper bootstrapper,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter) : base(localizer, logger, issueReporter)
    {
        _signoutHandler = signoutHandler;
        _urls = urls;
        _settings = settings;
        _webAuthenticator = webAuthenticator;
        _bootstrapper = bootstrapper;
    }

    [RelayCommand]
    public async Task OpenMyAccountUrlAsync()
    {
        _urls.NavigateTo(await _webAuthenticator.GetMyAccountUrlAsync());
    }

    [RelayCommand]
    private Task SignOutAsync()
    {
        return _signoutHandler.SignOutAsync();
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