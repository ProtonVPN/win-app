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

using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Cities;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Gateways;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.GatewayServers;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Servers;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.States;

namespace ProtonVPN.Client.Logic.Connection.Tests.Models.Intents.Locations;

[TestClass]
public class LocationIntentEqualityTests
{
    [TestMethod]
    public void LocationIntent_ShouldBeEqual_GivenSameReference()
    {
        ILocationIntent singleCountry = SingleCountryLocationIntent.From("CH");
        ILocationIntent singleState = SingleStateLocationIntent.From("US", "CA");
        ILocationIntent singleCityFromState = SingleCityLocationIntent.From("US", "CA", "Los Angeles");
        ILocationIntent singleCity = SingleCityLocationIntent.From("CH", "Geneva");
        ILocationIntent singleServerFromState = SingleServerLocationIntent.From("US", "CA", "Los Angeles", ServerInfo.From("1", "US-CA#1"));
        ILocationIntent singleServer = SingleServerLocationIntent.From("CH", "Geneva", ServerInfo.From("1", "CH#1"));
        ILocationIntent singleGateway = SingleGatewayLocationIntent.From("PROTON");
        ILocationIntent singleGatewayServer = SingleGatewayServerLocationIntent.From("PROTON", GatewayServerInfo.From("1", "PROTON-CH#1", "CH"));

        ILocationIntent multiCountry = MultiCountryLocationIntent.Default;
        ILocationIntent multiState = MultiStateLocationIntent.From("US", ["CA", "AZ", "NY"], SelectionStrategy.Fastest);
        ILocationIntent multiCityFromState = MultiCityLocationIntent.From("US", "CA", ["Los Angeles", "San Francisco"], SelectionStrategy.Fastest);
        ILocationIntent multiCity = MultiCityLocationIntent.From("CH", ["Geneva", "Zurich"], SelectionStrategy.Fastest);
        ILocationIntent multiServerFromState = MultiServerLocationIntent.From("US", "CA", "Los Angeles", [ServerInfo.From("1", "US-CA#1"), ServerInfo.From("2", "US-CA#2")], SelectionStrategy.Fastest);
        ILocationIntent multiServer = MultiServerLocationIntent.From("CH", "Geneva", [ServerInfo.From("1", "CH#1"), ServerInfo.From("2", "CH#2")], SelectionStrategy.Fastest);
        ILocationIntent multiGateway = MultiGatewayLocationIntent.From(["PROTON", "GUEST"], SelectionStrategy.Fastest);
        ILocationIntent multiGatewayServer = MultiGatewayServerLocationIntent.From("PROTON", [GatewayServerInfo.From("1", "PROTON-CH#1", "CH"), GatewayServerInfo.From("2", "PROTON-CH#2", "CH")], SelectionStrategy.Fastest);

        Assert.IsTrue(singleCountry.IsSameAs(singleCountry));
        Assert.IsTrue(singleState.IsSameAs(singleState));
        Assert.IsTrue(singleCityFromState.IsSameAs(singleCityFromState));
        Assert.IsTrue(singleCity.IsSameAs(singleCity));
        Assert.IsTrue(singleServerFromState.IsSameAs(singleServerFromState));
        Assert.IsTrue(singleServer.IsSameAs(singleServer));
        Assert.IsTrue(singleGateway.IsSameAs(singleGateway));
        Assert.IsTrue(singleGatewayServer.IsSameAs(singleGatewayServer));
        Assert.IsTrue(multiCountry.IsSameAs(multiCountry));
        Assert.IsTrue(multiState.IsSameAs(multiState));
        Assert.IsTrue(multiCityFromState.IsSameAs(multiCityFromState));
        Assert.IsTrue(multiCity.IsSameAs(multiCity));
        Assert.IsTrue(multiServerFromState.IsSameAs(multiServerFromState));
        Assert.IsTrue(multiServer.IsSameAs(multiServer));
        Assert.IsTrue(multiGateway.IsSameAs(multiGateway));
        Assert.IsTrue(multiGatewayServer.IsSameAs(multiGatewayServer));
    }

