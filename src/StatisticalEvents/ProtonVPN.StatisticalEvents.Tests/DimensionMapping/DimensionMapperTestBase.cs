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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.StatisticalEvents.Dimensions.Mappers.Bases;

namespace ProtonVPN.StatisticalEvents.Tests.DimensionMapping;

[TestClass]
public abstract class DimensionMapperTestBase<TEnum, TMapper>
    where TEnum : struct
    where TMapper : IDimensionMapper<TEnum?>, new()
{
    private TMapper? _mapper;

    protected virtual IEnumerable<TEnum> ValuesToIgnore => [];

    protected abstract Func<TMapper, TEnum?, string> MapFunction { get; }

    [TestInitialize]
    public void Setup()
    {
        _mapper = new TMapper();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _mapper = default;
    }

    [TestMethod]
    public void Map_ShouldReturnCorrectString_ForAllEnumValues()
    {
        foreach (TEnum value in Enum.GetValues(typeof(TEnum)).Cast<TEnum>())
        {
            if (ValuesToIgnore.Contains(value))
            {
                continue;
            }

            string result = MapFunction(_mapper!, value);

            Assert.IsFalse(string.IsNullOrWhiteSpace(result), $"Mapping for {value} returned an empty string.");
            Assert.AreNotEqual(DimensionMapperBase.NOT_AVAILABLE, result, $"Mapping for {value} returned 'n/a', did you forget to add a new mapping?");
        }
    }

    [TestMethod]
    public void Map_ShouldReturnNotAvailable_ForNull()
    {
        string result = MapFunction(_mapper!, null);

        Assert.AreEqual(DimensionMapperBase.NOT_AVAILABLE, result, $"Mapping for null or default did not return '{DimensionMapperBase.NOT_AVAILABLE}'.");
    }
}