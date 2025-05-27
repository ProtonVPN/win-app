/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.Api.Contracts.Features;
using ProtonVPN.Client.Common.Observers;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts.Observers;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.SettingsLogs;

namespace ProtonVPN.Client.Settings.Observers;

public class FeatureFlagsObserver : 
    PollingObserverBase, 
    IFeatureFlagsObserver
{
    private readonly ISettings _settings;
    private readonly IApiClient _apiClient;
    private readonly IConfiguration _config;
    private readonly IEventMessageSender _eventMessageSender;

    public bool IsLocalAreaNetworkAllowedForPaidUsersOnly => IsFlagEnabled("IsLocalAreaNetworkAllowedForPaidUsersOnly");

    protected override TimeSpan PollingInterval => _config.FeatureFlagsUpdateInterval;

    private bool HasFeatureFlags { get; } = typeof(IFeatureFlagsObserver).GetProperties().Any(prop => prop.PropertyType == typeof(bool));

    public FeatureFlagsObserver(
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IApiClient apiClient,
        IConfiguration config,
        IEventMessageSender eventMessageSender)
        : base(logger, issueReporter)
    {
        _settings = settings;
        _apiClient = apiClient;
        _config = config;
        _eventMessageSender = eventMessageSender;

        if (HasFeatureFlags)
        {
            StartTimerAndTriggerOnStart();
        }
    }

    private bool IsFlagEnabled(string featureFlagName)
    {
        return _settings.FeatureFlags
            .FirstOrDefault(f => f.Name.EqualsIgnoringCase(featureFlagName), FeatureFlag.Default)
            .IsEnabled;
    }

    protected override async Task OnTriggerAsync()
    {
        try
        {
            Logger.Info<SettingsLog>("Fetching feature flags");

            ApiResponseResult<FeatureFlagsResponse> response = await _apiClient.GetFeatureFlagsAsync();
            if (response.Success)
            {
                _settings.FeatureFlags = Map(response.Value).ToList();

                _eventMessageSender.Send(new FeatureFlagsChangedMessage());
            }
        }
        catch (Exception e)
        {
            Logger.Error<SettingsLog>("Failed to retrieve feature flags", e);
        }
    }

    private IReadOnlyList<FeatureFlag> Map(FeatureFlagsResponse featureFlagsResponse)
    {
        List<FeatureFlag> featureFlags = featureFlagsResponse?.FeatureFlags?.Select(f =>
            new FeatureFlag
            {
                Name = f.Name,
                IsEnabled = f.IsEnabled
            }).ToList() ?? new();

        return featureFlags;
    }
}