    [TestMethod]
    public void LocationIntent_ShouldBeEqual_GivenDuplicatedIntents()
    {
        ILocationIntent singleCountryA = SingleCountryLocationIntent.From("CH");
        ILocationIntent singleCountryB = SingleCountryLocationIntent.From("CH");
        ILocationIntent singleStateA = SingleStateLocationIntent.From("US", "CA");
        ILocationIntent singleStateB = SingleStateLocationIntent.From("US", "CA");
        ILocationIntent singleCityFromStateA = SingleCityLocationIntent.From("US", "CA", "Los Angeles");
        ILocationIntent singleCityFromStateB = SingleCityLocationIntent.From("US", "CA", "Los Angeles");
        ILocationIntent singleCityA = SingleCityLocationIntent.From("CH", "Geneva");
        ILocationIntent singleCityB = SingleCityLocationIntent.From("CH", "Geneva");
        ILocationIntent singleServerFromStateA = SingleServerLocationIntent.From("US", "CA", "Los Angeles", ServerInfo.From("1", "US-CA#1"));
        ILocationIntent singleServerFromStateB = SingleServerLocationIntent.From("US", "CA", "Los Angeles", ServerInfo.From("1", "US-CA#1"));
        ILocationIntent singleServerA = SingleServerLocationIntent.From("CH", "Geneva", ServerInfo.From("1", "CH#1"));
        ILocationIntent singleServerB = SingleServerLocationIntent.From("CH", "Geneva", ServerInfo.From("1", "CH#1"));
        ILocationIntent singleGatewayA = SingleGatewayLocationIntent.From("PROTON");
        ILocationIntent singleGatewayB = SingleGatewayLocationIntent.From("PROTON");
        ILocationIntent singleGatewayServerA = SingleGatewayServerLocationIntent.From("PROTON", GatewayServerInfo.From("1", "PROTON-CH#1", "CH"));
        ILocationIntent singleGatewayServerB = SingleGatewayServerLocationIntent.From("PROTON", GatewayServerInfo.From("1", "PROTON-CH#1", "CH"));

        ILocationIntent genericCountryA = MultiCountryLocationIntent.Default;
        ILocationIntent genericCountryB = MultiCountryLocationIntent.Default;
        ILocationIntent multiCountryA = MultiCountryLocationIntent.From(["CH", "FR", "US"], SelectionStrategy.Fastest);
        ILocationIntent multiCountryB = MultiCountryLocationIntent.From(["CH", "FR", "US"], SelectionStrategy.Fastest);
        ILocationIntent multiStateA = MultiStateLocationIntent.From("US", ["CA", "AZ", "NY"], SelectionStrategy.Fastest);
        ILocationIntent multiStateB = MultiStateLocationIntent.From("US", ["CA", "AZ", "NY"], SelectionStrategy.Fastest);
        ILocationIntent multiCityFromStateA = MultiCityLocationIntent.From("US", "CA", ["Los Angeles", "San Francisco"], SelectionStrategy.Fastest);
        ILocationIntent multiCityFromStateB = MultiCityLocationIntent.From("US", "CA", ["Los Angeles", "San Francisco"], SelectionStrategy.Fastest);
        ILocationIntent multiCityA = MultiCityLocationIntent.From("CH", ["Geneva", "Zurich"], SelectionStrategy.Fastest);
        ILocationIntent multiCityB = MultiCityLocationIntent.From("CH", ["Geneva", "Zurich"], SelectionStrategy.Fastest);
        ILocationIntent multiServerFromStateA = MultiServerLocationIntent.From("US", "CA", "Los Angeles", [ServerInfo.From("1", "US-CA#1"), ServerInfo.From("2", "US-CA#2")], SelectionStrategy.Fastest);
        ILocationIntent multiServerFromStateB = MultiServerLocationIntent.From("US", "CA", "Los Angeles", [ServerInfo.From("1", "US-CA#1"), ServerInfo.From("2", "US-CA#2")], SelectionStrategy.Fastest);
        ILocationIntent multiServerA = MultiServerLocationIntent.From("CH", "Geneva", [ServerInfo.From("1", "CH#1"), ServerInfo.From("2", "CH#2")], SelectionStrategy.Fastest);
        ILocationIntent multiServerB = MultiServerLocationIntent.From("CH", "Geneva", [ServerInfo.From("1", "CH#1"), ServerInfo.From("2", "CH#2")], SelectionStrategy.Fastest);
        ILocationIntent multiGatewayA = MultiGatewayLocationIntent.From(["PROTON", "GUEST"], SelectionStrategy.Fastest);
        ILocationIntent multiGatewayB = MultiGatewayLocationIntent.From(["PROTON", "GUEST"], SelectionStrategy.Fastest);
        ILocationIntent multiGatewayServerA = MultiGatewayServerLocationIntent.From("PROTON", [GatewayServerInfo.From("1", "PROTON-CH#1", "CH"), GatewayServerInfo.From("2", "PROTON-CH#2", "CH")], SelectionStrategy.Fastest);
        ILocationIntent multiGatewayServerB = MultiGatewayServerLocationIntent.From("PROTON", [GatewayServerInfo.From("1", "PROTON-CH#1", "CH"), GatewayServerInfo.From("2", "PROTON-CH#2", "CH")], SelectionStrategy.Fastest);

        Assert.IsTrue(singleCountryA.IsSameAs(singleCountryB));
        Assert.IsTrue(singleStateA.IsSameAs(singleStateB));
        Assert.IsTrue(singleCityFromStateA.IsSameAs(singleCityFromStateB));
        Assert.IsTrue(singleCityA.IsSameAs(singleCityB));
        Assert.IsTrue(singleServerFromStateA.IsSameAs(singleServerFromStateB));
        Assert.IsTrue(singleServerA.IsSameAs(singleServerB));
        Assert.IsTrue(singleGatewayA.IsSameAs(singleGatewayB));
        Assert.IsTrue(singleGatewayServerA.IsSameAs(singleGatewayServerB));
        Assert.IsTrue(genericCountryA.IsSameAs(genericCountryB));
        Assert.IsTrue(multiCountryA.IsSameAs(multiCountryB));
        Assert.IsTrue(multiStateA.IsSameAs(multiStateB));
        Assert.IsTrue(multiCityFromStateA.IsSameAs(multiCityFromStateB));
        Assert.IsTrue(multiCityA.IsSameAs(multiCityB));
        Assert.IsTrue(multiServerFromStateA.IsSameAs(multiServerFromStateB));
        Assert.IsTrue(multiServerA.IsSameAs(multiServerB));
        Assert.IsTrue(multiGatewayA.IsSameAs(multiGatewayB));
        Assert.IsTrue(multiGatewayServerA.IsSameAs(multiGatewayServerB));
    }

