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
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ApiLogs;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Servers;

public class ServersCache : IServersCache
{
    private readonly IApiClient _apiClient;
    private readonly IEntityMapper _entityMapper;
    private readonly IServersFileReaderWriter _serversFileReaderWriter;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly ISettings _settings;
    private readonly ILogger _logger;

    private readonly ReaderWriterLockSlim _lock = new();

    private IReadOnlyList<Server> _originalServers = new List<Server>();

    private IReadOnlyList<Server> _filteredServers = new List<Server>();
    public IReadOnlyList<Server> Servers => GetWithReadLock(() => _filteredServers);

    private IReadOnlyList<string> _countryCodes = new List<string>();
    public IReadOnlyList<string> CountryCodes => GetWithReadLock(() => _countryCodes);

    private IReadOnlyList<string> _gateways = new List<string>();
    public IReadOnlyList<string> Gateways => GetWithReadLock(() => _gateways);

    public ServersCache(IApiClient apiClient,
        IEntityMapper entityMapper,
        IServersFileReaderWriter serversFileReaderWriter,
        IEventMessageSender eventMessageSender, 
        ISettings settings,
        ILogger logger)
    {
        _apiClient = apiClient;
        _entityMapper = entityMapper;
        _serversFileReaderWriter = serversFileReaderWriter;
        _eventMessageSender = eventMessageSender;
        _settings = settings;
        _logger = logger;
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
        if (!HasAnyServers())
        {
            _logger.Info<AppLog>("Loading servers from file as the user has none.");
            IReadOnlyList<Server> servers = _serversFileReaderWriter.Read();
            ProcessServers(servers);
        }
    }

    public async Task UpdateAsync()
    {
        LoadFromFileIfEmpty();
        try
        {
            DeviceLocation? currentLocation = _settings.DeviceLocation;

            ApiResponseResult<ServersResponse> response = await _apiClient.GetServersAsync(currentLocation?.IpAddress ?? string.Empty);
            if (response.LastModified.HasValue)
            {
                _settings.LogicalsLastModifiedDate = response.LastModified.Value;
            }
            if (response.Success && !response.IsNotModified)
            {
                List<Server> servers = _entityMapper.Map<LogicalServerResponse, Server>(response.Value.Servers);
                SaveToFile(servers);
                ProcessServers(servers);
            }
        }
        catch (Exception e)
        {
            _logger.Error<ApiErrorLog>("API: Get servers failed", e);
        }
    }

    public async Task UpdateLoadsAsync()
    {
        if (!HasAnyServers())
        {
            return;
        }

        try
        {
            DeviceLocation? currentLocation = _settings.DeviceLocation;

            ApiResponseResult<ServersResponse> response = await _apiClient.GetServerLoadsAsync(currentLocation?.IpAddress ?? string.Empty);
            if (response.Success)
            {
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

                SaveToFile(servers);
                ProcessServers(servers);
            }
        }
        catch (Exception e)
        {
            _logger.Error<ApiErrorLog>("API: Get servers load failed", e);
        }
    }

    private void ProcessServers(IReadOnlyList<Server> servers)
    {
        if (servers is not null && servers.Any())
        {
            IReadOnlyList<string> countryCodes = GetCountryCodes(servers);
            IReadOnlyList<string> gateways = GetGateways(servers);
            IReadOnlyList<Server> filteredServers = GetFilteredServers(servers);

            _lock.EnterWriteLock();
            try
            {
                _originalServers = servers;
                _filteredServers = filteredServers;
                _countryCodes = countryCodes;
                _gateways = gateways;
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            _eventMessageSender.Send(new ServerListChangedMessage());
        }
    }

    private IReadOnlyList<string> GetCountryCodes(IEnumerable<Server> servers)
    {
        return servers.Select(s => s.ExitCountry).Distinct().ToList();
    }

    private IReadOnlyList<string> GetGateways(IReadOnlyList<Server> servers)
    {
        return servers
            .Where(s => s.Features.IsSupported(ServerFeatures.B2B) && !string.IsNullOrWhiteSpace(s.GatewayName))
            .Select(s => s.GatewayName)
            .Distinct()
            .ToList();
    }

    private IReadOnlyList<Server> GetFilteredServers(IReadOnlyList<Server> servers)
    {
        List<Server> filteredServers = [];
        foreach (Server server in servers)
        {
            if (_settings.VpnPlan.MaxTier >= (sbyte)server.Tier)
            {
                filteredServers.Add(server);
            }
            else if (server.Tier <= ServerTiers.Plus)
            {
                filteredServers.Add(server.CopyWithoutPhysicalServers());
            }
        }
        return filteredServers;
    }

    private void SaveToFile(IList<Server> servers)
    {
        _serversFileReaderWriter.Save(servers);
    }

    public bool HasAnyServers()
    {
        return Servers is not null && Servers.Count > 0;
    }

    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            _originalServers = new List<Server>();
            _filteredServers = new List<Server>();
            _countryCodes = new List<string>();
            _gateways = new List<string>();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void ReprocessServers()
    {
        _logger.Info<AppLog>("Reprocessing servers.");
        ProcessServers(_originalServers);
    }
}