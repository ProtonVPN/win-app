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
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Files;
using ProtonVPN.EntityMapping.Contracts;

namespace ProtonVPN.Client.Logic.Servers;

public class ServersUpdater : IServersUpdater, IServersCache, IEventMessageReceiver<LoggedInMessage>
{
    private readonly IApiClient _apiClient;
    private readonly IEntityMapper _entityMapper;
    private readonly IServersFileManager _serversFileManager;
    private readonly IEventMessageSender _eventMessageSender;

    private readonly ReaderWriterLockSlim _lock = new();

    private IReadOnlyList<Server> _servers = new List<Server>();
    public IReadOnlyList<Server> Servers => GetWithReadLock(() => _servers);

    private IReadOnlyList<string> _countryCodes = new List<string>();
    public IReadOnlyList<string> CountryCodes => GetWithReadLock(() => _countryCodes);

    private IReadOnlyList<string> _gateways = new List<string>();
    public IReadOnlyList<string> Gateways => GetWithReadLock(() => _gateways);

    public ServersUpdater(IApiClient apiClient,
        IEntityMapper entityMapper,
        IServersFileManager serversFileManager,
        IEventMessageSender eventMessageSender)
    {
        _apiClient = apiClient;
        _entityMapper = entityMapper;
        _serversFileManager = serversFileManager;
        _eventMessageSender = eventMessageSender;
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

    public async void Receive(LoggedInMessage message)
    {
        await UpdateAsync();
    }

    public async Task UpdateAsync()
    {
        if (Servers is null || Servers.Count == 0)
        {
            IReadOnlyList<Server> servers = _serversFileManager.Read();
            ProcessNewServers(servers);
        }
        try
        {
            ApiResponseResult<ServersResponse> response = await _apiClient.GetServersAsync(string.Empty); // TODO: Use IP address here
            if (response.Success)
            {
                List<Server> servers = _entityMapper.Map<LogicalServerResponse, Server>(response.Value.Servers);
                SaveToFile(servers);
                ProcessNewServers(servers);
            }
        }
        catch
        {
        }
    }

    private void ProcessNewServers(IReadOnlyList<Server> servers)
    {
        if (servers is not null && servers.Any())
        {
            IReadOnlyList<string> countryCodes = GetCountryCodes(servers);
            IReadOnlyList<string> gateways = GetGateways(servers);

            _lock.EnterWriteLock();
            try
            {
                _servers = servers;
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
        return servers.Where(s => s.Features.IsSupported(ServerFeatures.B2B) && !string.IsNullOrWhiteSpace(s.GatewayName))
            .Select(s => s.GatewayName).Distinct().ToList();
    }

    private void SaveToFile(IList<Server> servers)
    {
        _serversFileManager.Save(servers);
    }
}