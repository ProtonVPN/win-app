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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.Logic.Searches.Tests;

[TestClass]
public partial class GlobalSearchTest
{
    private IServersLoader? _serversLoader;
    private ILocalizationProvider? _localizationProvider;
    private GlobalSearch? _globalSearch;

    [TestInitialize]
    public void Initialize()
    {
        _serversLoader = Substitute.For<IServersLoader>();

        List<Server> standardServers = new()
        {
            CreateMockOfServer("DZ#123", "Argel", "", "DZ", default(ServerFeatures)),
            CreateMockOfServer("CA#234", "Ottawa", "", "CA", default(ServerFeatures)),
            CreateMockOfServer("US-AK#345", "Anchorage", "Alaska", "US", default(ServerFeatures)),
            CreateMockOfServer("US-FL#456", "Jacksonville", "Florida", "US", default(ServerFeatures)),
            CreateMockOfServer("US-MI#567", "Detroit", "Michigan", "US", default(ServerFeatures)),
            CreateMockOfServer("CH#678", "Zurich", "", "CH", default(ServerFeatures)),
            CreateMockOfServer("CL#789", "Santiago", "", "CL", default(ServerFeatures)),
        };
        List<Server> secureCoreServers = new()
        {
            CreateMockOfServer("US-WI#123", "Milwaukee", "Wisconsin", "US", ServerFeatures.SecureCore),
            CreateMockOfServer("US-IL#234", "Chicago", "Illinois", "US", ServerFeatures.SecureCore),
            CreateMockOfServer("IS#345", "Reykjavík", "", "IS", ServerFeatures.SecureCore),
            CreateMockOfServer("PK#456", "Islamabad", "", "PK", ServerFeatures.SecureCore),
        };
        _serversLoader.GetServers().Returns(standardServers.Concat(secureCoreServers));
        _serversLoader.GetServersByFeatures(Arg.Is(ServerFeatures.SecureCore)).Returns(secureCoreServers);

        List<City> standardCities = new()
        {
            CreateMockOfCity("Argel", "", "DZ", default(ServerFeatures)),
            CreateMockOfCity("Ottawa", "", "CA", default(ServerFeatures)),
            CreateMockOfCity("Anchorage", "Alaska", "US", default(ServerFeatures)),
            CreateMockOfCity("Jacksonville", "Florida", "US", default(ServerFeatures)),
            CreateMockOfCity("Detroit", "Michigan", "US", default(ServerFeatures)),
            CreateMockOfCity("Zurich", "", "CH", default(ServerFeatures)),
            CreateMockOfCity("Santiago", "", "CL", default(ServerFeatures)),
        };
        List<City> secureCoreCities = new()
        {
            CreateMockOfCity("Milwaukee", "Wisconsin", "US", ServerFeatures.SecureCore),
            CreateMockOfCity("Chicago", "Illinois", "US", ServerFeatures.SecureCore),
            CreateMockOfCity("Reykjavík", "", "IS", ServerFeatures.SecureCore),
            CreateMockOfCity("Islamabad", "", "PK", ServerFeatures.SecureCore),
        };
        _serversLoader.GetCities().Returns(standardCities.Concat(secureCoreCities));
        _serversLoader.GetCitiesByFeatures(Arg.Is(ServerFeatures.SecureCore)).Returns(secureCoreCities);

        List<State> standardStates = new()
        {
            CreateMockOfState("Alaska", "US", default(ServerFeatures)),
            CreateMockOfState("Florida", "US", default(ServerFeatures)),
            CreateMockOfState("Michigan", "US", default(ServerFeatures)),
        };
        List<State> secureCoreStates = new()
        {
            CreateMockOfState("Wisconsin", "US", ServerFeatures.SecureCore),
            CreateMockOfState("Illinois", "US", ServerFeatures.SecureCore),
        };
        _serversLoader.GetStates().Returns(standardStates.Concat(secureCoreStates));
        _serversLoader.GetStatesByFeatures(Arg.Is(ServerFeatures.SecureCore)).Returns(secureCoreStates);

        List<Country> standardCountries = new()
        {
            CreateMockOfCountry("AE", default(ServerFeatures)),
            CreateMockOfCountry("DZ", default(ServerFeatures)),
            CreateMockOfCountry("CA", default(ServerFeatures)),
            CreateMockOfCountry("US", default(ServerFeatures)),
            CreateMockOfCountry("CH", default(ServerFeatures)),
            CreateMockOfCountry("CL", default(ServerFeatures)),
        };
        List<Country> secureCoreCountries = new()
        {
            CreateMockOfCountry("IS", ServerFeatures.SecureCore),
            CreateMockOfCountry("PK", default(ServerFeatures)),
        };
        _serversLoader.GetCountries().Returns(standardCountries.Concat(secureCoreCountries));
        _serversLoader.GetCountriesByFeatures(Arg.Is(ServerFeatures.SecureCore)).Returns(secureCoreCountries);


        _localizationProvider = Substitute.For<ILocalizationProvider>();
        _localizationProvider.Get("Country_val_AE").Returns("United Arab Emirates");
        _localizationProvider.Get("Country_val_DZ").Returns("Algeria");
        _localizationProvider.Get("Country_val_CA").Returns("Canada");
        _localizationProvider.Get("Country_val_US").Returns("United States");
        _localizationProvider.Get("Country_val_CH").Returns("Switzerland");
        _localizationProvider.Get("Country_val_CL").Returns("Chile");

        _localizationProvider.Get("Country_val_IS").Returns("Iceland");
        _localizationProvider.Get("Country_val_PK").Returns("Pakistan");


        _globalSearch = new GlobalSearch(_serversLoader, _localizationProvider);
    }

    private Server CreateMockOfServer(string name, string city, string state, string countryCode, ServerFeatures features)
    {
        return new Server()
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            City = city,
            State = state,
            EntryCountry = countryCode,
            ExitCountry = countryCode,
            HostCountry = "",
            Domain = "test",
            Latitude = 0,
            Longitude = 0,
            GatewayName = "",
            Servers = [],
            Features = features
        };
    }

    private City CreateMockOfCity(string name, string stateName, string countryCode, ServerFeatures features)
    {
        return new City()
        {
            Name = name,
            StateName = stateName,
            CountryCode = countryCode,
            IsStandardUnderMaintenance = false,
            IsP2PUnderMaintenance = false,
            IsSecureCoreUnderMaintenance = false,
            IsTorUnderMaintenance = false,
            Features = features,
        };
    }

    private State CreateMockOfState(string name, string countryCode, ServerFeatures features)
    {
        return new State()
        {
            Name = name,
            CountryCode = countryCode,
            IsStandardUnderMaintenance = false,
            IsP2PUnderMaintenance = false,
            IsSecureCoreUnderMaintenance = false,
            IsTorUnderMaintenance = false,
            Features = features,
        };
    }

    private Country CreateMockOfCountry(string countryCode, ServerFeatures features)
    {
        return new Country()
        {
            Code = countryCode,
            IsStandardUnderMaintenance = false,
            IsP2PUnderMaintenance = false,
            IsSecureCoreUnderMaintenance = false,
            IsTorUnderMaintenance = false,
            Features = features,
        };
    }

    [TestCleanup]
    public void Cleanup()
    {
        _serversLoader = null;
        _localizationProvider = null;
        _globalSearch = null;
    }
}