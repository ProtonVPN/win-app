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
using ProtonVPN.Client.Common.Observers;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts.Observers;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Geographical;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Logging.Contracts.Events.SettingsLogs;

namespace ProtonVPN.Client.Logic.Servers.Observers;

public class DeviceLocationObserver :
    ObserverBase,
    IDeviceLocationObserver,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<ConnectionDetailsChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>
{
    private const int DISCONNECTING_FETCH_DELAY_IN_MS = 8000;
    private const int NETWORK_CHANGED_FETCH_DELAY_IN_MS = 2000;
    private const int APP_START_FETCH_DELAY_IN_MS = 0;

    private readonly IApiClient _apiClient;
    private readonly ISettings _settings;
    private readonly IConnectionManager _connectionManager;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IUserAuthenticator _userAuthenticator;

    private bool _isFetchInProgress = false;

    public DeviceLocationObserver(
        ILogger logger,
        IIssueReporter issueReporter,
        IApiClient apiClient,
        ISettings settings,
        IConnectionManager connectionManager,
        IEventMessageSender eventMessageSender,
        IUserAuthenticator userAuthenticator)
        : base(logger, issueReporter)
    {
        _apiClient = apiClient;
        _settings = settings;
        _connectionManager = connectionManager;
        _eventMessageSender = eventMessageSender;
        _userAuthenticator = userAuthenticator;

        NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;

        Initialize();
    }

    public void Receive(ConnectionDetailsChangedMessage message)
    {
        DeviceLocation? currentLocation = _settings.DeviceLocation;

        // Secure core servers do not provide client IP address, so we should keep the last known.
        string ip = !string.IsNullOrEmpty(message.ClientIpAddress)
            ? message.ClientIpAddress
            : currentLocation?.IpAddress ?? string.Empty;

        // Connection details does not contain the ISP info. If IP address has not changed, we can assume ISP is the same as previously.
        string isp = currentLocation != null && message.ClientIpAddress == currentLocation?.IpAddress
            ? currentLocation.Value.Isp
            : string.Empty;

        // Secure core servers do not provide client country code, so we should keep the last known.
        string countryCode = !string.IsNullOrEmpty(message.ClientCountryCode)
            ? message.ClientCountryCode
            : currentLocation?.CountryCode ?? string.Empty;

        UpdateDeviceLocation(ip, countryCode, isp);
    }

    public async void Receive(ConnectionStatusChangedMessage message)
    {
        if (_connectionManager.IsDisconnected)
        {
            await FetchDeviceLocationAsync(DISCONNECTING_FETCH_DELAY_IN_MS);
        }
    }

    protected override async Task OnTriggerAsync()
    {
        try
        {
            if (!_connectionManager.IsDisconnected)
            {
                // API Location endpoint cannot be called while connected to a VPN server
                return;
            }

            Logger.Info<SettingsLog>("Retrieving current device location");

            ApiResponseResult<DeviceLocationResponse> response = await _apiClient.GetLocationDataAsync();
            if (response.Success)
            {
                DeviceLocationResponse currentLocation = response.Value;
                UpdateDeviceLocation(currentLocation.Ip, currentLocation.Country, currentLocation.Isp);
            }
        }
        catch (Exception e)
        {
            Logger.Error<AppLog>("Error occured while fetching device location.", e);
        }
    }

    private async void Initialize()
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

            TriggerAction.Run();
        }
        finally
        {
            _isFetchInProgress = false;
        }
    }

    private void UpdateDeviceLocation(string ipAddress, string countryCode, string isp)
    {
        DeviceLocation deviceLocation = new()
        {
            IpAddress = ipAddress,
            CountryCode = countryCode.NormalizeCountryCode(),
            Isp = isp
        };

        _settings.DeviceLocation = deviceLocation;

        if (string.IsNullOrEmpty(deviceLocation.CountryCode))
        {
            Logger.Warn<AppLog>($"Device country location is unknown.");
        }
        if (string.IsNullOrEmpty(deviceLocation.IpAddress))
        {
            Logger.Warn<AppLog>($"Device IP is unknown.");
        }
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.DeviceLocation))
        {
            _eventMessageSender.Send(new DeviceLocationChangedMessage(
                (DeviceLocation?)message.OldValue,
                _settings.DeviceLocation,
                _userAuthenticator.IsLoggedIn));
        }
    }
}