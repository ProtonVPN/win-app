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
public class CountryLocationIntentFastestTests
{
    [TestMethod]
    public void CountryLocationIntent_ShouldBeFastest_GivenNoCountryCode()
    {
        CountryLocationIntent locationFastestCountryA = new CountryLocationIntent();
        CountryLocationIntent locationFastestCountryB = new CountryLocationIntent("");
        CountryLocationIntent locationFastestCountryC = new CountryLocationIntent(string.Empty);

        Assert.IsTrue(locationFastestCountryA.IsFastest);
        Assert.IsTrue(locationFastestCountryB.IsFastest);
        Assert.IsTrue(locationFastestCountryC.IsFastest);
    }

    [TestMethod]
    public void SecureCoreFeatureIntent_ShouldNotBeFastest_GivenEntryCountryCode()
    {
        CountryLocationIntent locationCountryA = new CountryLocationIntent("CH");
        CountryLocationIntent locationCountryB = new CountryLocationIntent("FR");

        Assert.IsFalse(locationCountryA.IsFastest);
        Assert.IsFalse(locationCountryB.IsFastest);
    }
}