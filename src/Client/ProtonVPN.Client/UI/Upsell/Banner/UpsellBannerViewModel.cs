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
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Helpers;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Upsell.Banner;

public partial class UpsellBannerViewModel : ViewModelBase,
    IEventMessageReceiver<ConnectionStatusChanged>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<ChangeServerAttemptInvalidatedMessage>
{
    private readonly IUrls _urls;
    private readonly IWebAuthenticator _webAuthenticator;
    private readonly IConnectionManager _connectionManager;
    private readonly IChangeServerModerator _changeServerModerator;
    private readonly ISettings _settings;

    public bool IsBannerVisible => !_settings.IsPaid &&
                                   _connectionManager.ConnectionStatus == ConnectionStatus.Connected;

    public string Title => Localizer.Get(IsWrongCountryBannerVisible
        ? "UpsellBanner_WrongCountry_Title"
        : "UpsellBanner_NetShield_Title");

    public string Description => Localizer.Get(IsWrongCountryBannerVisible
        ? "UpsellBanner_WrongCountry_Description"
        : "UpsellBanner_NetShield_Description");

    public ImageSource Image => ResourceHelper.GetIllustration(IsWrongCountryBannerVisible
        ? "WrongCountryIllustrationSource"
        : "NetShieldIllustrationSource");

    private bool IsWrongCountryBannerVisible => !_changeServerModerator.CanChangeServer();

    public UpsellBannerViewModel(
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        IUrls urls,
        IWebAuthenticator webAuthenticator,
        IConnectionManager connectionManager,
        IChangeServerModerator changeServerModerator,
        ISettings settings)
        : base(localizationProvider, logger, issueReporter)
    {
        _urls = urls;
        _webAuthenticator = webAuthenticator;
        _connectionManager = connectionManager;
        _changeServerModerator = changeServerModerator;
        _settings = settings;
    }

    public void Receive(ChangeServerAttemptInvalidatedMessage message)
    {
        ExecuteOnUIThread(InvalidateProperties);
    }

    public void Receive(ConnectionStatusChanged message)
    {
        ExecuteOnUIThread(InvalidateProperties);
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateProperties);
    }

    [RelayCommand]
    public async Task UpgradeAsync()
    {
        // TODO: Show upsell carousel instead
        _urls.NavigateTo(await _webAuthenticator.GetUpgradeAccountUrlAsync(ModalSources.ChangeServer));
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Description));
    }

    private void InvalidateProperties()
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(Image));
        OnPropertyChanged(nameof(IsBannerVisible));
    }
}