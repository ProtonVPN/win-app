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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Features;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Threading;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Core.FeatureFlags;

public class FeatureFlagsProvider : IFeatureFlagsProvider
{
    private const bool DEFAULT_STATE_IF_FLAG_MISSING = false;

    private readonly SingleAction _updateAction;
    private readonly ISchedulerTimer _timer;
    private readonly ILogger _logger;
    private readonly IApiClient _apiClient;
    private readonly IFeatureFlagsCache _featureFlagsCache;

    public bool IsSsoEnabled => IsFlagEnabled("ExternalSSO");

    public FeatureFlagsProvider(
        ILogger logger,
        IApiClient apiClient,
        IConfiguration config,
        IScheduler scheduler,
        IFeatureFlagsCache featureFlagsCache)
    {
        _logger = logger;
        _apiClient = apiClient;
        _featureFlagsCache = featureFlagsCache;

        _updateAction = new(FetchAsync);

        _timer = scheduler.Timer();
        _timer.Interval = config.FeatureFlagsUpdateInterval.RandomizedWithDeviation(0.2);
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    public event EventHandler FeatureFlagsUpdated;

    public async Task UpdateAsync()
    {
        await _updateAction.Run();
    }

    protected bool IsFlagEnabled(string featureFlagName)
    {
        return _featureFlagsCache.Get().FirstOrDefault(f => f.Name.EqualsIgnoringCase(featureFlagName))?.IsEnabled
            ?? DEFAULT_STATE_IF_FLAG_MISSING;
    }

    private async void OnTimerTick(object sender, EventArgs eventArgs)
    {
        await UpdateAsync();
    }

    private async Task FetchAsync()
    {
        try
        {
            ApiResponseResult<FeatureFlagsResponse> response = await _apiClient.GetFeatureFlagsAsync();
            if (response.Success)
            {
                _featureFlagsCache.Store(Map(response.Value));
            }

            FeatureFlagsUpdated?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("Error occured when fetching feature flags", e);
        }
    }

    private IReadOnlyList<FeatureFlag> Map(FeatureFlagsResponse featureFlagsResponse)
    {
        List<FeatureFlag> featureFlags = featureFlagsResponse?.Toggles?.Select(f =>
            new FeatureFlag
            {
                Name = f.Name,
                IsEnabled = f.IsEnabled
            }).ToList() ?? new();

        return featureFlags;
    }
}