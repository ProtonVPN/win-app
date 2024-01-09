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

using System.Net.NetworkInformation;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Geographical;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Handlers;

public class DeviceLocationHandler : 
    IHandler, 
    IEventMessageReceiver<ConnectionStatusChanged>, 
    IEventMessageReceiver<ApplicationStartedMessage>,
    IEventMessageReceiver<ConnectionDetailsChanged>
{
    private const int DISCONNECTING_FETCH_DELAY_IN_MS = 5000;
    private const int NETWORK_CHANGED_FETCH_DELAY_IN_MS = 2000;
    private const int APP_START_FETCH_DELAY_IN_MS = 0;

    private readonly ILogger _logger;
    private readonly IApiClient _apiClient;
    private readonly ISettings _settings;
    private readonly IConnectionManager _connectionManager;
    private readonly IEventMessageSender _eventMessageSender;

    private bool _isFetchInProgress = false;

    public DeviceLocationHandler(ILogger logger, IApiClient apiClient, ISettings settings, IConnectionManager connectionManager, IEventMessageSender eventMessageSender)
    {
        _logger = logger;
        _apiClient = apiClient;
        _settings = settings;
        _connectionManager = connectionManager;
        _eventMessageSender = eventMessageSender;

        NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;
    }
    public void Receive(ConnectionDetailsChanged message)
    {
        DeviceLocation? currentLocation = _settings.DeviceLocation;

        // Connection details does not contain the ISP info. If IP address has not changed, we can assume ISP is the same as previously.
        string isp = currentLocation != null && message.ClientIpAddress == currentLocation?.IpAddress 
            ? currentLocation.Value.Isp
            : string.Empty;

        UpdateDeviceLocation(message.ClientIpAddress, message.ClientCountryCode, isp);
    }

    public async void Receive(ConnectionStatusChanged message)
    {
        if (_connectionManager.IsDisconnected)
        {
            await FetchDeviceLocationAsync(DISCONNECTING_FETCH_DELAY_IN_MS);
        };
    }

    public async void Receive(ApplicationStartedMessage message)
    {
        await FetchDeviceLocationAsync(APP_START_FETCH_DELAY_IN_MS);
    }

    private async void OnNetworkAddressChanged(object? sender, EventArgs e)
    {
        await FetchDeviceLocationAsync(NETWORK_CHANGED_FETCH_DELAY_IN_MS);
    }

    private async Task FetchDeviceLocationAsync(int delayInMs)
    {
        if (_isFetchInProgress)
        {
            return;
        }

        try
        {
            _isFetchInProgress = true;

            await Task.Delay(delayInMs);

            DeviceLocationResponse? response = await GetDeviceLocationAsync();
            if (response != null)
            {
                UpdateDeviceLocation(response.Ip, response.Country, response.Isp);
            }
        }
        finally
        {
            _isFetchInProgress = false;
        }
    }

    private async Task<DeviceLocationResponse?> GetDeviceLocationAsync()
    {
        try
        {
            ApiResponseResult<DeviceLocationResponse> response = await _apiClient.GetLocationDataAsync();

            return response.Success
                ? response.Value
                : null;
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("Error occured while fetching user location.", e);
            return null;
        }
    }

    private void UpdateDeviceLocation(string ipAddress, string countryCode, string isp)
    {
        DeviceLocation deviceLocation = new()
        {
            IpAddress = ipAddress,
            CountryCode = countryCode,
            Isp = isp
        };

        _settings.DeviceLocation = deviceLocation;

        _eventMessageSender.Send(new DeviceLocationChangedMessage(deviceLocation));
    }
}