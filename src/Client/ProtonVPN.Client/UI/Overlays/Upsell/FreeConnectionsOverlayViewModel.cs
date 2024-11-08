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
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Searches;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Overlays.Upsell;

public partial class FreeConnectionsOverlayViewModel : OverlayViewModelBase<IMainWindowOverlayActivator>,
    IEventMessageReceiver<ServerListChangedMessage>
{
    private readonly IServersLoader _serversLoader;
    private readonly IServerCountCache _serverCountCache;
    private readonly IWebAuthenticator _webAuthenticator;
    private readonly IUrlsBrowser _urlsBrowser;

    public SmartObservableCollection<LocalizedCountry> FreeCountries { get; } = new();

    public string UpsellTagline
        => Localizer.GetFormat("Upsell_Carousel_WorldwideCoverage",
                Localizer.GetPluralFormat("Upsell_Carousel_WorldwideCoverage_Servers", _serverCountCache.GetServerCount()),
                Localizer.GetPluralFormat("Upsell_Carousel_WorldwideCoverage_Countries", _serverCountCache.GetCountryCount()));

    public long FreeCountriesCount => FreeCountries.Count;

    public FreeConnectionsOverlayViewModel(
        IMainWindowOverlayActivator overlayActivator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IServersLoader serversLoader,
        IServerCountCache serverCountCache,
        IUrlsBrowser urlsBrowser,
        IWebAuthenticator webAuthenticator)
        : base(overlayActivator, localizer, logger, issueReporter)
    {
        _serversLoader = serversLoader;
        _serverCountCache = serverCountCache;
        _urlsBrowser = urlsBrowser;
        _webAuthenticator = webAuthenticator;

        InvalidateFreeCountries();
    }

    public void Receive(ServerListChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateFreeCountries);
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateFreeCountries();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        InvalidateFreeCountries();
    }

    private void InvalidateFreeCountries()
    {
        List<LocalizedCountry> countries = _serversLoader
            .GetFreeCountries()
            .Select(c => new LocalizedCountry()
            {
                Country = c,
                LocalizedName = Localizer.GetCountryName(c.Code)
            })
            .OrderBy(c => c.LocalizedName).
            ToList();

        FreeCountries.Reset(countries);

        OnPropertyChanged(nameof(FreeCountriesCount));
        OnPropertyChanged(nameof(UpsellTagline));
    }

    [RelayCommand]
    private async Task UpgradePlanAsync()
    {
        _urlsBrowser.BrowseTo(await _webAuthenticator.GetUpgradeAccountUrlAsync(ModalSources.Countries));
    }
}