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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Update.Responses;

namespace ProtonVPN.Update.Tests.Responses
{
    [TestClass]
    public class FileResponseTest
    {
        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_IsNull()
        {
            FileResponse file = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };

            bool result = file.Equals(null);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeTrue_WhenOther_IsSelf()
        {
            FileResponse file = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb"};

            bool result = file.Equals(file);

            result.Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_IsNotFileResponse()
        {
            FileResponse file = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };

            bool result = file.Equals(new { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" });

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeTrue_WhenOther_IsEqual()
        {
            FileResponse file = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };
            FileResponse other = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };

            bool result = file.Equals(other);

            result.Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_Url_IsDifferent()
        {
            FileResponse file = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };
            FileResponse other = new() { Url = "http://ubiquito.com/abc.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };

            bool result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_Url_IsNull()
        {
            FileResponse file = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };
            FileResponse other = new() { Url = null, Sha512CheckSum = "123456789", Arguments = "/qb" };

            bool result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenThis_Url_IsNull()
        {
            FileResponse file = new() { Url = null, Sha512CheckSum = "123456789", Arguments = "/qb" };
            FileResponse other = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };

            bool result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeTrue_WhenBoth_Urls_AreNull()
        {
            FileResponse file = new() { Url = null, Sha512CheckSum = "123456789", Arguments = "/qb" };
            FileResponse other = new() { Url = null, Sha512CheckSum = "123456789", Arguments = "/qb" };

            bool result = file.Equals(other);

            result.Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_Sha512CheckSum_IsDifferent()
        {
            FileResponse file = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };
            FileResponse other = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "abcdefghij", Arguments = "/qb" };

            bool result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_Sha512CheckSum_IsNull()
        {
            FileResponse file = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };
            FileResponse other = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = null, Arguments = "/qb" };

            bool result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenThis_Sha512CheckSum_IsNull()
        {
            FileResponse file = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = null, Arguments = "/qb" };
            FileResponse other = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };

            bool result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeTrue_WhenBoth_Sha512CheckSum_AreNull()
        {
            FileResponse file = new() { Url = null, Sha512CheckSum = null, Arguments = "/qb" };
            FileResponse other = new() { Url = null, Sha512CheckSum = null, Arguments = "/qb" };

            bool result = file.Equals(other);

            result.Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_Arguments_IsDifferent()
        {
            FileResponse file = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };
            FileResponse other = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "" };

            bool result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_Arguments_IsNull()
        {
            FileResponse file = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };
            FileResponse other = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = null };

            bool result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenThis_Arguments_IsNull()
        {
            FileResponse file = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = null };
            FileResponse other = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };

            bool result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeTrue_WhenBoth_Arguments_AreNull()
        {
            FileResponse file = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = null };
            FileResponse other = new() { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = null };

            bool result = file.Equals(other);

            result.Should().BeTrue();
        }
    }
}
