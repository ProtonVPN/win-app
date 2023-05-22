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

using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;

namespace ProtonVPN.Client.Logic.Connection.Tests.Models.Intents;

[TestClass]
public class ConnectionIntentEqualityTests
{
    [TestMethod]
    public void ConnectionIntent_ShouldBeEqual_GivenSameReference()
    {
        IConnectionIntent intentCountrySecureCore = new ConnectionIntent(new CountryLocationIntent("CH"), new SecureCoreFeatureIntent("SE"));
        IConnectionIntent intentCityP2P = new ConnectionIntent(new CityStateLocationIntent("CH", "Geneva"), new P2PFeatureIntent());
        IConnectionIntent intentServerTor = new ConnectionIntent(new ServerLocationIntent("CH", "Geneva", 10), new TorFeatureIntent());
        IConnectionIntent intentFreeServer = new ConnectionIntent(new FreeServerLocationIntent("CH", 10));

        Assert.IsTrue(intentCountrySecureCore.IsSameAs(intentCountrySecureCore));
        Assert.IsTrue(intentCityP2P.IsSameAs(intentCityP2P));
        Assert.IsTrue(intentServerTor.IsSameAs(intentServerTor));
        Assert.IsTrue(intentFreeServer.IsSameAs(intentFreeServer));
    }

    [TestMethod]
    public void ConnectionIntent_ShouldBeEqual_GivenSameLocationAndFeatureReference()
    {
        ILocationIntent locationCountry = new CountryLocationIntent("CH");
        ILocationIntent locationCity = new CityStateLocationIntent("CH", "Geneva");
        ILocationIntent locationServer = new ServerLocationIntent("CH", "Geneva", 10);
        ILocationIntent locationFreeServer = new FreeServerLocationIntent("CH", 10);

        IFeatureIntent featureSecureCore = new SecureCoreFeatureIntent("SE");
        IFeatureIntent featureP2P = new P2PFeatureIntent();
        IFeatureIntent featureTor = new TorFeatureIntent();

        IConnectionIntent intentCountrySecureCoreA = new ConnectionIntent(locationCountry, featureSecureCore);
        IConnectionIntent intentCountrySecureCoreB = new ConnectionIntent(locationCountry, featureSecureCore);
        IConnectionIntent intentCityP2PA = new ConnectionIntent(locationCity, featureP2P);
        IConnectionIntent intentCityP2PB = new ConnectionIntent(locationCity, featureP2P);
        IConnectionIntent intentServerTorA = new ConnectionIntent(locationServer, featureTor);
        IConnectionIntent intentServerTorB = new ConnectionIntent(locationServer, featureTor);
        IConnectionIntent intentFreeServerA = new ConnectionIntent(locationFreeServer);
        IConnectionIntent intentFreeServerB = new ConnectionIntent(locationFreeServer);

        Assert.IsTrue(intentCountrySecureCoreA.IsSameAs(intentCountrySecureCoreB));
        Assert.IsTrue(intentCountrySecureCoreB.IsSameAs(intentCountrySecureCoreA));
        Assert.IsTrue(intentCityP2PA.IsSameAs(intentCityP2PB));
        Assert.IsTrue(intentCityP2PB.IsSameAs(intentCityP2PA));
        Assert.IsTrue(intentServerTorA.IsSameAs(intentServerTorB));
        Assert.IsTrue(intentServerTorB.IsSameAs(intentServerTorA));
        Assert.IsTrue(intentFreeServerA.IsSameAs(intentFreeServerB));
        Assert.IsTrue(intentFreeServerB.IsSameAs(intentFreeServerA));
    }

