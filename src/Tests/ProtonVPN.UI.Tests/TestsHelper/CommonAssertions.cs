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

using System.Threading.Tasks;
using NUnit.Framework;

namespace ProtonVPN.UI.Tests.TestsHelper;

public class CommonAssertions
{
    public static async Task<string> GetCurrentIpAddressAsync()
    {
        string currentIpAddress = await NetworkUtils.GetExternalIpAddressAsync();
        Assert.IsNotNull(currentIpAddress);
        Assert.IsNotEmpty(currentIpAddress);

        return currentIpAddress;
    }

    public static async Task AssertIpAddressChangedAsync(string previousIpAddress)
    {
        string currentIpAddress = await GetCurrentIpAddressAsync();
        Assert.AreNotEqual(previousIpAddress, currentIpAddress);
    }

    public static async Task AssertIpAddressUnchangedAsync(string previousIpAddress)
    {
        string currentIpAddress = await GetCurrentIpAddressAsync();
        Assert.AreEqual(previousIpAddress, currentIpAddress);
    }
}