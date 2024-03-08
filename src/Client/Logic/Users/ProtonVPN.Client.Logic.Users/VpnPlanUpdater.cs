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

using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Users;

public class VpnPlanUpdater : IVpnPlanUpdater
{
    private readonly IApiClient _apiClient;
    private readonly ISettings _settings;
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private DateTime _minimumRequestDateUtc = DateTime.MinValue;

    public VpnPlanUpdater(IApiClient apiClient,
        ISettings settings,
        ILogger logger,
        IConfiguration configuration,
        IEventMessageSender eventMessageSender)
    {
        _apiClient = apiClient;
        _settings = settings;
        _logger = logger;
        _configuration = configuration;
        _eventMessageSender = eventMessageSender;
    }

    public async Task<ApiResponseResult<VpnInfoWrapperResponse>?> ForceUpdateAsync()
    {
        return await EnqueueRequestAsync(isToForceRequest: true);
    }

    public async Task<ApiResponseResult<VpnInfoWrapperResponse>?> UpdateAsync()
    {
        return await EnqueueRequestAsync(isToForceRequest: false);
    }

    private async Task<ApiResponseResult<VpnInfoWrapperResponse>?> EnqueueRequestAsync(bool isToForceRequest)
    {
        await _semaphore.WaitAsync();

        ApiResponseResult<VpnInfoWrapperResponse>? response = null;
        try
        {
            if (isToForceRequest || IsToRequest())
            {
                _minimumRequestDateUtc = DateTime.UtcNow + _configuration.VpnPlanMinimumRequestInterval;
                _logger.Info<AppLog>($"Requesting a VPN plan update (Force request: {isToForceRequest}) " +
                    $"(Minimum request date UTC: {_minimumRequestDateUtc})");

                response = await _apiClient.GetVpnInfoResponse();

                if (response.Success)
                {
                    OnResponseSuccess(response.Value.Vpn);
                }
                else
                {
                    _logger.Error<AppLog>("VPN plan request failed with " +
                        $"Status Code {response.ResponseMessage.StatusCode}, " +
                        $"Internal Code {response.Value.Code}, " +
                        $"Error '{response.Value.Error}'.");
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("VPN plan request failed.", e);
        }
        finally
        {
            _semaphore.Release();
        }
        return response;
    }

    private bool IsToRequest()
    {
        return DateTime.UtcNow >= _minimumRequestDateUtc;
    }

    private void OnResponseSuccess(VpnInfoResponse vpnInfoResponse)
    {
        VpnPlanChangedMessage message = new(vpnInfoResponse.PlanTitle, vpnInfoResponse.MaxTier);

        if (HasAnyVpnPlanSettingChanged(message))
        {
            _settings.VpnPlanTitle = message.PlanTitle;
            _settings.IsPaid = message.IsPaid;
            _settings.MaxTier = message.MaxTier;

            _eventMessageSender.Send(message);
        }
    }

    private bool HasAnyVpnPlanSettingChanged(VpnPlanChangedMessage message)
    {
        return _settings.VpnPlanTitle != message.PlanTitle ||
               _settings.IsPaid != message.IsPaid ||
               _settings.MaxTier != message.MaxTier;
    }
}