    [TestMethod]
    public void LocationIntent_ShouldBeEqual_GivenSimilarIntent()
    {
        ILocationIntent fastestCountryA = MultiCountryLocationIntent.Default;
        ILocationIntent fastestCountryB = MultiCountryLocationIntent.Fastest;
        ILocationIntent fastestCountryC = MultiCountryLocationIntent.FastestFrom([]);
        ILocationIntent fastestCountryD = MultiCountryLocationIntent.From([], SelectionStrategy.Fastest);
        ILocationIntent fastestCountryE = new MultiCountryLocationIntent();
        ILocationIntent fastestCountryF = new MultiCountryLocationIntent([]);
        ILocationIntent fastestCountryG = new MultiCountryLocationIntent([], SelectionStrategy.Fastest);
        ILocationIntent fastestCountryH = new MultiCountryLocationIntent(SelectionStrategy.Fastest);

        Assert.IsTrue(fastestCountryA.IsSameAs(fastestCountryB));
        Assert.IsTrue(fastestCountryA.IsSameAs(fastestCountryC));
        Assert.IsTrue(fastestCountryA.IsSameAs(fastestCountryD));
        Assert.IsTrue(fastestCountryA.IsSameAs(fastestCountryE));
        Assert.IsTrue(fastestCountryA.IsSameAs(fastestCountryF));
        Assert.IsTrue(fastestCountryA.IsSameAs(fastestCountryG));
        Assert.IsTrue(fastestCountryA.IsSameAs(fastestCountryH));

        Assert.IsTrue(fastestCountryB.IsSameAs(fastestCountryA));
        Assert.IsTrue(fastestCountryB.IsSameAs(fastestCountryC));
        Assert.IsTrue(fastestCountryB.IsSameAs(fastestCountryD));
        Assert.IsTrue(fastestCountryB.IsSameAs(fastestCountryE));
        Assert.IsTrue(fastestCountryB.IsSameAs(fastestCountryF));
        Assert.IsTrue(fastestCountryB.IsSameAs(fastestCountryG));
        Assert.IsTrue(fastestCountryB.IsSameAs(fastestCountryH));

        Assert.IsTrue(fastestCountryC.IsSameAs(fastestCountryA));
        Assert.IsTrue(fastestCountryC.IsSameAs(fastestCountryB));
        Assert.IsTrue(fastestCountryC.IsSameAs(fastestCountryD));
        Assert.IsTrue(fastestCountryC.IsSameAs(fastestCountryE));
        Assert.IsTrue(fastestCountryC.IsSameAs(fastestCountryF));
        Assert.IsTrue(fastestCountryC.IsSameAs(fastestCountryG));
        Assert.IsTrue(fastestCountryC.IsSameAs(fastestCountryH));

        Assert.IsTrue(fastestCountryD.IsSameAs(fastestCountryA));
        Assert.IsTrue(fastestCountryD.IsSameAs(fastestCountryB));
        Assert.IsTrue(fastestCountryD.IsSameAs(fastestCountryC));
        Assert.IsTrue(fastestCountryD.IsSameAs(fastestCountryE));
        Assert.IsTrue(fastestCountryD.IsSameAs(fastestCountryF));
        Assert.IsTrue(fastestCountryD.IsSameAs(fastestCountryG));
        Assert.IsTrue(fastestCountryD.IsSameAs(fastestCountryH));

        Assert.IsTrue(fastestCountryE.IsSameAs(fastestCountryA));
        Assert.IsTrue(fastestCountryE.IsSameAs(fastestCountryB));
        Assert.IsTrue(fastestCountryE.IsSameAs(fastestCountryC));
        Assert.IsTrue(fastestCountryE.IsSameAs(fastestCountryD));
        Assert.IsTrue(fastestCountryE.IsSameAs(fastestCountryF));
        Assert.IsTrue(fastestCountryE.IsSameAs(fastestCountryG));
        Assert.IsTrue(fastestCountryE.IsSameAs(fastestCountryH));

        Assert.IsTrue(fastestCountryF.IsSameAs(fastestCountryA));
        Assert.IsTrue(fastestCountryF.IsSameAs(fastestCountryB));
        Assert.IsTrue(fastestCountryF.IsSameAs(fastestCountryC));
        Assert.IsTrue(fastestCountryF.IsSameAs(fastestCountryD));
        Assert.IsTrue(fastestCountryF.IsSameAs(fastestCountryE));
        Assert.IsTrue(fastestCountryF.IsSameAs(fastestCountryG));
        Assert.IsTrue(fastestCountryF.IsSameAs(fastestCountryH));

        Assert.IsTrue(fastestCountryG.IsSameAs(fastestCountryA));
        Assert.IsTrue(fastestCountryG.IsSameAs(fastestCountryB));
        Assert.IsTrue(fastestCountryG.IsSameAs(fastestCountryC));
        Assert.IsTrue(fastestCountryG.IsSameAs(fastestCountryD));
        Assert.IsTrue(fastestCountryG.IsSameAs(fastestCountryE));
        Assert.IsTrue(fastestCountryG.IsSameAs(fastestCountryF));
        Assert.IsTrue(fastestCountryG.IsSameAs(fastestCountryH));

        Assert.IsTrue(fastestCountryH.IsSameAs(fastestCountryA));
        Assert.IsTrue(fastestCountryH.IsSameAs(fastestCountryB));
        Assert.IsTrue(fastestCountryH.IsSameAs(fastestCountryC));
        Assert.IsTrue(fastestCountryH.IsSameAs(fastestCountryD));
        Assert.IsTrue(fastestCountryH.IsSameAs(fastestCountryE));
        Assert.IsTrue(fastestCountryH.IsSameAs(fastestCountryF));
        Assert.IsTrue(fastestCountryH.IsSameAs(fastestCountryG));

        ILocationIntent countryA = new SingleCountryLocationIntent("CH");
        ILocationIntent countryB = SingleCountryLocationIntent.From("CH");
        ILocationIntent countryC = SingleCountryLocationIntent.From("ch");

        Assert.IsTrue(countryA.IsSameAs(countryB));
        Assert.IsTrue(countryA.IsSameAs(countryC));
        Assert.IsTrue(countryB.IsSameAs(countryA));
        Assert.IsTrue(countryB.IsSameAs(countryC));
        Assert.IsTrue(countryC.IsSameAs(countryA));
        Assert.IsTrue(countryC.IsSameAs(countryB));

        ILocationIntent multiCountryA = MultiCountryLocationIntent.From(["CH", "FR", "US"], SelectionStrategy.Fastest);
        ILocationIntent multiCountryB = MultiCountryLocationIntent.FastestFrom(["CH", "FR", "US"]);
        ILocationIntent multiCountryC = MultiCountryLocationIntent.FastestFrom(["ch", "fr", "us"]);

        Assert.IsTrue(multiCountryA.IsSameAs(multiCountryB));
        Assert.IsTrue(multiCountryA.IsSameAs(multiCountryC));
        Assert.IsTrue(multiCountryB.IsSameAs(multiCountryA));
        Assert.IsTrue(multiCountryB.IsSameAs(multiCountryC));
        Assert.IsTrue(multiCountryC.IsSameAs(multiCountryA));
        Assert.IsTrue(multiCountryC.IsSameAs(multiCountryB));
    }

