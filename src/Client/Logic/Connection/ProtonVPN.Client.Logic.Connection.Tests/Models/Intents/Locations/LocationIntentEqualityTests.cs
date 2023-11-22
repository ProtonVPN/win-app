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

using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;

namespace ProtonVPN.Client.Logic.Connection.Tests.Models.Intents.Locations;

[TestClass]
public class LocationIntentEqualityTests
{
    [TestMethod]
    public void LocationIntent_ShouldBeEqual_GivenSameReference()
    {
        ILocationIntent locationFastestCountry = new CountryLocationIntent();
        ILocationIntent locationCountry = new CountryLocationIntent("CH");
        ILocationIntent locationCity = new CityStateLocationIntent("CH", "Geneva");
        ILocationIntent locationServer = new ServerLocationIntent("1", "CH#1", "CH", "Geneva");

        Assert.IsTrue(locationFastestCountry.IsSameAs(locationFastestCountry));
        Assert.IsTrue(locationCountry.IsSameAs(locationCountry));
        Assert.IsTrue(locationCity.IsSameAs(locationCity));
        Assert.IsTrue(locationServer.IsSameAs(locationServer));
    }

    [TestMethod]
    public void LocationIntent_ShouldBeEqual_GivenSameIntent()
    {
        ILocationIntent locationFastestCountryA = new CountryLocationIntent();
        ILocationIntent locationFastestCountryB = new CountryLocationIntent();
        ILocationIntent locationCountryA = new CountryLocationIntent("CH");
        ILocationIntent locationCountryB = new CountryLocationIntent("CH");
        ILocationIntent locationCityA = new CityStateLocationIntent("CH", "Geneva");
        ILocationIntent locationCityB = new CityStateLocationIntent("CH", "Geneva");
        ILocationIntent locationServerA = new ServerLocationIntent("1", "CH#1", "CH", "Geneva");
        ILocationIntent locationServerB = new ServerLocationIntent("1", "CH#1", "CH", "Geneva");

        Assert.IsTrue(locationFastestCountryA.IsSameAs(locationFastestCountryB));
        Assert.IsTrue(locationFastestCountryB.IsSameAs(locationFastestCountryA));
        Assert.IsTrue(locationCountryA.IsSameAs(locationCountryB));
        Assert.IsTrue(locationCountryB.IsSameAs(locationCountryA));
        Assert.IsTrue(locationCityA.IsSameAs(locationCityB));
        Assert.IsTrue(locationCityB.IsSameAs(locationCityA));
        Assert.IsTrue(locationServerA.IsSameAs(locationServerB));
        Assert.IsTrue(locationServerB.IsSameAs(locationServerA));
    }

    [TestMethod]
    public void LocationIntent_ShouldBeEqual_GivenSimilarIntent()
    {
        ILocationIntent locationFastestCountryA = new CountryLocationIntent();
        ILocationIntent locationFastestCountryB = new CountryLocationIntent("");
        ILocationIntent locationFastestCountryC = new CountryLocationIntent(string.Empty);

        Assert.IsTrue(locationFastestCountryA.IsSameAs(locationFastestCountryB));
        Assert.IsTrue(locationFastestCountryA.IsSameAs(locationFastestCountryC));
        Assert.IsTrue(locationFastestCountryB.IsSameAs(locationFastestCountryA));
        Assert.IsTrue(locationFastestCountryB.IsSameAs(locationFastestCountryC));
        Assert.IsTrue(locationFastestCountryC.IsSameAs(locationFastestCountryA));
        Assert.IsTrue(locationFastestCountryC.IsSameAs(locationFastestCountryB));

        ILocationIntent locationCountryA = new CountryLocationIntent("CH");
        ILocationIntent locationCountryB = new CountryLocationIntent("ch");

        Assert.IsTrue(locationCountryA.IsSameAs(locationCountryB));
        Assert.IsTrue(locationCountryB.IsSameAs(locationCountryA));
    }

