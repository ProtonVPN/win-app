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

using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;

namespace ProtonVPN.Client.Logic.Connection.Tests.Models.Intents.Locations;

[TestClass]
public class CountryLocationIntentFastestTests
{
    [TestMethod]
    public void CountryCodeShouldBeNullAndKindFastestAndIsNotToExcludeMyCountry_GivenNoCountryCode()
    {
        CountryLocationIntent cliA = new();
        CountryLocationIntent cliB = new(" ");
        CountryLocationIntent cliC = new(string.Empty);

        List<CountryLocationIntent> list = [cliA, cliB, cliC];

        foreach(CountryLocationIntent cli in list)
        {
            Assert.IsNull(cli.CountryCode);
            Assert.AreEqual(ConnectionIntentKind.Fastest, cli.Kind);
            Assert.IsFalse(cli.IsToExcludeMyCountry);

            Assert.IsFalse(cli.IsSpecificCountry);
            Assert.IsTrue(cli.IsFastestCountry);
            Assert.IsFalse(cli.IsFastestCountryExcludingMine);
            Assert.IsFalse(cli.IsRandomCountry);
            Assert.IsFalse(cli.IsRandomCountryExcludingMine);
        }
    }

    [TestMethod]
    public void KindShouldBeFastestAndIsNotToExcludeMyCountry_GivenCountryCode()
    {
        const string countryCode = "CH";

        CountryLocationIntent cli = new(countryCode);

        Assert.AreEqual(countryCode, cli.CountryCode);
        Assert.AreEqual(ConnectionIntentKind.Fastest, cli.Kind);
        Assert.IsFalse(cli.IsToExcludeMyCountry);

        Assert.IsTrue(cli.IsSpecificCountry);
        Assert.IsFalse(cli.IsFastestCountry);
        Assert.IsFalse(cli.IsFastestCountryExcludingMine);
        Assert.IsFalse(cli.IsRandomCountry);
        Assert.IsFalse(cli.IsRandomCountryExcludingMine);
    }

    [TestMethod]
    public void KindShouldNotBeFastestAndIsToExcludeMyCountry()
    {
        const string countryCode = "LT";

        CountryLocationIntent cli = new(countryCode, ConnectionIntentKind.Random, isToExcludeMyCountry: true);

        Assert.AreEqual(countryCode, cli.CountryCode);
        Assert.AreEqual(ConnectionIntentKind.Random, cli.Kind);
        Assert.IsTrue(cli.IsToExcludeMyCountry);

        Assert.IsTrue(cli.IsSpecificCountry);
        Assert.IsFalse(cli.IsFastestCountry);
        Assert.IsFalse(cli.IsFastestCountryExcludingMine);
        Assert.IsFalse(cli.IsRandomCountry);
        Assert.IsFalse(cli.IsRandomCountryExcludingMine);
    }
}