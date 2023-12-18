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
using ProtonVPN.EntityMapping.Contracts;

namespace ProtonVPN.Client.Logic.Servers;

public class ServersUpdater : IServersUpdater, IServersCache, IEventMessageReceiver<LoggedInMessage>
{
    private readonly IApiClient _apiClient;
    private readonly IEntityMapper _entityMapper;
    private readonly IEventMessageSender _eventMessageSender;

    public IReadOnlyList<Server> Servers { get; private set; } = new List<Server>();
    public IReadOnlyList<string> CountryCodes { get; private set; } = new List<string>();
    public IReadOnlyList<string> Gateways { get; private set; } = new List<string>();

    public ServersUpdater(IApiClient apiClient, IEntityMapper entityMapper, IEventMessageSender eventMessageSender)
    {
        _apiClient = apiClient;
        _entityMapper = entityMapper;
        _eventMessageSender = eventMessageSender;
    }

    public async void Receive(LoggedInMessage message)
    {
        await UpdateAsync();
    }

    public async Task UpdateAsync()
    {
        try
        {
            ApiResponseResult<ServersResponse> response = await _apiClient.GetServersAsync(string.Empty); // TODO: Use IP address here
            if (response.Success)
            {
                IReadOnlyList<Server> servers = _entityMapper.Map<LogicalServerResponse, Server>(response.Value.Servers);

                Servers = servers;

                UpdateCountryCodes();
                UpdateGateways();

                _eventMessageSender.Send(new ServerListChangedMessage());
            }
        }
        catch
        { }
    }

    private void UpdateCountryCodes()
    {
        CountryCodes = Servers
            .Where(s => !s.Features.IsSupported(ServerFeatures.B2B) && IsCountryValid(s.ExitCountry))
            .Select(s => s.ExitCountry)
            .Distinct()
            .ToList();
    }

    private void UpdateGateways()
    {
        Gateways = Servers
            .Where(s => s.Features.IsSupported(ServerFeatures.B2B))
            .Select(s => s.GatewayName)
            .Distinct()
            .ToList();
    }

    private bool IsCountryValid(string countryCode)
    {
        if (countryCode.Equals("AA") || countryCode.Equals("ZZ") || countryCode.StartsWith("X"))
        {
            return false;
        }

        string[] letters = { "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        if (countryCode.StartsWith("Q") && letters.Contains(countryCode.Substring(1, 1)))
        {
            return false;
        }

        return true;
    }
}