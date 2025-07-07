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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Common.UI.Controls.Map;
using ProtonVPN.Client.Contracts.Messages;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Cache;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;

namespace ProtonVPN.Client.UI.Main.Map;

public partial class MapComponentViewModel : ViewModelBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<MainWindowVisibilityChangedMessage>,
    IEventMessageReceiver<ServerListChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IServersCache _serversCache;
    private readonly IConnectionManager _connectionManager;
    private readonly ICoordinatesProvider _coordinatesProvider;
    private readonly IUpsellCarouselWindowActivator _upsellCarouselWindowActivator;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;

    [ObservableProperty]
    private bool _isMainWindowVisible;

    [ObservableProperty]
    private List<Country> _countries = [];

    [ObservableProperty]
    private Country? _currentCountry;

    public bool IsConnecting => _connectionManager.IsConnecting;
    public bool IsConnected => _connectionManager.IsConnected;
    public bool IsDisconnected => _connectionManager.IsDisconnected;

    public MapComponentViewModel(
        ISettings settings,
        IServersCache serversCache,
        IConnectionManager connectionManager,
        ICoordinatesProvider coordinatesProvider,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _settings = settings;
        _serversCache = serversCache;
        _connectionManager = connectionManager;
        _coordinatesProvider = coordinatesProvider;
        _upsellCarouselWindowActivator = upsellCarouselWindowActivator;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;

        InvalidateActiveCountry();
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            InvalidateActiveCountry();

            OnPropertyChanged(nameof(IsConnecting));
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(IsDisconnected));
        });
    }

    public void Receive(SettingChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            if (message.PropertyName == nameof(ISettings.DeviceLocation))
            {
                InvalidateActiveCountry();
            }
        });
    }

    public void Receive(MainWindowVisibilityChangedMessage message)
    {
        ExecuteOnUIThread(() => IsMainWindowVisible = message.IsMainWindowVisible);
    }

    public void Receive(ServerListChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            InvalidateCountries();
            InvalidateActiveCountry();
        });
    }

    private void InvalidateActiveCountry()
    {
        switch (_connectionManager.ConnectionStatus)
        {
            case ConnectionStatus.Connected:
            case ConnectionStatus.Connecting:
                {
                    string? countryCode = _connectionManager.CurrentConnectionDetails?.ExitCountryCode;
                    if (!string.IsNullOrEmpty(countryCode))
                    {
                        CurrentCountry = Countries.FirstOrDefault(c => c.Code == countryCode);
                    }
                    break;
                }
            case ConnectionStatus.Disconnected:
                CurrentCountry = Countries.FirstOrDefault(c => c.Code == _settings.DeviceLocation?.CountryCode);
                break;
        }
    }

    private void InvalidateCountries()
    {
        Countries = _serversCache.Countries
            .Select(c =>
            {
                (double Latitude, double Longitude)? coordinates = _coordinatesProvider.GetCoordinates(c);
                return coordinates != null
                    ? new Country
                    {
                        Name = Localizer.GetCountryName(c.Code),
                        Code = c.Code,
                        Latitude = coordinates.Value.Latitude,
                        Longitude = coordinates.Value.Longitude,
                        IsUnderMaintenance = c.IsStandardUnderMaintenance
                    }
                    : null;
            })
            .OfType<Country>()
            .ToList();
    }

    [RelayCommand]
    private Task ConnectAsync(Country country)
    {
        if (!_settings.VpnPlan.IsPaid)
        {
            return _upsellCarouselWindowActivator.ActivateAsync(UpsellFeatureType.WorldwideCoverage);
        }

        if (country == null || country.IsUnderMaintenance)
        {
            return _mainWindowOverlayActivator.ShowMessageAsync(new()
            {
                Title =  Localizer.Get("Connections_Country_UnderMaintenance_Title"),
                Message =  Localizer.Get("Connections_Country_UnderMaintenance"),
                PrimaryButtonText = Localizer.Get("Common_Actions_GotIt"),
            });
        }

        return _connectionManager.ConnectAsync(VpnTriggerDimension.Map, new ConnectionIntent(new CountryLocationIntent(country.Code)));
    }
}