    [TestMethod]
    public void LocationIntent_ShouldBeEqual_GivenMultiSelectionWithRandomOrderAndDuplicates()
    {
        List<MultiCountryLocationIntent> countryIntents =
        [
            MultiCountryLocationIntent.FastestFrom(["CH", "FR", "US"]),
            MultiCountryLocationIntent.FastestFrom(["FR", "US", "CH"]),
            MultiCountryLocationIntent.FastestFrom(["FR", "US", "FR", "CH", "US"])
        ];

        foreach (MultiCountryLocationIntent intent in countryIntents)
        {
            Assert.IsFalse(intent.IsSelectionEmpty);
            Assert.AreEqual(3, intent.CountryCodes.Count);
            Assert.AreEqual("CH", intent.CountryCodes[0]);
            Assert.AreEqual("FR", intent.CountryCodes[1]);
            Assert.AreEqual("US", intent.CountryCodes[2]);
        }

        Assert.IsTrue(countryIntents[0].IsSameAs(countryIntents[1]));
        Assert.IsTrue(countryIntents[0].IsSameAs(countryIntents[2]));
        Assert.IsTrue(countryIntents[1].IsSameAs(countryIntents[0]));
        Assert.IsTrue(countryIntents[1].IsSameAs(countryIntents[2]));
        Assert.IsTrue(countryIntents[2].IsSameAs(countryIntents[0]));
        Assert.IsTrue(countryIntents[2].IsSameAs(countryIntents[1]));

        List<MultiStateLocationIntent> stateIntents =
        [
            MultiStateLocationIntent.From("US", ["AZ", "CA", "NY"], SelectionStrategy.Fastest),
            MultiStateLocationIntent.From("US", ["CA", "AZ", "NY"], SelectionStrategy.Fastest),
            MultiStateLocationIntent.From("US", ["CA", "NY", "CA", "AZ", "NY"], SelectionStrategy.Fastest)
        ];

        foreach (MultiStateLocationIntent intent in stateIntents)
        {
            Assert.IsFalse(intent.IsSelectionEmpty);
            Assert.AreEqual(3, intent.StateNames.Count);
            Assert.AreEqual("AZ", intent.StateNames[0]);
            Assert.AreEqual("CA", intent.StateNames[1]);
            Assert.AreEqual("NY", intent.StateNames[2]);
        }

        Assert.IsTrue(stateIntents[0].IsSameAs(stateIntents[1]));
        Assert.IsTrue(stateIntents[0].IsSameAs(stateIntents[2]));
        Assert.IsTrue(stateIntents[1].IsSameAs(stateIntents[0]));
        Assert.IsTrue(stateIntents[1].IsSameAs(stateIntents[2]));
        Assert.IsTrue(stateIntents[2].IsSameAs(stateIntents[0]));
        Assert.IsTrue(stateIntents[2].IsSameAs(stateIntents[1]));

        List<MultiCityLocationIntent> cityIntents =
        [
            MultiCityLocationIntent.From("US", ["Los Angeles", "San Francisco"], SelectionStrategy.Fastest),
            MultiCityLocationIntent.From("US", ["San Francisco", "Los Angeles"], SelectionStrategy.Fastest),
            MultiCityLocationIntent.From("US", ["San Francisco", "Los Angeles", "San Francisco"], SelectionStrategy.Fastest)
        ];

        foreach (MultiCityLocationIntent intent in cityIntents)
        {
            Assert.IsFalse(intent.IsSelectionEmpty);
            Assert.AreEqual(2, intent.CityNames.Count);
            Assert.AreEqual("Los Angeles", intent.CityNames[0]);
            Assert.AreEqual("San Francisco", intent.CityNames[1]);
        }

        Assert.IsTrue(cityIntents[0].IsSameAs(cityIntents[1]));
        Assert.IsTrue(cityIntents[0].IsSameAs(cityIntents[2]));
        Assert.IsTrue(cityIntents[1].IsSameAs(cityIntents[0]));
        Assert.IsTrue(cityIntents[1].IsSameAs(cityIntents[2]));
        Assert.IsTrue(cityIntents[2].IsSameAs(cityIntents[0]));
        Assert.IsTrue(cityIntents[2].IsSameAs(cityIntents[1]));

        List<MultiServerLocationIntent> serverIntents =
        [
            MultiServerLocationIntent.From("CH", "Geneva", [ServerInfo.From("1", "CH#1"), ServerInfo.From("2", "CH#2"), ServerInfo.From("3", "CH#3")], SelectionStrategy.Fastest),
            MultiServerLocationIntent.From("CH", "Geneva", [ServerInfo.From("3", "CH#3"), ServerInfo.From("2", "CH#2"), ServerInfo.From("1", "CH#1")], SelectionStrategy.Fastest),
            MultiServerLocationIntent.From("CH", "Geneva", [ServerInfo.From("1", "CH#1"), ServerInfo.From("3", "CH#3"), ServerInfo.From("2", "CH#2"), ServerInfo.From("1", "CH#1"), ServerInfo.From("3", "CH#3")], SelectionStrategy.Fastest)
        ];

        foreach (MultiServerLocationIntent intent in serverIntents)
        {
            Assert.IsFalse(intent.IsSelectionEmpty);
            Assert.AreEqual(3, intent.Servers.Count);
            Assert.AreEqual("1", intent.Servers[0].Id);
            Assert.AreEqual("CH#1", intent.Servers[0].Name);
            Assert.AreEqual("2", intent.Servers[1].Id);
            Assert.AreEqual("CH#2", intent.Servers[1].Name);
            Assert.AreEqual("3", intent.Servers[2].Id);
            Assert.AreEqual("CH#3", intent.Servers[2].Name);
        }

        Assert.IsTrue(serverIntents[0].IsSameAs(serverIntents[1]));
        Assert.IsTrue(serverIntents[0].IsSameAs(serverIntents[2]));
        Assert.IsTrue(serverIntents[1].IsSameAs(serverIntents[0]));
        Assert.IsTrue(serverIntents[1].IsSameAs(serverIntents[2]));
        Assert.IsTrue(serverIntents[2].IsSameAs(serverIntents[0]));
        Assert.IsTrue(serverIntents[2].IsSameAs(serverIntents[1]));

        List<MultiGatewayLocationIntent> gatewayIntents =
        [
            MultiGatewayLocationIntent.From(["GUEST", "PROTON"], SelectionStrategy.Fastest),
            MultiGatewayLocationIntent.From(["PROTON", "GUEST"], SelectionStrategy.Fastest),
            MultiGatewayLocationIntent.From(["PROTON", "GUEST", "PROTON"], SelectionStrategy.Fastest)
        ];

        foreach (MultiGatewayLocationIntent intent in gatewayIntents)
        {
            Assert.IsFalse(intent.IsSelectionEmpty);
            Assert.AreEqual(2, intent.GatewayNames.Count);
            Assert.AreEqual("GUEST", intent.GatewayNames[0]);
            Assert.AreEqual("PROTON", intent.GatewayNames[1]);
        }

        Assert.IsTrue(gatewayIntents[0].IsSameAs(gatewayIntents[1]));
        Assert.IsTrue(gatewayIntents[0].IsSameAs(gatewayIntents[2]));
        Assert.IsTrue(gatewayIntents[1].IsSameAs(gatewayIntents[0]));
        Assert.IsTrue(gatewayIntents[1].IsSameAs(gatewayIntents[2]));
        Assert.IsTrue(gatewayIntents[2].IsSameAs(gatewayIntents[0]));
        Assert.IsTrue(gatewayIntents[2].IsSameAs(gatewayIntents[1]));

        List<MultiGatewayServerLocationIntent> gatewayServerIntents =
        [
            MultiGatewayServerLocationIntent.From("PROTON", [GatewayServerInfo.From("1", "PROTON-CH#1", "CH"), GatewayServerInfo.From("2", "PROTON-CH#2", "CH"), GatewayServerInfo.From("3", "PROTON-CH#3", "CH")], SelectionStrategy.Fastest),
            MultiGatewayServerLocationIntent.From("PROTON", [GatewayServerInfo.From("3", "PROTON-CH#3", "CH"), GatewayServerInfo.From("2", "PROTON-CH#2", "CH"), GatewayServerInfo.From("1", "PROTON-CH#1", "CH")], SelectionStrategy.Fastest),
            MultiGatewayServerLocationIntent.From("PROTON", [GatewayServerInfo.From("1", "PROTON-CH#1", "CH"), GatewayServerInfo.From("3", "PROTON-CH#3", "CH"), GatewayServerInfo.From("2", "PROTON-CH#2", "CH"), GatewayServerInfo.From("1", "PROTON-CH#1", "CH"), GatewayServerInfo.From("3", "PROTON-CH#3", "CH")], SelectionStrategy.Fastest)
        ];

        foreach (MultiGatewayServerLocationIntent intent in gatewayServerIntents)
        {
            Assert.IsFalse(intent.IsSelectionEmpty);
            Assert.AreEqual(3, intent.Servers.Count);
            Assert.AreEqual("1", intent.Servers[0].Id);
            Assert.AreEqual("PROTON-CH#1", intent.Servers[0].Name);
            Assert.AreEqual("2", intent.Servers[1].Id);
            Assert.AreEqual("PROTON-CH#2", intent.Servers[1].Name);
            Assert.AreEqual("3", intent.Servers[2].Id);
            Assert.AreEqual("PROTON-CH#3", intent.Servers[2].Name);
        }

        Assert.IsTrue(gatewayServerIntents[0].IsSameAs(gatewayServerIntents[1]));
        Assert.IsTrue(gatewayServerIntents[0].IsSameAs(gatewayServerIntents[2]));
        Assert.IsTrue(gatewayServerIntents[1].IsSameAs(gatewayServerIntents[0]));
        Assert.IsTrue(gatewayServerIntents[1].IsSameAs(gatewayServerIntents[2]));
        Assert.IsTrue(gatewayServerIntents[2].IsSameAs(gatewayServerIntents[0]));
        Assert.IsTrue(gatewayServerIntents[2].IsSameAs(gatewayServerIntents[1]));
    }

