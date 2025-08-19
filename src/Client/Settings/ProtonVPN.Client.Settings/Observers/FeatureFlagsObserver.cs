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

using System.Reflection;
using System.Runtime.CompilerServices;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Features;
using ProtonVPN.Client.Common.Observers;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Settings.Attributes;
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

    [FeatureFlag("IPv6Support")]
    public bool IsIpv6SupportEnabled => IsEnabled();

    [FeatureFlag("IsLocalAreaNetworkAllowedForPaidUsersOnly")]
    public bool IsLocalAreaNetworkAllowedForPaidUsersOnly => IsEnabled();

    protected override TimeSpan PollingInterval => _config.FeatureFlagsUpdateInterval;

    private static PropertyInfo[] Properties { get; } = typeof(FeatureFlagsObserver).GetProperties();

    private static bool HasFeatureFlags { get; } = Properties.Any(prop => prop.IsDefined(typeof(FeatureFlagAttribute)));

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

    public Task UpdateAsync(CancellationToken cancellationToken)
    {
        return UpdateFeatureFlagsAsync(cancellationToken);
    }

    private bool IsEnabled([CallerMemberName] string propertyName = "")
    {
        PropertyInfo? property = GetType().GetProperty(propertyName)
            ?? throw new InvalidOperationException($"Property '{propertyName}' not found on {GetType().Name}");

        FeatureFlagAttribute? featureFlagAttribute = property.GetCustomAttribute<FeatureFlagAttribute>()
            ?? throw new InvalidOperationException($"Property '{propertyName}' is missing [FeatureFlag]");

        return _settings.FeatureFlags
            .FirstOrDefault(f => f.Name.EqualsIgnoringCase(featureFlagAttribute.Name), FeatureFlag.Default)
            .IsEnabled;
    }

    protected override Task OnTriggerAsync()
    {
        return UpdateFeatureFlagsAsync(CancellationToken.None);
    }

    private async Task UpdateFeatureFlagsAsync(CancellationToken cancellationToken)
    {
        try
        {
            Logger.Info<SettingsLog>("Fetching feature flags");

            ApiResponseResult<FeatureFlagsResponse> response = await _apiClient.GetFeatureFlagsAsync(cancellationToken);
            if (response.Success)
            {
                List<FeatureFlag> updatedFeatureFlags = Map(response.Value).ToList();
                List<FeatureFlagChange> changes = GetChanges(updatedFeatureFlags);

                _settings.FeatureFlags = updatedFeatureFlags;

                if (changes.Count > 0)
                {
                    _eventMessageSender.Send(new FeatureFlagsChangedMessage()
                    {
                        Changes = changes,
                    });
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error<SettingsLog>("Failed to retrieve feature flags", e);
        }
    }

    private List<FeatureFlagChange> GetChanges(List<FeatureFlag> updatedFeatureFlags)
    {
        List<FeatureFlagChange> changes = [];
        List<PropertyInfo> featureFlags = Properties.Where(prop => prop.IsDefined(typeof(FeatureFlagAttribute))).ToList();

        foreach (PropertyInfo featureFlagPropertyInfo in featureFlags)
        {
            string? featureFlagName = featureFlagPropertyInfo.GetCustomAttribute<FeatureFlagAttribute>()?.Name;
            if (string.IsNullOrEmpty(featureFlagName))
            {
                continue;
            }

            bool? oldValue = GetFeatureFlag(_settings.FeatureFlags, featureFlagName)?.IsEnabled;
            bool? newValue = GetFeatureFlag(updatedFeatureFlags, featureFlagName)?.IsEnabled;

            if (oldValue != newValue)
            {
                changes.Add(new()
                {
                    // Use property name instead of attribute name so that later we can compare
                    // using nameof(IFeatureFlagsObserver.FeatureFlag)
                    Name = featureFlagPropertyInfo.Name,
                    OldValue = oldValue,
                    NewValue = newValue,
                });
            }
        }

        return changes;
    }

    private static FeatureFlag? GetFeatureFlag(IReadOnlyList<FeatureFlag> featureFlags, string name)
    {
        return featureFlags.FirstOrNull(f => f.Name == name);
    }

    private static List<FeatureFlag> Map(FeatureFlagsResponse featureFlagsResponse)
    {
        return featureFlagsResponse?.FeatureFlags?.Select(f =>
            new FeatureFlag
            {
                Name = f.Name,
                IsEnabled = f.IsEnabled
            }).ToList() ?? [];
    }
}