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
using ProtonVPN.Client.Logic.Servers.Contracts.Models;

namespace ProtonVPN.Client.Logic.Searches.Tests;

public partial class GlobalSearchTest
{
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public async Task TestStandard_InvalidInput_Async(string? input)
    {
        List<ILocation> result = await _globalSearch!.SearchAsync(input);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    [DataRow("A")]
    [DataRow("a")]
    [DataRow("Á")]
    [DataRow("á")]
    [DataRow("À")]
    [DataRow("à")]
    [DataRow("Ã")]
    [DataRow("ã")]
    [DataRow("Â")]
    [DataRow("â")]
    [DataRow(" A")]
    [DataRow("A ")]
    public async Task TestStandard_1Char_Async(string input)
    {
        List<ILocation> result = await _globalSearch!.SearchAsync(input);

        Assert.IsNotNull(result);
        Assert.AreEqual(5, result.Count);
        Assert.IsNotNull(result.Single(l => l is City city && city.Name == "Anchorage"));
        Assert.IsNotNull(result.Single(l => l is City city && city.Name == "Argel"));
        Assert.IsNotNull(result.Single(l => l is State state && state.Name == "Alaska"));
        Assert.IsNotNull(result.Single(l => l is Country country && country.Code == "AE"));
        Assert.IsNotNull(result.Single(l => l is Country country && country.Code == "DZ"));
    }

    [TestMethod]
    [DataRow("CH")]
    [DataRow("ch")]
    [DataRow("Ch")]
    [DataRow("cH")]
    [DataRow(" ch")]
    [DataRow("ch ")]
    public async Task TestStandard_2Chars_Async(string input)
    {
        List<ILocation> result = await _globalSearch!.SearchAsync(input);

        Assert.IsNotNull(result);
        Assert.AreEqual(6, result.Count);
        Assert.IsNotNull(result.Single(l => l is City city && city.Name == "Anchorage"));
        Assert.IsNotNull(result.Single(l => l is City city && city.Name == "Zurich"));
        Assert.IsNotNull(result.Single(l => l is City city && city.Name == "Chicago"));
        Assert.IsNotNull(result.Single(l => l is State state && state.Name == "Michigan"));
        Assert.IsNotNull(result.Single(l => l is Country country && country.Code == "CH"));
        Assert.IsNotNull(result.Single(l => l is Country country && country.Code == "CL"));
    }

    [TestMethod]
    [DataRow("CH#678")]
    [DataRow("ch678")]
    [DataRow("CH6")]
    [DataRow("ch#6")]
    public async Task TestStandard_Servers_Async(string input)
    {
        List<ILocation> result = await _globalSearch!.SearchAsync(input);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.IsNotNull(result.Single(l => l is Server server && server.Name == "CH#678"));
    }

    [TestMethod]
    [DataRow("FL#456")]
    [DataRow("fl456")]
    [DataRow("US#")]
    [DataRow("#")]
    [DataRow("456")]
    public async Task TestStandard_Servers_NotStartsWith_DoesNotReturn_Async(string input)
    {
        List<ILocation> result = await _globalSearch!.SearchAsync(input);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    [DataRow("united")]
    [DataRow("UNITED ")]
    [DataRow("nited")]
    [DataRow(" NiTeD ")]
    public async Task TestStandard_UnitedCountries_Async(string input)
    {
        List<ILocation> result = await _globalSearch!.SearchAsync(input);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.IsNotNull(result.Single(l => l is Country country && country.Code == "AE"));
        Assert.IsNotNull(result.Single(l => l is Country country && country.Code == "US"));
    }

    [TestMethod]
    [DataRow("state")]
    [DataRow("STATES")]
    [DataRow(" states")]
    [DataRow(" sTaTeS ")]
    [DataRow("united state")]
    [DataRow("UNITED STATES")]
    [DataRow("nited s")]
    [DataRow("Nited State")]
    [DataRow("D S")]
    public async Task TestStandard_UnitedStatesName_Async(string input)
    {
        List<ILocation> result = await _globalSearch!.SearchAsync(input);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.IsNotNull(result.Single(l => l is Country country && country.Code == "US"));
    }
}