    [TestMethod]
    public void LocationIntent_ShouldNotBeEqual_GivenDifferentIntentOfSameType()
    {
        ILocationIntent singleCountryA = SingleCountryLocationIntent.From("CH");
        ILocationIntent singleCountryB = SingleCountryLocationIntent.From("US");
        ILocationIntent singleStateA = SingleStateLocationIntent.From("US", "CA");
        ILocationIntent singleStateB = SingleStateLocationIntent.From("US", "AZ");
        ILocationIntent singleCityFromStateA = SingleCityLocationIntent.From("US", "CA", "Los Angeles");
        ILocationIntent singleCityFromStateB = SingleCityLocationIntent.From("US", "CA", "San Francisco");
        ILocationIntent singleCityA = SingleCityLocationIntent.From("CH", "Geneva");
        ILocationIntent singleCityB = SingleCityLocationIntent.From("CH", "Zurich");
        ILocationIntent singleServerFromStateA = SingleServerLocationIntent.From("US", "CA", "Los Angeles", ServerInfo.From("1", "US-CA#1"));
        ILocationIntent singleServerFromStateB = SingleServerLocationIntent.From("US", "CA", "Los Angeles", ServerInfo.From("2", "US-CA#2"));
        ILocationIntent singleServerA = SingleServerLocationIntent.From("CH", "Geneva", ServerInfo.From("1", "CH#1"));
        ILocationIntent singleServerB = SingleServerLocationIntent.From("CH", "Geneva", ServerInfo.From("2", "CH#2"));
        ILocationIntent singleGatewayA = SingleGatewayLocationIntent.From("PROTON");
        ILocationIntent singleGatewayB = SingleGatewayLocationIntent.From("GUEST");
        ILocationIntent singleGatewayServerA = SingleGatewayServerLocationIntent.From("PROTON", GatewayServerInfo.From("1", "PROTON-CH#1", "CH"));
        ILocationIntent singleGatewayServerB = SingleGatewayServerLocationIntent.From("PROTON", GatewayServerInfo.From("2", "PROTON-CH#2", "CH"));

        ILocationIntent genericCountryA = MultiCountryLocationIntent.Fastest;
        ILocationIntent genericCountryB = MultiCountryLocationIntent.Random;
        ILocationIntent genericCountryC = MultiCountryLocationIntent.FastestExcludingMyCountry;
        ILocationIntent genericCountryD = MultiCountryLocationIntent.RandomExcludingMyCountry;
        ILocationIntent multiCountryA = MultiCountryLocationIntent.From(["CH", "FR", "US"], SelectionStrategy.Fastest);
        ILocationIntent multiCountryB = MultiCountryLocationIntent.From(["CH", "FR", "ES"], SelectionStrategy.Fastest);
        ILocationIntent multiCountryC = MultiCountryLocationIntent.From(["CH", "FR"], SelectionStrategy.Fastest);
        ILocationIntent multiCountryD = MultiCountryLocationIntent.From(["CH", "FR", "US"], SelectionStrategy.Random);
        ILocationIntent multiStateA = MultiStateLocationIntent.From("US", ["CA", "AZ", "NY"], SelectionStrategy.Fastest);
        ILocationIntent multiStateB = MultiStateLocationIntent.From("US", ["CA", "AZ", "MA"], SelectionStrategy.Fastest);
        ILocationIntent multiStateC = MultiStateLocationIntent.From("US", ["CA", "AZ"], SelectionStrategy.Fastest);
        ILocationIntent multiStateD = MultiStateLocationIntent.From("US", ["CA", "AZ", "NY"], SelectionStrategy.Random);
        ILocationIntent multiCityFromStateA = MultiCityLocationIntent.From("US", "CA", ["Los Angeles", "San Francisco"], SelectionStrategy.Fastest);
        ILocationIntent multiCityFromStateB = MultiCityLocationIntent.From("US", "CA", ["Los Angeles", "Sacramento"], SelectionStrategy.Fastest);
        ILocationIntent multiCityFromStateC = MultiCityLocationIntent.From("US", "CA", ["Los Angeles"], SelectionStrategy.Fastest);
        ILocationIntent multiCityFromStateD = MultiCityLocationIntent.From("US", "California", ["Los Angeles", "San Francisco"], SelectionStrategy.Fastest);
        ILocationIntent multiCityA = MultiCityLocationIntent.From("CH", ["Geneva", "Zurich"], SelectionStrategy.Fastest);
        ILocationIntent multiCityB = MultiCityLocationIntent.From("CH", ["Geneva", "Bern"], SelectionStrategy.Fastest);
        ILocationIntent multiCityC = MultiCityLocationIntent.From("CH", ["Geneva"], SelectionStrategy.Fastest);
        ILocationIntent multiCityD = MultiCityLocationIntent.From("CH", ["Geneva", "Zurich"], SelectionStrategy.Random);
        ILocationIntent multiServerFromStateA = MultiServerLocationIntent.From("US", "CA", "Los Angeles", [ServerInfo.From("1", "US-CA#1"), ServerInfo.From("2", "US-CA#2")], SelectionStrategy.Fastest);
        ILocationIntent multiServerFromStateB = MultiServerLocationIntent.From("US", "CA", "Los Angeles", [ServerInfo.From("1", "US-CA#1"), ServerInfo.From("3", "US-CA#3")], SelectionStrategy.Fastest);
        ILocationIntent multiServerFromStateC = MultiServerLocationIntent.From("US", "CA", "Los Angeles", [ServerInfo.From("1", "US-CA#1")], SelectionStrategy.Fastest);
        ILocationIntent multiServerFromStateD = MultiServerLocationIntent.From("US", "CA", "San Francisco", [ServerInfo.From("1", "US-CA#1"), ServerInfo.From("2", "US-CA#2")], SelectionStrategy.Fastest);
        ILocationIntent multiServerA = MultiServerLocationIntent.From("CH", "Geneva", [ServerInfo.From("1", "CH#1"), ServerInfo.From("2", "CH#2")], SelectionStrategy.Fastest);
        ILocationIntent multiServerB = MultiServerLocationIntent.From("CH", "Geneva", [ServerInfo.From("1", "CH#1"), ServerInfo.From("3", "CH#3")], SelectionStrategy.Fastest);
        ILocationIntent multiServerC = MultiServerLocationIntent.From("CH", "Geneva", [ServerInfo.From("1", "CH#1")], SelectionStrategy.Fastest);
        ILocationIntent multiServerD = MultiServerLocationIntent.From("CH", "Zurich", [ServerInfo.From("1", "CH#1"), ServerInfo.From("2", "CH#2")], SelectionStrategy.Fastest);
        ILocationIntent multiGatewayA = MultiGatewayLocationIntent.From(["PROTON", "GUEST"], SelectionStrategy.Fastest);
        ILocationIntent multiGatewayB = MultiGatewayLocationIntent.From(["PROTON", "EXTERNAL"], SelectionStrategy.Fastest);
        ILocationIntent multiGatewayC = MultiGatewayLocationIntent.From(["PROTON"], SelectionStrategy.Fastest);
        ILocationIntent multiGatewayD = MultiGatewayLocationIntent.From(["PROTON", "GUEST"], SelectionStrategy.Random);
        ILocationIntent multiGatewayServerA = MultiGatewayServerLocationIntent.From("PROTON", [GatewayServerInfo.From("1", "PROTON-CH#1", "CH"), GatewayServerInfo.From("2", "PROTON-CH#2", "CH")], SelectionStrategy.Fastest);
        ILocationIntent multiGatewayServerB = MultiGatewayServerLocationIntent.From("PROTON", [GatewayServerInfo.From("1", "PROTON-CH#1", "CH"), GatewayServerInfo.From("3", "PROTON-CH#3", "CH")], SelectionStrategy.Fastest);
        ILocationIntent multiGatewayServerC = MultiGatewayServerLocationIntent.From("PROTON", [GatewayServerInfo.From("1", "PROTON-CH#1", "CH")], SelectionStrategy.Fastest);
        ILocationIntent multiGatewayServerD = MultiGatewayServerLocationIntent.From("PROTON", [GatewayServerInfo.From("1", "PROTON-CH#1", "CH"), GatewayServerInfo.From("2", "PROTON-CH#2", "CH")], SelectionStrategy.Random);

        Assert.IsFalse(singleCountryA.IsSameAs(singleCountryB));
        Assert.IsFalse(singleStateA.IsSameAs(singleStateB));
        Assert.IsFalse(singleCityFromStateA.IsSameAs(singleCityFromStateB));
        Assert.IsFalse(singleCityA.IsSameAs(singleCityB));
        Assert.IsFalse(singleServerFromStateA.IsSameAs(singleServerFromStateB));
        Assert.IsFalse(singleServerA.IsSameAs(singleServerB));
        Assert.IsFalse(singleGatewayA.IsSameAs(singleGatewayB));
        Assert.IsFalse(singleGatewayServerA.IsSameAs(singleGatewayServerB));

        Assert.IsFalse(genericCountryA.IsSameAs(genericCountryB));
        Assert.IsFalse(genericCountryA.IsSameAs(genericCountryC));
        Assert.IsFalse(genericCountryA.IsSameAs(genericCountryD));
        Assert.IsFalse(multiCountryA.IsSameAs(multiCountryB));
        Assert.IsFalse(multiCountryA.IsSameAs(multiCountryC));
        Assert.IsFalse(multiCountryA.IsSameAs(multiCountryD));
        Assert.IsFalse(multiStateA.IsSameAs(multiStateB));
        Assert.IsFalse(multiStateA.IsSameAs(multiStateC));
        Assert.IsFalse(multiStateA.IsSameAs(multiStateD));
        Assert.IsFalse(multiCityFromStateA.IsSameAs(multiCityFromStateB));
        Assert.IsFalse(multiCityFromStateA.IsSameAs(multiCityFromStateC));
        Assert.IsFalse(multiCityFromStateA.IsSameAs(multiCityFromStateD));
        Assert.IsFalse(multiCityA.IsSameAs(multiCityB));
        Assert.IsFalse(multiCityA.IsSameAs(multiCityC));
        Assert.IsFalse(multiCityA.IsSameAs(multiCityD));
        Assert.IsFalse(multiServerFromStateA.IsSameAs(multiServerFromStateB));
        Assert.IsFalse(multiServerFromStateA.IsSameAs(multiServerFromStateC));
        Assert.IsFalse(multiServerFromStateA.IsSameAs(multiServerFromStateD));
        Assert.IsFalse(multiServerA.IsSameAs(multiServerB));
        Assert.IsFalse(multiServerA.IsSameAs(multiServerC));
        Assert.IsFalse(multiServerA.IsSameAs(multiServerD));
        Assert.IsFalse(multiGatewayA.IsSameAs(multiGatewayB));
        Assert.IsFalse(multiGatewayA.IsSameAs(multiGatewayC));
        Assert.IsFalse(multiGatewayA.IsSameAs(multiGatewayD));
        Assert.IsFalse(multiGatewayServerA.IsSameAs(multiGatewayServerB));
        Assert.IsFalse(multiGatewayServerA.IsSameAs(multiGatewayServerC));
        Assert.IsFalse(multiGatewayServerA.IsSameAs(multiGatewayServerD));
    }