    [TestMethod]
    public void LocationIntent_ShouldNotBeEqual_GivenDifferentIntentOfSameType()
    {
        ILocationIntent locationCountryA = new CountryLocationIntent();
        ILocationIntent locationCountryB = new CountryLocationIntent("CH");
        ILocationIntent locationCountryC = new CountryLocationIntent("FR");

        Assert.IsFalse(locationCountryA.IsSameAs(locationCountryB));
        Assert.IsFalse(locationCountryA.IsSameAs(locationCountryC));
        Assert.IsFalse(locationCountryB.IsSameAs(locationCountryA));
        Assert.IsFalse(locationCountryB.IsSameAs(locationCountryC));
        Assert.IsFalse(locationCountryC.IsSameAs(locationCountryA));
        Assert.IsFalse(locationCountryC.IsSameAs(locationCountryB));

        ILocationIntent locationCityA = new CityStateLocationIntent("CH", "Geneva");
        ILocationIntent locationCityB = new CityStateLocationIntent("CH", "Zurich");
        ILocationIntent locationCityC = new CityStateLocationIntent("FR", "Paris");
        ILocationIntent locationCityD = new CityStateLocationIntent("US", "Paris");

        Assert.IsFalse(locationCityA.IsSameAs(locationCityB));
        Assert.IsFalse(locationCityA.IsSameAs(locationCityC));
        Assert.IsFalse(locationCityA.IsSameAs(locationCityD));
        Assert.IsFalse(locationCityB.IsSameAs(locationCityA));
        Assert.IsFalse(locationCityB.IsSameAs(locationCityC));
        Assert.IsFalse(locationCityB.IsSameAs(locationCityD));
        Assert.IsFalse(locationCityC.IsSameAs(locationCityA));
        Assert.IsFalse(locationCityC.IsSameAs(locationCityB));
        Assert.IsFalse(locationCityC.IsSameAs(locationCityD));
        Assert.IsFalse(locationCityD.IsSameAs(locationCityA));
        Assert.IsFalse(locationCityD.IsSameAs(locationCityB));
        Assert.IsFalse(locationCityD.IsSameAs(locationCityC));

        ILocationIntent locationServerA = new ServerLocationIntent("1", "CH#1", "CH", "Geneva");
        ILocationIntent locationServerB = new ServerLocationIntent("2", "CH#2", "CH", "Geneva");
        ILocationIntent locationServerC = new ServerLocationIntent("3", "CH#3", "CH", "Zurich");
        ILocationIntent locationServerD = new ServerLocationIntent("4", "FR#1", "FR", "Paris");

        Assert.IsFalse(locationServerA.IsSameAs(locationServerB));
        Assert.IsFalse(locationServerA.IsSameAs(locationServerC));
        Assert.IsFalse(locationServerA.IsSameAs(locationServerD));
        Assert.IsFalse(locationServerB.IsSameAs(locationServerA));
        Assert.IsFalse(locationServerB.IsSameAs(locationServerC));
        Assert.IsFalse(locationServerB.IsSameAs(locationServerD));
        Assert.IsFalse(locationServerC.IsSameAs(locationServerA));
        Assert.IsFalse(locationServerC.IsSameAs(locationServerB));
        Assert.IsFalse(locationServerC.IsSameAs(locationServerD));
        Assert.IsFalse(locationServerD.IsSameAs(locationServerA));
        Assert.IsFalse(locationServerD.IsSameAs(locationServerB));
        Assert.IsFalse(locationServerD.IsSameAs(locationServerC));
    }

    [TestMethod]
    public void LocationIntent_ShouldNotBeEqual_GivenDifferentIntent()
    {
        ILocationIntent locationFastestCountry = new CountryLocationIntent();
        ILocationIntent locationCountry = new CountryLocationIntent("CH");
        ILocationIntent locationCity = new CityStateLocationIntent("CH", "Geneva");
        ILocationIntent locationServer = new ServerLocationIntent("1", "CH#1", "CH", "Geneva");

        Assert.IsFalse(locationFastestCountry.IsSameAs(locationCountry));
        Assert.IsFalse(locationFastestCountry.IsSameAs(locationCity));
        Assert.IsFalse(locationFastestCountry.IsSameAs(locationServer));
        Assert.IsFalse(locationCountry.IsSameAs(locationFastestCountry));
        Assert.IsFalse(locationCountry.IsSameAs(locationCity));
        Assert.IsFalse(locationCountry.IsSameAs(locationServer));
        Assert.IsFalse(locationCity.IsSameAs(locationFastestCountry));
        Assert.IsFalse(locationCity.IsSameAs(locationCountry));
        Assert.IsFalse(locationCity.IsSameAs(locationServer));
        Assert.IsFalse(locationServer.IsSameAs(locationFastestCountry));
        Assert.IsFalse(locationServer.IsSameAs(locationCountry));
        Assert.IsFalse(locationServer.IsSameAs(locationCity));
    }
}