/*
 * Copyright (c) 2026 Proton AG
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
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Logic.Servers.Files;
using ProtonVPN.Client.Logic.Servers.Loads;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Geographical;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ApiLogs;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Servers.Cache;

public class ServersCache : IServersCache
{
    private readonly IApiClient _apiClient;
    private readonly IEntityMapper _entityMapper;
    private readonly IServersFileReaderWriter _serversFileReaderWriter;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IConfiguration _config;
    private readonly ISettings _settings;
    private readonly ILogger _logger;
    private readonly IFavoriteServersStorage _favoriteServersStorage;
    private readonly IServerLoadsCalculator _serverLoadsCalculator;

    private readonly ReaderWriterLockSlim _lock = new();

    private string? _deviceCountryLocation;

    private sbyte? _userMaxTier;

    private bool _hasServersRequestFailed;

    private DateTime _lastFullUpdateUtc = DateTime.MinValue;
    private DateTime _lastLoadsUpdateUtc = DateTime.MinValue;

    private IReadOnlyList<Server> _originalServers = [];

    private IReadOnlyList<Server> _filteredServers = [];
    public IReadOnlyList<Server> Servers => GetWithReadLock(() => _filteredServers);

    private IReadOnlyList<FreeCountry> _freeCountries = [];
    public IReadOnlyList<FreeCountry> FreeCountries => GetWithReadLock(() => _freeCountries);

    private IReadOnlyList<Country> _countries = [];
    public IReadOnlyList<Country> Countries => GetWithReadLock(() => _countries);

    private IReadOnlyList<State> _states = [];
    public IReadOnlyList<State> States => GetWithReadLock(() => _states);

    private IReadOnlyList<City> _cities = [];
    public IReadOnlyList<City> Cities => GetWithReadLock(() => _cities);

    private IReadOnlyList<Gateway> _gateways = [];
    public IReadOnlyList<Gateway> Gateways => GetWithReadLock(() => _gateways);

    private IReadOnlyList<SecureCoreCountryPair> _secureCoreCountryPairs = [];
    public IReadOnlyList<SecureCoreCountryPair> SecureCoreCountryPairs => GetWithReadLock(() => _secureCoreCountryPairs);

    public ServersCache(IApiClient apiClient,
        IEntityMapper entityMapper,
        IServersFileReaderWriter serversFileReaderWriter,
        IEventMessageSender eventMessageSender,
        IConfiguration config,
        ISettings settings,
        ILogger logger,
        IFavoriteServersStorage favoriteServersLoader,
        IServerLoadsCalculator serverLoadsCalculator)
    {
        _apiClient = apiClient;
        _entityMapper = entityMapper;
        _serversFileReaderWriter = serversFileReaderWriter;
        _eventMessageSender = eventMessageSender;
        _config = config;
        _settings = settings;
        _logger = logger;
        _favoriteServersStorage = favoriteServersLoader;
        _serverLoadsCalculator = serverLoadsCalculator;
    }

    public bool IsEmpty()
    {
        return Servers is null || Servers.Count == 0;
    }

    public bool AreAllServersUnderMaintenance()
    {
        return Servers.All(s => s.IsUnderMaintenance());
    }

    public bool IsStale()
    {
        return _deviceCountryLocation != _settings.DeviceLocation?.CountryCode
            || _userMaxTier != _settings.VpnPlan.MaxTier;
    }

    public bool IsOutdated()
    {
        return DateTime.UtcNow - _lastFullUpdateUtc >= _config.ServerUpdateInterval;
    }

    public bool IsLoadOutdated()
    {
        return DateTime.UtcNow - _lastLoadsUpdateUtc >= _config.MinimumServerLoadUpdateInterval;
    }

    public bool HasServersRequestFailed()
    {
        return _hasServersRequestFailed;
    }

    public bool HasGatewaysAndNoCountries()
    {
        return Gateways.Any() && !Countries.Any();
    }

    public bool HasNoServers()
    {
#if DEBUG
        if (_settings.SkipNoConnectionsPage)
        {
            return false;
        }
#endif

        return IsEmpty() || AreAllServersUnderMaintenance();
    }

    private T GetWithReadLock<T>(Func<T> func)
    {
        _lock.EnterReadLock();
        try
        {
            return func();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void LoadFromFileIfEmpty()
    {
        if (IsEmpty())
        {
            _logger.Info<AppLog>("Cache is empty, loading servers from file.");

            ServersFile file = _serversFileReaderWriter.Read();
            ProcessServers(file.DeviceCountryLocation, file.UserMaxTier, file.Servers);
        }
    }

    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            _lastFullUpdateUtc = DateTime.MinValue;
            _lastLoadsUpdateUtc = DateTime.MinValue;

            _deviceCountryLocation = null;
            _userMaxTier = null;

            _originalServers = [];
            _filteredServers = [];
            _freeCountries = [];
            _countries = [];
            _states = [];
            _cities = [];
            _gateways = [];
            _secureCoreCountryPairs = [];
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public async Task UpdateAsync(CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<string> favoriteServerIds = GetFavoriteServerIds();

            DeviceLocation? deviceLocation = _settings.DeviceLocation;
            DateTime utcNow = DateTime.UtcNow;

            ApiResponseResult<ServersResponse> response = await _apiClient.GetServersAsync(
                deviceLocation,
                favoriteServerIds,
                cancellationToken);

            if (response.Success)
            {
                _lastFullUpdateUtc = utcNow;
                _lastLoadsUpdateUtc = utcNow;
                _hasServersRequestFailed = false;

                if (response.LastModified.HasValue)
                {
                    _settings.LogicalsLastModifiedDate = response.LastModified.Value;
                }

                if (response.IsNotModified)
                {
                    _logger.Info<ApiLog>("API: Get servers response was not modified since last call, using cached data.");
                }
                else
                {
                    _logger.Info<ApiLog>("API: Get servers response was modified since last call, updating cached data.");

                    List<Server> servers = _entityMapper.Map<LogicalServerResponse, Server>(response.Value.Servers);

                    // Handle race condition when new favorite servers are added between API request and response
                    if (response.Value.ResponseMetadata is not null && response.Value.ResponseMetadata.ListIsTruncated && favoriteServerIds.Any())
                    {
                        IEnumerable<string> favoriteServerIdsUpdated = GetFavoriteServerIds();
                        IEnumerable<string> preserveIds = favoriteServerIdsUpdated.Except(favoriteServerIds);
                        if (preserveIds.Any())
                        {
                            servers.AddRange(_originalServers.Where(s => preserveIds.Contains(s.Id)));
                        }
                    }

                    _settings.LastLogicalsStatusId = response.Value.StatusId;

                    bool result = await UpdateBinaryLoadsAsync(servers, cancellationToken);
                    if (!result)
                    {
                        _logger.Warn<ApiLog>("Loads were not updated.");
                        return;
                    }

                    string deviceCountryLocation = deviceLocation?.CountryCode ?? string.Empty;
                    sbyte userMaxTier = _settings.VpnPlan.MaxTier;

                    SaveToFile(deviceCountryLocation, userMaxTier, servers);
                    ProcessServers(deviceCountryLocation, userMaxTier, servers);
                }
            }
            else
            {
                _hasServersRequestFailed = true;
            }
        }
        catch (Exception e)
        {
            _logger.Error<ApiErrorLog>("API: Get servers failed", e);

            _hasServersRequestFailed = true;

            if (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
        }
    }

    private IEnumerable<string> GetFavoriteServerIds()
    {
        return _favoriteServersStorage.Get();
    }

    private async Task<byte[]?> GetServerStatusAndLoadFileAsync(string statusId, CancellationToken cancellationToken)
    {
        try
        {
            ApiResponseResult<byte[]> response = await _apiClient.GetServerLoadsAndStatusBinaryStringAsync(statusId, cancellationToken);
            return response.Success
                ? response.Value
                : null;
        }
        catch (Exception e)
        {
            _logger.Error<ApiErrorLog>("API: Get binary status file failed", e);
            return null;
        }
    }

    public async Task UpdateLoadsAsync(CancellationToken cancellationToken)
    {
        List<Server> servers = Servers.ToList();

        bool result = await UpdateBinaryLoadsAsync(servers, cancellationToken);

        if (result)
        {
            _lastLoadsUpdateUtc = DateTime.UtcNow;

            string deviceCountryLocation = _settings.DeviceLocation?.CountryCode ?? string.Empty;
            sbyte userMaxTier = _settings.VpnPlan.MaxTier;

            SaveToFile(deviceCountryLocation, userMaxTier, servers);
            ProcessServers(deviceCountryLocation, userMaxTier, servers);
        }
        else
        {
            _logger.Warn<ApiLog>("Loads were not updated.");
        }
    }

    private async Task<bool> UpdateBinaryLoadsAsync(List<Server> servers, CancellationToken cancellationToken)
    {
        if (_settings.LastLogicalsStatusId is null)
        {
            _logger.Warn<AppLog>("Cannot make the API request because LastLogicalsStatusId is null.");
            return false;
        }

        byte[]? statusFile = await GetServerStatusAndLoadFileAsync(_settings.LastLogicalsStatusId, cancellationToken);
        if (statusFile is null)
        {
            _logger.Warn<AppLog>("Cached server data was not updated, because status file is missing.");
            return false;
        }

        bool result = _serverLoadsCalculator.UpdateServerLoads(servers, statusFile, _settings.DeviceLocation);
        if (!result)
        {
            _logger.Warn<AppLog>("Cached server data was not updated, because server status and loads were not updated.");
            return false;
        }

        return true;
    }

    private void ProcessServers(string? deviceCountryLocation, sbyte? userMaxTier, IReadOnlyList<Server> servers)
    {
        SetIpv6Flags(servers);

        IReadOnlyList<FreeCountry> freeCountries = GetFreeCountries(servers);
        IReadOnlyList<Country> countries = GetCountries(servers);
        IReadOnlyList<State> states = GetStates(servers);
        IReadOnlyList<City> cities = GetCities(servers);
        IReadOnlyList<Gateway> gateways = GetGateways(servers);
        IReadOnlyList<SecureCoreCountryPair> secureCoreCountryPairs = GetSecureCoreCountryPairs(servers);
        IReadOnlyList<Server> filteredServers = GetFilteredServers(servers);

        _lock.EnterWriteLock();
        try
        {
            _deviceCountryLocation = deviceCountryLocation;
            _userMaxTier = userMaxTier;

            _originalServers = servers;
            _filteredServers = filteredServers;
            _freeCountries = freeCountries;
            _countries = countries;
            _states = states;
            _cities = cities;
            _gateways = gateways;
            _secureCoreCountryPairs = secureCoreCountryPairs;
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        _eventMessageSender.Send(new ServerListChangedMessage());
    }

    private static void SetIpv6Flags(IEnumerable<Server> servers)
    {
        foreach (Server server in servers)
        {
            if (server.Servers is null)
            {
                continue;
            }

            foreach (PhysicalServer physicalServer in server.Servers)
            {
                physicalServer.IsIpv6Supported = server.Features.IsSupported(ServerFeatures.Ipv6);
            }
        }
    }

    private IReadOnlyList<FreeCountry> GetFreeCountries(IEnumerable<Server> servers)
    {
        return servers
            .Where(s => !string.IsNullOrWhiteSpace(s.ExitCountry)
                     && s.IsFreeNonB2B())
            .GroupBy(s => s.ExitCountry)
            .Select(g => new FreeCountry()
            {
                Code = g.Key,
                IsLocationUnderMaintenance = IsUnderMaintenance(g)
            })
            .ToList();
    }

    private IReadOnlyList<Country> GetCountries(IEnumerable<Server> servers)
    {
        return servers
            .Where(s => !string.IsNullOrWhiteSpace(s.ExitCountry)
                     && s.IsPaidNonB2B())
            .GroupBy(s => s.ExitCountry)
            .Select(g => new Country()
            {
                Code = g.Key,
                Features = AggregateFeatures(g),
                IsStandardUnderMaintenance = IsUnderMaintenance(g, s => s.Features.IsStandard()),
                IsP2PUnderMaintenance = IsUnderMaintenance(g, s => s.Features.IsSupported(ServerFeatures.P2P)),
                IsSecureCoreUnderMaintenance = IsUnderMaintenance(g, s => s.Features.IsSupported(ServerFeatures.SecureCore)),
                IsTorUnderMaintenance = IsUnderMaintenance(g, s => s.Features.IsSupported(ServerFeatures.Tor))
            })
            .ToList();
    }

    private ServerFeatures AggregateFeatures<T>(IGrouping<T, Server> servers)
    {
        return servers.Aggregate(default(ServerFeatures), (combinedFeatures, s) => combinedFeatures | s.Features);
    }

    private bool IsUnderMaintenance<T>(IGrouping<T, Server> servers, Func<Server, bool>? filterFunc = null)
    {
        return !servers.Any(s => (filterFunc == null || filterFunc(s))
                              && !s.IsUnderMaintenance());
    }

    private IReadOnlyList<State> GetStates(IReadOnlyList<Server> servers)
    {
        return servers
            .Where(s => !string.IsNullOrWhiteSpace(s.ExitCountry)
                     && !string.IsNullOrWhiteSpace(s.State)
                     && s.IsPaidNonB2B())
            .GroupBy(s => new { Country = s.ExitCountry, s.State })
            .Select(g => new State()
            {
                CountryCode = g.Key.Country,
                Name = g.Key.State,
                Features = AggregateFeatures(g),
                IsStandardUnderMaintenance = IsUnderMaintenance(g, s => s.Features.IsStandard()),
                IsP2PUnderMaintenance = IsUnderMaintenance(g, s => s.Features.IsSupported(ServerFeatures.P2P)),
                IsSecureCoreUnderMaintenance = IsUnderMaintenance(g, s => s.Features.IsSupported(ServerFeatures.SecureCore)),
                IsTorUnderMaintenance = IsUnderMaintenance(g, s => s.Features.IsSupported(ServerFeatures.Tor))
            })
            .ToList();
    }

    private IReadOnlyList<City> GetCities(IReadOnlyList<Server> servers)
    {
        return servers
            .Where(s => !string.IsNullOrWhiteSpace(s.ExitCountry)
                     && !string.IsNullOrWhiteSpace(s.City)
                     && s.IsPaidNonB2B())
            .GroupBy(s => new { Country = s.ExitCountry, s.State, s.City })
            .Select(g => new City()
            {
                CountryCode = g.Key.Country,
                StateName = g.Key.State,
                Name = g.Key.City,
                Features = AggregateFeatures(g),
                IsStandardUnderMaintenance = IsUnderMaintenance(g, s => s.Features.IsStandard()),
                IsP2PUnderMaintenance = IsUnderMaintenance(g, s => s.Features.IsSupported(ServerFeatures.P2P)),
                IsSecureCoreUnderMaintenance = IsUnderMaintenance(g, s => s.Features.IsSupported(ServerFeatures.SecureCore)),
                IsTorUnderMaintenance = IsUnderMaintenance(g, s => s.Features.IsSupported(ServerFeatures.Tor))
            })
            .ToList();
    }

    private IReadOnlyList<Gateway> GetGateways(IReadOnlyList<Server> servers)
    {
        return servers
            .Where(s => s.Features.IsB2B()
                     && !string.IsNullOrWhiteSpace(s.GatewayName))
            .GroupBy(s => s.GatewayName)
            .Select(g => new Gateway()
            {
                Name = g.Key,
                IsLocationUnderMaintenance = IsUnderMaintenance(g)
            })
            .ToList();
    }

    private IReadOnlyList<SecureCoreCountryPair> GetSecureCoreCountryPairs(IReadOnlyList<Server> servers)
    {
        return servers
            .Where(s => s.Features.IsSupported(ServerFeatures.SecureCore)
                     && !string.IsNullOrWhiteSpace(s.EntryCountry)
                     && !string.IsNullOrWhiteSpace(s.ExitCountry))
            .GroupBy(s => new { s.EntryCountry, s.ExitCountry })
            .Select(g => new SecureCoreCountryPair()
            {
                EntryCountry = g.Key.EntryCountry,
                ExitCountry = g.Key.ExitCountry,
                IsLocationUnderMaintenance = IsUnderMaintenance(g)
            })
            .ToList();
    }

    private IReadOnlyList<Server> GetFilteredServers(IReadOnlyList<Server> servers)
    {
        ServerTiers maxTier = (ServerTiers)_settings.VpnPlan.MaxTier;

        List<Server> filteredServers = [];
        foreach (Server server in servers.Where(s => s.IsVisible))
        {
            if (server.Tier <= maxTier)
            {
                // Add all the servers the user can access (based on his plan)
                filteredServers.Add(server);
            }
            else if (server.Tier <= ServerTiers.Plus)
            {
                // Include all the servers the user cannot access (but without the physical servers)
                filteredServers.Add(server.CopyWithoutPhysicalServers());
            }
        }
        return filteredServers;
    }

    private void SaveToFile(string? deviceCountryLocation, sbyte? userMaxTier, List<Server> servers)
    {
        ServersFile serversFile = new()
        {
            DeviceCountryLocation = deviceCountryLocation,
            UserMaxTier = userMaxTier,
            Servers = servers,
        };
        _serversFileReaderWriter.Save(serversFile);
    }

    public void ReprocessServers()
    {
        _logger.Info<AppLog>("Reprocessing servers.");
        ProcessServers(_deviceCountryLocation, _userMaxTier, _originalServers);
    }

    public async Task<ApiResponseResult<LookupServerResponse>?> LookupAsync(string input)
    {
        LoadFromFileIfEmpty();
        try
        {
            DeviceLocation? deviceLocation = _settings.DeviceLocation;
            ApiResponseResult<LookupServerResponse> response = await _apiClient.GetServerByNameAsync(input, deviceLocation);
            if (response.Success)
            {
                Server server = _entityMapper.Map<LogicalServerResponse, Server>(response.Value.LogicalServer);
                List<Server> servers = _originalServers.ToList();
                Server? alreadyExistingServer = servers.FirstOrDefault(s => s.Id == server.Id);
                if (alreadyExistingServer is not null)
                {
                    servers.Remove(alreadyExistingServer);
                }
                servers.Add(server);

                string deviceCountryLocation = deviceLocation?.CountryCode ?? string.Empty;
                sbyte userMaxTier = _settings.VpnPlan.MaxTier;

                SaveToFile(deviceCountryLocation, userMaxTier, servers);
                ProcessServers(deviceCountryLocation, userMaxTier, servers);
            }
            else
            {
                _logger.Warn<ApiLog>($"API: Get server by name returned with Code: {response.ResponseMessage?.StatusCode}, Error: {response.Error}");
            }

            return response;
        }
        catch (Exception e)
        {
            _logger.Error<ApiErrorLog>("API: Get server by name failed", e);
        }

        return null;
    }
}