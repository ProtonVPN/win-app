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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Models;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Home.ConnectionCard.Overlays;

public partial class FreeConnectionsOverlayViewModel : OverlayViewModelBase,
    IEventMessageReceiver<ServerListChangedMessage>
{
    private readonly IServersLoader _serversLoader;
    private readonly IWebAuthenticator _webAuthenticator;
    private readonly IUrls _urls;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FreeCountriesCount))]
    private SmartObservableCollection<Country> _freeCountries;

    public int FreeCountriesCount => FreeCountries.Count;

    public FreeConnectionsOverlayViewModel(
        ILocalizationProvider localizationProvider,
        IOverlayActivator overlayActivator,
        IServersLoader serversLoader,
        IWebAuthenticator webAuthenticator,
        IUrls urls,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(localizationProvider,
            logger,
            issueReporter,
            overlayActivator)
    {
        _serversLoader = serversLoader;
        _webAuthenticator = webAuthenticator;
        _urls = urls;
        _freeCountries = [];

        InvalidateFreeCountries();
    }

    public void Receive(ServerListChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateFreeCountries);
    }

    [RelayCommand]
    private async Task UpgradePlanAsync()
    {
        _urls.NavigateTo(await _webAuthenticator.GetUpgradeAccountUrlAsync(ModalSources.Countries));
    }

    private void InvalidateFreeCountries()
    {
        FreeCountries.Reset(_serversLoader
            .GetFreeCountryCodes()
            .Select(GetCountry)
            .OrderBy(c => c.Name));

        OnPropertyChanged(nameof(FreeCountriesCount));
    }

    private Country GetCountry(string countryCode)
    {
        return new Country()
        {
            Code = countryCode,
            Name = Localizer.GetCountryName(countryCode)
        };
    }
}