    [TestMethod]
    public void ConnectionIntent_ShouldBeEqual_GivenSameIntent()
    {
        IConnectionIntent intentCountrySecureCoreA = new ConnectionIntent(new CountryLocationIntent("CH"), new SecureCoreFeatureIntent("SE"));
        IConnectionIntent intentCountrySecureCoreB = new ConnectionIntent(new CountryLocationIntent("CH"), new SecureCoreFeatureIntent("SE"));
        IConnectionIntent intentCityP2PA = new ConnectionIntent(new CityStateLocationIntent("CH", "Geneva"), new P2PFeatureIntent());
        IConnectionIntent intentCityP2PB = new ConnectionIntent(new CityStateLocationIntent("CH", "Geneva"), new P2PFeatureIntent());
        IConnectionIntent intentServerTorA = new ConnectionIntent(new ServerLocationIntent("CH", "Geneva", 10), new TorFeatureIntent());
        IConnectionIntent intentServerTorB = new ConnectionIntent(new ServerLocationIntent("CH", "Geneva", 10), new TorFeatureIntent());
        IConnectionIntent intentFreeServerA = new ConnectionIntent(new FreeServerLocationIntent("CH", 10));
        IConnectionIntent intentFreeServerB = new ConnectionIntent(new FreeServerLocationIntent("CH", 10));

        Assert.IsTrue(intentCountrySecureCoreA.IsSameAs(intentCountrySecureCoreB));
        Assert.IsTrue(intentCountrySecureCoreB.IsSameAs(intentCountrySecureCoreA));
        Assert.IsTrue(intentCityP2PA.IsSameAs(intentCityP2PB));
        Assert.IsTrue(intentCityP2PB.IsSameAs(intentCityP2PA));
        Assert.IsTrue(intentServerTorA.IsSameAs(intentServerTorB));
        Assert.IsTrue(intentServerTorB.IsSameAs(intentServerTorA));
        Assert.IsTrue(intentFreeServerA.IsSameAs(intentFreeServerB));
        Assert.IsTrue(intentFreeServerB.IsSameAs(intentFreeServerA));
    }

    [TestMethod]
    public void ConnectionIntent_ShouldNotBeEqual_GivenDifferentIntentOfSameType()
    {
        IConnectionIntent intentCountrySecureCoreA = new ConnectionIntent(new CountryLocationIntent("CH"), new SecureCoreFeatureIntent("SE"));
        IConnectionIntent intentCountrySecureCoreB = new ConnectionIntent(new CountryLocationIntent("CH"), new SecureCoreFeatureIntent("IS"));
        IConnectionIntent intentCountrySecureCoreC = new ConnectionIntent(new CountryLocationIntent("FR"), new SecureCoreFeatureIntent("SE"));
        IConnectionIntent intentCountrySecureCoreD = new ConnectionIntent(new CountryLocationIntent("FR"), new SecureCoreFeatureIntent("IS"));

        Assert.IsFalse(intentCountrySecureCoreA.IsSameAs(intentCountrySecureCoreB));
        Assert.IsFalse(intentCountrySecureCoreA.IsSameAs(intentCountrySecureCoreC));
        Assert.IsFalse(intentCountrySecureCoreA.IsSameAs(intentCountrySecureCoreD));

        IConnectionIntent intentCityP2PA = new ConnectionIntent(new CityStateLocationIntent("CH", "Geneva"), new P2PFeatureIntent());
        IConnectionIntent intentCityP2PB = new ConnectionIntent(new CityStateLocationIntent("CH", "Zurich"), new P2PFeatureIntent());
        IConnectionIntent intentCityP2PC = new ConnectionIntent(new CityStateLocationIntent("FR", "Paris"), new P2PFeatureIntent());

        Assert.IsFalse(intentCityP2PA.IsSameAs(intentCityP2PB));
        Assert.IsFalse(intentCityP2PA.IsSameAs(intentCityP2PC));

        IConnectionIntent intentServerTorA = new ConnectionIntent(new ServerLocationIntent("CH", "Geneva", 10), new TorFeatureIntent());
        IConnectionIntent intentServerTorB = new ConnectionIntent(new ServerLocationIntent("CH", "Geneva", 20), new TorFeatureIntent());
        IConnectionIntent intentServerTorC = new ConnectionIntent(new ServerLocationIntent("CH", "Zurich", 10), new TorFeatureIntent());
        IConnectionIntent intentServerTorD = new ConnectionIntent(new ServerLocationIntent("FR", "Paris", 30), new TorFeatureIntent());

        Assert.IsFalse(intentServerTorA.IsSameAs(intentServerTorB));
        Assert.IsFalse(intentServerTorA.IsSameAs(intentServerTorC));
        Assert.IsFalse(intentServerTorA.IsSameAs(intentServerTorD));

        IConnectionIntent intentFreeServerA = new ConnectionIntent(new FreeServerLocationIntent("CH", 10));
        IConnectionIntent intentFreeServerB = new ConnectionIntent(new FreeServerLocationIntent("CH", 20));
        IConnectionIntent intentFreeServerC = new ConnectionIntent(new FreeServerLocationIntent("FR", 10));
        IConnectionIntent intentFreeServerD = new ConnectionIntent(new FreeServerLocationIntent("FR", 20));

        Assert.IsFalse(intentFreeServerA.IsSameAs(intentFreeServerB));
        Assert.IsFalse(intentFreeServerA.IsSameAs(intentFreeServerC));
        Assert.IsFalse(intentFreeServerA.IsSameAs(intentFreeServerD));
    }

