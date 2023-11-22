﻿/*
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
        IConnectionIntent intentServerTor = new ConnectionIntent(new ServerLocationIntent("1", "CH#1", "CH", "Geneva"), new TorFeatureIntent());

        Assert.IsTrue(intentCountrySecureCore.IsSameAs(intentCountrySecureCore));
        Assert.IsTrue(intentCityP2P.IsSameAs(intentCityP2P));
        Assert.IsTrue(intentServerTor.IsSameAs(intentServerTor));
    }

    [TestMethod]
    public void ConnectionIntent_ShouldBeEqual_GivenSameLocationAndFeatureReference()
    {
        ILocationIntent locationCountry = new CountryLocationIntent("CH");
        ILocationIntent locationCity = new CityStateLocationIntent("CH", "Geneva");
        ILocationIntent locationServer = new ServerLocationIntent("1", "CH#1", "CH", "Geneva");

        IFeatureIntent featureSecureCore = new SecureCoreFeatureIntent("SE");
        IFeatureIntent featureP2P = new P2PFeatureIntent();
        IFeatureIntent featureTor = new TorFeatureIntent();

        IConnectionIntent intentCountrySecureCoreA = new ConnectionIntent(locationCountry, featureSecureCore);
        IConnectionIntent intentCountrySecureCoreB = new ConnectionIntent(locationCountry, featureSecureCore);
        IConnectionIntent intentCityP2PA = new ConnectionIntent(locationCity, featureP2P);
        IConnectionIntent intentCityP2PB = new ConnectionIntent(locationCity, featureP2P);
        IConnectionIntent intentServerTorA = new ConnectionIntent(locationServer, featureTor);
        IConnectionIntent intentServerTorB = new ConnectionIntent(locationServer, featureTor);

        Assert.IsTrue(intentCountrySecureCoreA.IsSameAs(intentCountrySecureCoreB));
        Assert.IsTrue(intentCountrySecureCoreB.IsSameAs(intentCountrySecureCoreA));
        Assert.IsTrue(intentCityP2PA.IsSameAs(intentCityP2PB));
        Assert.IsTrue(intentCityP2PB.IsSameAs(intentCityP2PA));
        Assert.IsTrue(intentServerTorA.IsSameAs(intentServerTorB));
        Assert.IsTrue(intentServerTorB.IsSameAs(intentServerTorA));
    }

    [TestMethod]
    public void ConnectionIntent_ShouldBeEqual_GivenSameIntent()
    {
        IConnectionIntent intentCountrySecureCoreA = new ConnectionIntent(new CountryLocationIntent("CH"), new SecureCoreFeatureIntent("SE"));
        IConnectionIntent intentCountrySecureCoreB = new ConnectionIntent(new CountryLocationIntent("CH"), new SecureCoreFeatureIntent("SE"));
        IConnectionIntent intentCityP2PA = new ConnectionIntent(new CityStateLocationIntent("CH", "Geneva"), new P2PFeatureIntent());
        IConnectionIntent intentCityP2PB = new ConnectionIntent(new CityStateLocationIntent("CH", "Geneva"), new P2PFeatureIntent());
        IConnectionIntent intentServerTorA = new ConnectionIntent(new ServerLocationIntent("1", "CH#1", "CH", "Geneva"), new TorFeatureIntent());
        IConnectionIntent intentServerTorB = new ConnectionIntent(new ServerLocationIntent("1", "CH#1", "CH", "Geneva"), new TorFeatureIntent());

        Assert.IsTrue(intentCountrySecureCoreA.IsSameAs(intentCountrySecureCoreB));
        Assert.IsTrue(intentCountrySecureCoreB.IsSameAs(intentCountrySecureCoreA));
        Assert.IsTrue(intentCityP2PA.IsSameAs(intentCityP2PB));
        Assert.IsTrue(intentCityP2PB.IsSameAs(intentCityP2PA));
        Assert.IsTrue(intentServerTorA.IsSameAs(intentServerTorB));
        Assert.IsTrue(intentServerTorB.IsSameAs(intentServerTorA));
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

        IConnectionIntent intentServerTorA = new ConnectionIntent(new ServerLocationIntent("1", "CH#1", "CH", "Geneva"), new TorFeatureIntent());
        IConnectionIntent intentServerTorB = new ConnectionIntent(new ServerLocationIntent("2", "CH#2", "CH", "Geneva"), new TorFeatureIntent());
        IConnectionIntent intentServerTorC = new ConnectionIntent(new ServerLocationIntent("3", "CH#3", "CH", "Zurich"), new TorFeatureIntent());
        IConnectionIntent intentServerTorD = new ConnectionIntent(new ServerLocationIntent("4", "FR#1", "FR", "Paris"), new TorFeatureIntent());

        Assert.IsFalse(intentServerTorA.IsSameAs(intentServerTorB));
        Assert.IsFalse(intentServerTorA.IsSameAs(intentServerTorC));
        Assert.IsFalse(intentServerTorA.IsSameAs(intentServerTorD));
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
        IConnectionIntent intentServerSecureCore = new ConnectionIntent(new ServerLocationIntent("1", "CH#1", "CH", "Geneva"), new SecureCoreFeatureIntent("SE"));
        IConnectionIntent intentServerP2P = new ConnectionIntent(new ServerLocationIntent("1", "CH#1", "CH", "Geneva"), new P2PFeatureIntent());
        IConnectionIntent intentServerTor = new ConnectionIntent(new ServerLocationIntent("1", "CH#1", "CH", "Geneva"), new TorFeatureIntent());
        IConnectionIntent intentServer = new ConnectionIntent(new ServerLocationIntent("1", "CH#1", "CH", "Geneva"));

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
    }
}