    [TestMethod]
    public void LocationIntent_ShouldNotBeEqual_GivenDifferentIntent()
    {
        ILocationIntent singleCountry = SingleCountryLocationIntent.From("CH");
        ILocationIntent singleState = SingleStateLocationIntent.From("US", "CA");
        ILocationIntent singleCity = SingleCityLocationIntent.From("CH", "Geneva");
        ILocationIntent singleServer = SingleServerLocationIntent.From("CH", "Geneva", ServerInfo.From("1", "CH#1"));
        ILocationIntent singleGateway = SingleGatewayLocationIntent.From("PROTON");
        ILocationIntent multiCountry = MultiCountryLocationIntent.Default;
        ILocationIntent multiState = MultiStateLocationIntent.From("US", ["CA", "AZ", "NY"], SelectionStrategy.Fastest);
        ILocationIntent multiCity = MultiCityLocationIntent.From("CH", ["Geneva", "Zurich"], SelectionStrategy.Fastest);
        ILocationIntent multiServer = MultiServerLocationIntent.From("CH", "Geneva", [ServerInfo.From("1", "CH#1"), ServerInfo.From("2", "CH#2")], SelectionStrategy.Fastest);
        ILocationIntent multiGateway = MultiGatewayLocationIntent.From(["PROTON", "GUEST"], SelectionStrategy.Fastest);

        Assert.IsFalse(singleCountry.IsSameAs(singleState));
        Assert.IsFalse(singleCountry.IsSameAs(singleCity));
        Assert.IsFalse(singleCountry.IsSameAs(singleServer));
        Assert.IsFalse(singleCountry.IsSameAs(singleGateway));
        Assert.IsFalse(singleCountry.IsSameAs(multiCountry));
        Assert.IsFalse(singleCountry.IsSameAs(multiState));
        Assert.IsFalse(singleCountry.IsSameAs(multiCity));
        Assert.IsFalse(singleCountry.IsSameAs(multiServer));
        Assert.IsFalse(singleCountry.IsSameAs(multiGateway));
    }