    [TestMethod]
    public void ConnectionIntent_ShouldNotBeEqual_GivenDifferentIntent()
    {
        IConnectionIntent intentCountrySecureCore = new ConnectionIntent(new CountryLocationIntent("CH"), new SecureCoreFeatureIntent("SE"));
        IConnectionIntent intentCountryP2P = new ConnectionIntent(new CountryLocationIntent("CH"), new P2PFeatureIntent());
        IConnectionIntent intentCountryTor = new ConnectionIntent(new CountryLocationIntent("CH"), new TorFeatureIntent());
        IConnectionIntent intentCountry = new ConnectionIntent(new CountryLocationIntent("CH"));
        IConnectionIntent intentCitySecureCore = new ConnectionIntent(new CityStateLocationIntent("CH", "Geneva"), new SecureCoreFeatureIntent("SE"));
        IConnectionIntent intentCityP2P = new ConnectionIntent(new CityStateLocationIntent("CH", "Geneva"), new P2PFeatureIntent());
        IConnectionIntent intentCityTor = new ConnectionIntent(new CityStateLocationIntent("CH", "Geneva"), new TorFeatureIntent());
        IConnectionIntent intentCity = new ConnectionIntent(new CityStateLocationIntent("CH", "Geneva"));
        IConnectionIntent intentServerSecureCore = new ConnectionIntent(new ServerLocationIntent("CH", "Geneva", 10), new SecureCoreFeatureIntent("SE"));
        IConnectionIntent intentServerP2P = new ConnectionIntent(new ServerLocationIntent("CH", "Geneva", 10), new P2PFeatureIntent());
        IConnectionIntent intentServerTor = new ConnectionIntent(new ServerLocationIntent("CH", "Geneva", 10), new TorFeatureIntent());
        IConnectionIntent intentServer = new ConnectionIntent(new ServerLocationIntent("CH", "Geneva", 10));
        IConnectionIntent intentFreeServerSecureCore = new ConnectionIntent(new FreeServerLocationIntent("CH", 10), new SecureCoreFeatureIntent("SE"));
        IConnectionIntent intentFreeServerP2P = new ConnectionIntent(new FreeServerLocationIntent("CH", 10), new P2PFeatureIntent());
        IConnectionIntent intentFreeServerTor = new ConnectionIntent(new FreeServerLocationIntent("CH", 10), new TorFeatureIntent());
        IConnectionIntent intentFreeServer = new ConnectionIntent(new FreeServerLocationIntent("CH", 10));

        Assert.IsFalse(intentCountrySecureCore.IsSameAs(intentCountryP2P));
        Assert.IsFalse(intentCountrySecureCore.IsSameAs(intentCountryTor));
        Assert.IsFalse(intentCountrySecureCore.IsSameAs(intentCountry));
        Assert.IsFalse(intentCountrySecureCore.IsSameAs(intentCitySecureCore));
        Assert.IsFalse(intentCountrySecureCore.IsSameAs(intentCityP2P));
        Assert.IsFalse(intentCountrySecureCore.IsSameAs(intentCityTor));
        Assert.IsFalse(intentCountrySecureCore.IsSameAs(intentCity));
        Assert.IsFalse(intentCountrySecureCore.IsSameAs(intentServerSecureCore));
        Assert.IsFalse(intentCountrySecureCore.IsSameAs(intentServerP2P));
        Assert.IsFalse(intentCountrySecureCore.IsSameAs(intentServerTor));
        Assert.IsFalse(intentCountrySecureCore.IsSameAs(intentServer));
        Assert.IsFalse(intentCountrySecureCore.IsSameAs(intentFreeServerSecureCore));
        Assert.IsFalse(intentCountrySecureCore.IsSameAs(intentFreeServerP2P));
        Assert.IsFalse(intentCountrySecureCore.IsSameAs(intentFreeServerTor));
        Assert.IsFalse(intentCountrySecureCore.IsSameAs(intentFreeServer));

        Assert.IsFalse(intentCityP2P.IsSameAs(intentCountrySecureCore));
        Assert.IsFalse(intentCityP2P.IsSameAs(intentCountryP2P));
        Assert.IsFalse(intentCityP2P.IsSameAs(intentCountryTor));
        Assert.IsFalse(intentCityP2P.IsSameAs(intentCountry));
        Assert.IsFalse(intentCityP2P.IsSameAs(intentCitySecureCore));
        Assert.IsFalse(intentCityP2P.IsSameAs(intentCityTor));
        Assert.IsFalse(intentCityP2P.IsSameAs(intentCity));
        Assert.IsFalse(intentCityP2P.IsSameAs(intentServerSecureCore));
        Assert.IsFalse(intentCityP2P.IsSameAs(intentServerP2P));
        Assert.IsFalse(intentCityP2P.IsSameAs(intentServerTor));
        Assert.IsFalse(intentCityP2P.IsSameAs(intentServer));
        Assert.IsFalse(intentCityP2P.IsSameAs(intentFreeServerSecureCore));
        Assert.IsFalse(intentCityP2P.IsSameAs(intentFreeServerP2P));
        Assert.IsFalse(intentCityP2P.IsSameAs(intentFreeServerTor));
        Assert.IsFalse(intentCityP2P.IsSameAs(intentFreeServer));

        Assert.IsFalse(intentServerTor.IsSameAs(intentCountrySecureCore));
        Assert.IsFalse(intentServerTor.IsSameAs(intentCountryP2P));
        Assert.IsFalse(intentServerTor.IsSameAs(intentCountryTor));
        Assert.IsFalse(intentServerTor.IsSameAs(intentCountry));
        Assert.IsFalse(intentServerTor.IsSameAs(intentCitySecureCore));
        Assert.IsFalse(intentServerTor.IsSameAs(intentCityP2P));
        Assert.IsFalse(intentServerTor.IsSameAs(intentCityTor));
        Assert.IsFalse(intentServerTor.IsSameAs(intentCity));
        Assert.IsFalse(intentServerTor.IsSameAs(intentServerSecureCore));
        Assert.IsFalse(intentServerTor.IsSameAs(intentServerP2P));
        Assert.IsFalse(intentServerTor.IsSameAs(intentServer));
        Assert.IsFalse(intentServerTor.IsSameAs(intentFreeServerSecureCore));
        Assert.IsFalse(intentServerTor.IsSameAs(intentFreeServerP2P));
        Assert.IsFalse(intentServerTor.IsSameAs(intentFreeServerTor));
        Assert.IsFalse(intentServerTor.IsSameAs(intentFreeServer));

        Assert.IsFalse(intentFreeServer.IsSameAs(intentCountrySecureCore));
        Assert.IsFalse(intentFreeServer.IsSameAs(intentCountryP2P));
        Assert.IsFalse(intentFreeServer.IsSameAs(intentCountryTor));
        Assert.IsFalse(intentFreeServer.IsSameAs(intentCountry));
        Assert.IsFalse(intentFreeServer.IsSameAs(intentCitySecureCore));
        Assert.IsFalse(intentFreeServer.IsSameAs(intentCityP2P));
        Assert.IsFalse(intentFreeServer.IsSameAs(intentCityTor));
        Assert.IsFalse(intentFreeServer.IsSameAs(intentCity));
        Assert.IsFalse(intentFreeServer.IsSameAs(intentServerSecureCore));
        Assert.IsFalse(intentFreeServer.IsSameAs(intentServerP2P));
        Assert.IsFalse(intentFreeServer.IsSameAs(intentServerTor));
        Assert.IsFalse(intentFreeServer.IsSameAs(intentServer));
        Assert.IsFalse(intentFreeServer.IsSameAs(intentFreeServerSecureCore));
        Assert.IsFalse(intentFreeServer.IsSameAs(intentFreeServerP2P));
        Assert.IsFalse(intentFreeServer.IsSameAs(intentFreeServerTor));
    }
}