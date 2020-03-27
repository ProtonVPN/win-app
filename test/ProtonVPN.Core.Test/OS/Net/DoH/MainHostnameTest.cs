/*
 * Copyright (c) 2020 Proton Technologies AG
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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Core.OS.Net.DoH;

namespace ProtonVPN.Core.Test.OS.Net.DoH
{
    [TestClass]
    public class MainHostnameTest
    {
        [TestMethod]
        public void Value_ShouldReturnCorrectHost()
        {
            new MainHostname("https://api.protonvpn.ch")
                .Value()
                .Should()
                .Be("dMFYGSLTQOJXXI33OOZYG4LTDNA.protonpro.xyz");
        }
    }
}
