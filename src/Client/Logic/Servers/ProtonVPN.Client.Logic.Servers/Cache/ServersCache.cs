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
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Logic.Servers.Files;
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

    private readonly ReaderWriterLockSlim _lock = new();

    private string? _deviceCountryLocation;

    private sbyte? _userMaxTier; 
    
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
        ILogger logger)
    {
        _apiClient = apiClient;
        _entityMapper = entityMapper;
        _serversFileReaderWriter = serversFileReaderWriter;
        _eventMessageSender = eventMessageSender;
        _config = config;
        _settings = settings;
        _logger = logger;
    }

    public bool IsEmpty()
    {
        return Servers is null || Servers.Count == 0;
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
            DeviceLocation? deviceLocation = _settings.DeviceLocation;
            DateTime utcNow = DateTime.UtcNow;

            ApiResponseResult<ServersResponse> response = await _apiClient.GetServersAsync(deviceLocation, cancellationToken);
            if (response.Success)
            {
                _lastFullUpdateUtc = utcNow;
                _lastLoadsUpdateUtc = utcNow;

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

                    string deviceCountryLocation = deviceLocation?.CountryCode ?? string.Empty;
                    sbyte userMaxTier = _settings.VpnPlan.MaxTier;

                    SaveToFile(deviceCountryLocation, userMaxTier, servers);
                    ProcessServers(deviceCountryLocation, userMaxTier, servers);
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error<ApiErrorLog>("API: Get servers failed", e);

            if (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
        }
    }

    public async Task UpdateLoadsAsync(CancellationToken cancellationToken)
    {
        try
        {
            DeviceLocation? deviceLocation = _settings.DeviceLocation;
            DateTime utcNow = DateTime.UtcNow;

            ApiResponseResult<ServersResponse> response = await _apiClient.GetServerLoadsAsync(deviceLocation, cancellationToken);
            if (response.Success)
            {
                _lastLoadsUpdateUtc = utcNow;

                _logger.Info<ApiLog>("API: Get server loads response received, updating cached data.");

                List<Server> servers = Servers.ToList();
                List<ServerLoad> serverLoads = _entityMapper.Map<LogicalServerResponse, ServerLoad>(response.Value.Servers);

                foreach (ServerLoad serverLoad in serverLoads)
                {
                    Server? server = servers.FirstOrDefault(s => s.Id == serverLoad.Id);
                    if (server != null)
                    {
                        server.Load = serverLoad.Load;
                        server.Score = serverLoad.Score;

                        // Server loads response does not give physical server details, so...
                        // If the logical server only has one physical server, then the status of the logical and physical server are tied
                        // If the status for the logical is down, it means that all physical servers for this logical are down
                        // If the status for the logical is up, it means that at least one physical server is up, but we can't know which one(s)
                        // -> in that case, we need to wait the update servers call to update the status properly
                        if (serverLoad.Status == 0 || server.Servers.Count <= 1)
                        {
                            foreach (PhysicalServer physicalServer in server.Servers)
                            {
                                physicalServer.Status = serverLoad.Status;
                            }
                            server.Status = serverLoad.Status;
                        }
                    }
                }

                string deviceCountryLocation = deviceLocation?.CountryCode ?? string.Empty;
                sbyte userMaxTier = _settings.VpnPlan.MaxTier;

                SaveToFile(deviceCountryLocation, userMaxTier, servers);
                ProcessServers(deviceCountryLocation, userMaxTier, servers);
            }
        }
        catch (Exception e)
        {
            _logger.Error<ApiErrorLog>("API: Get servers load failed", e);
        }
    }

    private void ProcessServers(string? deviceCountryLocation, sbyte? userMaxTier, IReadOnlyList<Server> servers)
    {
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
        foreach (Server server in servers)
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
}