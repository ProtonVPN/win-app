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
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;

namespace ProtonVPN.Client.Logic.Connection.Tests.Models.Intents.Locations;

[TestClass]
public class CountryLocationIntentFastestTests
{
    [TestMethod]
    public void CountryLocationIntent_ShouldBeFastest_GivenNoneOrEmptyCountryCodesList()
    {
        List<MultiCountryLocationIntent> intents =
        [
            new(),
            new([]),
            new(new List<string>()),
            new(SelectionStrategy.Fastest),
            new([], SelectionStrategy.Fastest),
            new(new List<string>(), SelectionStrategy.Fastest),
            MultiCountryLocationIntent.Default,
            MultiCountryLocationIntent.Fastest,
        ];

        foreach (MultiCountryLocationIntent intent in intents)
        {
            Assert.IsTrue(intent.IsSelectionEmpty);
            Assert.IsFalse(intent.IsToExcludeMyCountry);
            Assert.AreEqual(SelectionStrategy.Fastest, intent.Strategy);
        }
    }

    [TestMethod]
    public void CountryLocationIntent_ShouldBeRandom_GivenRandomStrategyAndNoneOrEmptyCountryCodesList()
    {
        List<MultiCountryLocationIntent> intents =
        [
            new(SelectionStrategy.Random),
            new([], SelectionStrategy.Random),
            new(new List<string>(), SelectionStrategy.Random),
            MultiCountryLocationIntent.Random
        ];

        foreach (MultiCountryLocationIntent intent in intents)
        {
            Assert.IsTrue(intent.IsSelectionEmpty);
            Assert.IsFalse(intent.IsToExcludeMyCountry);
            Assert.AreEqual(SelectionStrategy.Random, intent.Strategy);
        }
    }

    [TestMethod]
    public void CountryLocationIntent_ShouldBeFastestExcludingMyCountry_GivenIsToExcludeMyCountryAndNoCountryCodes()
    {
        List<MultiCountryLocationIntent> intents =
        [
            new(isToExcludeMyCountry: true),
            new(SelectionStrategy.Fastest, isToExcludeMyCountry: true),
            MultiCountryLocationIntent.FastestExcludingMyCountry,
            MultiCountryLocationIntent.ExcludingMyCountry(SelectionStrategy.Fastest)
        ];

        foreach (MultiCountryLocationIntent intent in intents)
        {
            Assert.IsTrue(intent.IsSelectionEmpty);
            Assert.IsTrue(intent.IsToExcludeMyCountry);
            Assert.AreEqual(SelectionStrategy.Fastest, intent.Strategy);
        }
    }

    [TestMethod]
    public void CountryLocationIntent_ShouldBeRandomExcludingMyCountry_GivenIsToExcludeMyCountryAndRandomStrategyAndNoCountryCodes()
    {
        List<MultiCountryLocationIntent> intents =
        [
            new(SelectionStrategy.Random, isToExcludeMyCountry: true),
            MultiCountryLocationIntent.RandomExcludingMyCountry,
            MultiCountryLocationIntent.ExcludingMyCountry(SelectionStrategy.Random)
        ];

        foreach (MultiCountryLocationIntent intent in intents)
        {
            Assert.IsTrue(intent.IsSelectionEmpty);
            Assert.IsTrue(intent.IsToExcludeMyCountry);
            Assert.AreEqual(SelectionStrategy.Random, intent.Strategy);
        }
    }

    [TestMethod]
    public void CountryLocationIntent_ShouldBeFastestNotEmpty_GivenCountryCodesList()
    {
        List<MultiCountryLocationIntent> intents =
        [
            new(["CH"]),
            new(["CH", "FR", "US"]),
            new(new List<string>() { "CH", "FR", "US" }),
            new(["CH", "FR", "US"], SelectionStrategy.Fastest),
            MultiCountryLocationIntent.FastestFrom(["CH", "FR", "US"]),
            MultiCountryLocationIntent.From(["CH", "FR", "US"], SelectionStrategy.Fastest)
        ];

        foreach (MultiCountryLocationIntent intent in intents)
        {
            Assert.IsFalse(intent.IsSelectionEmpty);
            Assert.IsFalse(intent.IsToExcludeMyCountry);
            Assert.AreEqual(SelectionStrategy.Fastest, intent.Strategy);
        }
    }

    [TestMethod]
    public void CountryLocationIntent_ShouldBeRandomNotEmpty_GivenRandomStrategyAndCountryCodesList()
    {
        List<MultiCountryLocationIntent> intents =
        [
            new(["CH"], SelectionStrategy.Random),
            new(["CH", "FR", "US"], SelectionStrategy.Random),
            new(new List<string>() { "CH", "FR", "US" }, SelectionStrategy.Random),
            MultiCountryLocationIntent.RandomFrom(["CH", "FR", "US"]),
            MultiCountryLocationIntent.From(["CH", "FR", "US"], SelectionStrategy.Random)
        ];

        foreach (MultiCountryLocationIntent intent in intents)
        {
            Assert.IsFalse(intent.IsSelectionEmpty);
            Assert.IsFalse(intent.IsToExcludeMyCountry);
            Assert.AreEqual(SelectionStrategy.Random, intent.Strategy);
        }
    }
}