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
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;

namespace ProtonVPN.Client.Logic.Searches.Tests;

public partial class GlobalSearchTest
{
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public async Task TestSecureCore_InvalidInput_Async(string? input)
    {
        List<ILocation> result = await _globalSearch!.SearchAsync(input, ServerFeatures.SecureCore);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    [DataRow("I")]
    [DataRow("i")]
    [DataRow("Í")]
    [DataRow("í")]
    [DataRow("Ì")]
    [DataRow("ì")]
    [DataRow("Î")]
    [DataRow("î")]
    [DataRow(" i")]
    [DataRow("i ")]
    public async Task TestSecureCore_1Char_Async(string input)
    {
        List<ILocation> result = await _globalSearch!.SearchAsync(input);

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Count);
        Assert.IsNotNull(result.Single(l => l is City city && city.Name == "Islamabad"));
        Assert.IsNotNull(result.Single(l => l is State state && state.Name == "Illinois"));
        Assert.IsNotNull(result.Single(l => l is Country country && country.Code == "IS"));
    }

    [TestMethod]
    [DataRow("IS")]
    [DataRow("is")]
    [DataRow("Is")]
    [DataRow("iS")]
    [DataRow(" IS")]
    [DataRow("IS ")]
    public async Task TestSecureCore_2Chars_Async(string input)
    {
        List<ILocation> result = await _globalSearch!.SearchAsync(input, ServerFeatures.SecureCore);

        Assert.IsNotNull(result);
        Assert.AreEqual(5, result.Count);
        Assert.IsNotNull(result.Single(l => l is City city && city.Name == "Islamabad"));
        Assert.IsNotNull(result.Single(l => l is State state && state.Name == "Wisconsin"));
        Assert.IsNotNull(result.Single(l => l is State state && state.Name == "Illinois"));
        Assert.IsNotNull(result.Single(l => l is Country country && country.Code == "IS"));
        Assert.IsNotNull(result.Single(l => l is Country country && country.Code == "PK"));
    }

    [TestMethod]
    [DataRow("US-IL#234")]
    [DataRow("US-IL234")]
    [DataRow("us-il2")]
    [DataRow("us-il#2")]
    public async Task TestSecureCore_Servers_Async(string input)
    {
        List<ILocation> result = await _globalSearch!.SearchAsync(input, ServerFeatures.SecureCore);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.IsNotNull(result.Single(l => l is Server server && server.Name == "US-IL#234"));
    }

    [TestMethod]
    [DataRow("WI#123")]
    [DataRow("wi123")]
    [DataRow("us#")]
    [DataRow("#")]
    [DataRow("123")]
    public async Task TestSecureCore_Servers_NotStartsWith_DoesNotReturn_Async(string input)
    {
        List<ILocation> result = await _globalSearch!.SearchAsync(input, ServerFeatures.SecureCore);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }
}