    [TestMethod]
    public void LocationIntent_BaseShouldBeEqual_GivenHierarchicalIntent()
    {
        SingleCountryLocationIntent singleCountry = SingleCountryLocationIntent.From("US");
        SingleStateLocationIntent singleState = SingleStateLocationIntent.From("US", "CA");
        SingleCityLocationIntent singleCityFromState = SingleCityLocationIntent.From("US", "CA", "Los Angeles");
        SingleCityLocationIntent singleCity = SingleCityLocationIntent.From("US", "Los Angeles");
        SingleServerLocationIntent singleServerFromState = SingleServerLocationIntent.From("US", "CA", "Los Angeles", ServerInfo.From("1", "US-CA#1"));
        SingleServerLocationIntent singleServer = SingleServerLocationIntent.From("US", "Los Angeles", ServerInfo.From("1", "CH#1"));

        MultiStateLocationIntent multiState = MultiStateLocationIntent.From("US", ["CA", "AZ", "NY"], SelectionStrategy.Fastest);
        MultiCityLocationIntent multiCityFromState = MultiCityLocationIntent.From("US", "CA", ["Los Angeles", "San Francisco"], SelectionStrategy.Fastest);
        MultiCityLocationIntent multiCity = MultiCityLocationIntent.From("US", ["Los Angeles", "San Francisco"], SelectionStrategy.Fastest);
        MultiServerLocationIntent multiServerFromState = MultiServerLocationIntent.From("US", "CA", "Los Angeles", [ServerInfo.From("1", "US-CA#1"), ServerInfo.From("2", "US-CA#2")], SelectionStrategy.Fastest);
        MultiServerLocationIntent multiServer = MultiServerLocationIntent.From("US", "Los Angeles", [ServerInfo.From("1", "US-CA#1"), ServerInfo.From("2", "US-CA#2")], SelectionStrategy.Fastest);

        Assert.IsTrue(singleCountry.IsSameAs(singleState.Country));
        Assert.IsTrue(singleCountry.IsSameAs(singleCityFromState.Country));
        Assert.IsTrue(singleCountry.IsSameAs(singleCity.Country));
        Assert.IsTrue(singleCountry.IsSameAs(singleServerFromState.Country));
        Assert.IsTrue(singleCountry.IsSameAs(singleServer.Country));
        Assert.IsTrue(singleCountry.IsSameAs(multiState.Country));
        Assert.IsTrue(singleCountry.IsSameAs(multiCityFromState.Country));
        Assert.IsTrue(singleCountry.IsSameAs(multiCity.Country));
        Assert.IsTrue(singleCountry.IsSameAs(multiServerFromState.Country));
        Assert.IsTrue(singleCountry.IsSameAs(multiServer.Country));

        Assert.IsTrue(singleState.IsSameAs(singleCityFromState.State));
        Assert.IsTrue(singleState.IsSameAs(singleServerFromState.State));
        Assert.IsTrue(singleState.IsSameAs(multiCityFromState.State));
        Assert.IsTrue(singleState.IsSameAs(multiServerFromState.State));

        Assert.IsTrue(singleCity.IsSameAs(singleServerFromState.City));
        Assert.IsTrue(singleCity.IsSameAs(singleServer.City));
        Assert.IsTrue(singleCity.IsSameAs(multiServerFromState.City));
        Assert.IsTrue(singleCity.IsSameAs(multiServer.City));

        SingleGatewayLocationIntent singleGateway = SingleGatewayLocationIntent.From("PROTON");
        SingleGatewayServerLocationIntent singleGatewayServer = SingleGatewayServerLocationIntent.From("PROTON", GatewayServerInfo.From("1", "PROTON-CH#1", "CH"));
        MultiGatewayServerLocationIntent multiGatewayServer = MultiGatewayServerLocationIntent.From("PROTON", [GatewayServerInfo.From("1", "PROTON-CH#1", "CH"), GatewayServerInfo.From("2", "PROTON-CH#2", "CH")], SelectionStrategy.Fastest);

        Assert.IsTrue(singleGateway.IsSameAs(singleGatewayServer.Gateway));
        Assert.IsTrue(singleGateway.IsSameAs(multiGatewayServer.Gateway));
    }
}