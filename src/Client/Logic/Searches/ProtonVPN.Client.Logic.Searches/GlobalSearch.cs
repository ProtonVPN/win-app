/*
 * Copyright (c) 2024 Proton AG
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

using System.Text.RegularExpressions;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Searches.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.Logic.Searches;

public partial class GlobalSearch : IGlobalSearch
{
    [GeneratedRegex(@"^.{2,}(#\d*|\d+)$")]
    private static partial Regex GenerateCompileTimeRegex();
    private readonly Regex _serverSearchTriggerRegex = GenerateCompileTimeRegex();

    private readonly IServersLoader _serversLoader;
    //private readonly IProfilesManager _profilesManager;
    private readonly ILocalizationProvider _localizer;

    public GlobalSearch(IServersLoader serversLoader,
        //IProfilesManager profilesManager,
        ILocalizationProvider localizer)
    {
        _serversLoader = serversLoader;
        //_profilesManager = profilesManager;
        _localizer = localizer;
    }

    public async Task<List<ILocation>> SearchAsync(string? input, ServerFeatures? serverFeatures = null)
    {
        input = input?.Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            return [];
        }

        input = input.RemoveDiacritics();

        Task<IEnumerable<ILocation>> serversTask;
        if (_serverSearchTriggerRegex.IsMatch(input))
        {
            serversTask = Task.Run(() => SearchServers(input, serverFeatures));
        }
        else
        {
            serversTask = Task.FromResult<IEnumerable<ILocation>>([]);
        }

        Func<string, string, bool> searchFunc = GetSearchFunction(input);
        Task<IEnumerable<ILocation>> citiesTask = Task.Run(() => SearchCities(input, serverFeatures, searchFunc));
        Task<IEnumerable<ILocation>> statesTask = Task.Run(() => SearchStates(input, serverFeatures, searchFunc));
        Task<IEnumerable<ILocation>> countriesTask = Task.Run(() => SearchCountries(input, serverFeatures, searchFunc));
        //Task<IEnumerable<IConnectionIntent>> gatewaysTask = Task.Run(() => SearchGateways(input));
        //Task<IEnumerable<IConnectionIntent>> profilesTask = Task.Run(() => SearchProfiles(input));

        Task.WaitAll(serversTask, citiesTask, statesTask, countriesTask/*, gatewaysTask, profilesTask*/);

        IEnumerable<ILocation> servers = await serversTask;
        IEnumerable<ILocation> cities = await citiesTask;
        IEnumerable<ILocation> states = await statesTask;
        IEnumerable<ILocation> countries = await countriesTask;
        //IEnumerable<IConnectionIntent> gateways = await gatewaysTask;
        //IEnumerable<IConnectionIntent> profiles = await profilesTask;

        //return profiles.Concat(gateways).Concat(countries).Concat(states).Concat(cities).Concat(servers).ToList();
        return countries.Concat(states).Concat(cities).Concat(servers).ToList();
    }

    private IEnumerable<ILocation> SearchServers(string input, ServerFeatures? serverFeatures)
    {
        IEnumerable<Server> servers = serverFeatures is null
            ? _serversLoader.GetServers()
            : _serversLoader.GetServersByFeatures(serverFeatures.Value);
        return servers.Where(s => StartsWith(s.Name, input) || StartsWith(s.Name.Replace("#", ""), input));
    }    

    private Func<string, string, bool> GetSearchFunction(string input)
    {
        return input.Length < SearchConfiguration.MIN_CONTAINS_LENGTH
            ? StartsWith
            : Contains;
    }

    private static bool StartsWith(string name, string input)
    {
        return name?.RemoveDiacritics().StartsWith(input, StringComparison.InvariantCultureIgnoreCase) ?? false;
    }

    private static bool Contains(string name, string input)
    {
        return name?.RemoveDiacritics().Contains(input, StringComparison.InvariantCultureIgnoreCase) ?? false;
    }

    private IEnumerable<ILocation> SearchCities(string input, ServerFeatures? serverFeatures, Func<string, string, bool> searchFunc)
    {
        IEnumerable<City> cities = serverFeatures is null
            ? _serversLoader.GetCities()
            : _serversLoader.GetCitiesByFeatures(serverFeatures.Value);

        List<LocalizedLocation> localizedCities = cities.Select(city => new LocalizedLocation()
        {
            Location = city,
            LocalizedName = _localizer.GetCityName(city.Name, city.CountryCode)
        }).ToList();

        return localizedCities
            .Where(c => searchFunc(c.LocalizedName, input))
            .Select(c => c.Location);
    }

    private IEnumerable<ILocation> SearchStates(string input, ServerFeatures? serverFeatures, Func<string, string, bool> searchFunc)
    {
        IEnumerable<State> states = serverFeatures is null
            ? _serversLoader.GetStates()
            : _serversLoader.GetStatesByFeatures(serverFeatures.Value);

        List<LocalizedLocation> localizedStates = states.Select(state => new LocalizedLocation()
        {
            Location = state,
            LocalizedName = _localizer.GetStateName(state.Name, state.CountryCode)
        }).ToList();

        return localizedStates
            .Where(s => searchFunc(s.LocalizedName, input))
            .Select(s => s.Location);
    }

    private IEnumerable<ILocation> SearchCountries(string input, ServerFeatures? serverFeatures, Func<string, string, bool> searchFunc)
    {
        IEnumerable<ICountryLocation> countries = serverFeatures is null
            ? _serversLoader.GetCountries()
            : _serversLoader.GetCountriesByFeatures(serverFeatures.Value);

        List<LocalizedCountry> localizedCountries = countries.Select(c => new LocalizedCountry()
        {
            Country = c,
            LocalizedName = _localizer.GetCountryName(c.Code)
        }).ToList();

        return localizedCountries
            .Where(c => searchFunc(c.Country.Code, input) || searchFunc(c.LocalizedName, input))
            .Select(c => c.Country);
    }

    //private IEnumerable<ILocation> SearchGateways(string input)
    //{
    //    IEnumerable<string> gateways = _serversLoader.GetGateways();
    //    return gateways.Where(g => IsAMatch(g, input))
    //        .Select(g => new ConnectionIntent(new GatewayLocationIntent(g)));
    //}

    //private IEnumerable<ILocation> SearchProfiles(string input)
    //{
    //    IOrderedEnumerable<IConnectionProfile> profiles = _profilesManager.GetAll();
    //    return profiles.Where(p => IsAMatch(p.Name, input));
    //}
}