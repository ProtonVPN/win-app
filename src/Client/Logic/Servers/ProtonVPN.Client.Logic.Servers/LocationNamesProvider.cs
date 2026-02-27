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
using ProtonVPN.Api.Contracts.Geographical;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Logic.Servers.Files;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ApiLogs;

namespace ProtonVPN.Client.Logic.Servers;

public class LocationNamesProvider :
    IEventMessageReceiver<LanguageChangedMessage>,
    IEventMessageReceiver<ServerListChangedMessage>
{
    private static readonly TimeSpan _cacheExpiry = TimeSpan.FromDays(7);

    private readonly IApiClient _apiClient;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly ILogger _logger;
    private readonly ILocationNamesFileReaderWriter _fileReaderWriter;
    private readonly ILocationLocalizationSetter _locationLocalizationSetter;
    private readonly IGlobalSettings _globalSettings;

    private LocationNamesFile _cacheFile = new();

    public LocationNamesProvider(
        IApiClient apiClient,
        IEventMessageSender eventMessageSender,
        ILogger logger,
        ILocationNamesFileReaderWriter fileReaderWriter,
        ILocationLocalizationSetter locationLocalizationSetter,
        IGlobalSettings globalSettings)
    {
        _apiClient = apiClient;
        _eventMessageSender = eventMessageSender;
        _logger = logger;
        _fileReaderWriter = fileReaderWriter;
        _locationLocalizationSetter = locationLocalizationSetter;
        _globalSettings = globalSettings;
    }

    private async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        string currentLanguage = _globalSettings.Language;

        if (_cacheFile.Languages.Count == 0)
        {
            LoadCacheFile();
        }

        bool hasCachedLanguage = _cacheFile.Languages.TryGetValue(currentLanguage, out LocationNamesCache? languageCache);
        bool cacheNeedsRefresh = !hasCachedLanguage || DateTime.UtcNow - languageCache!.LastUpdatedUtc > _cacheExpiry;

        if (cacheNeedsRefresh)
        {
            _logger.Info<ApiLog>($"Cache refresh needed for language '{currentLanguage}'. Has cache: {hasCachedLanguage}, Expired: {cacheNeedsRefresh}");
            await FetchFromApiAsync(currentLanguage, cancellationToken);
        }
        else
        {
            _logger.Info<ApiLog>($"Using cached location names for language '{currentLanguage}'.");
            SetActiveLanguage(languageCache!);
        }
    }

    public void Receive(LanguageChangedMessage message)
    {
        RefreshAsync().FireAndForget();
    }

    public void Receive(ServerListChangedMessage message)
    {
        RefreshAsync().FireAndForget();
    }

    private void LoadCacheFile()
    {
        _cacheFile = _fileReaderWriter.Read();
    }

    private void SetActiveLanguage(LocationNamesCache languageCache)
    {
        _locationLocalizationSetter.SetCityNames(languageCache.Cities);
        _locationLocalizationSetter.SetStateNames(languageCache.States);

        _eventMessageSender.Send<LocationNamesChangedMessage>();
    }

    private async Task FetchFromApiAsync(string requestedLanguage, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Info<ApiLog>($"Fetching localized location names from API for language '{requestedLanguage}'.");

            ApiResponseResult<LocalizedLocationsResponse> response = await _apiClient.GetCityNamesAsync(cancellationToken);

            if (response.Success)
            {
                LocationNamesCache newCache = new()
                {
                    LastUpdatedUtc = DateTime.UtcNow,
                    Cities = response.Value.Cities ?? [],
                    States = response.Value.States ?? []
                };

                _cacheFile.Languages[requestedLanguage] = newCache;
                _fileReaderWriter.Save(_cacheFile);

                _logger.Info<ApiLog>($"Using fetched location names for language '{requestedLanguage}'. Location names have been cached for future usage.");
                SetActiveLanguage(newCache);
            }
            else
            {
                _logger.Error<ApiErrorLog>($"Failed to fetch name translations: {response.Error}");

                if (_cacheFile.Languages.TryGetValue(requestedLanguage, out LocationNamesCache? staleCache))
                {                 
                    _logger.Info<ApiLog>($"Using stale cached location names for language '{requestedLanguage}' due to API failure.");
                    SetActiveLanguage(staleCache);;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error<ApiErrorLog>("Error fetching name translations", ex);
        }
    }
}