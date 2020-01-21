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
using ProtonVPN.Update.Contracts;

namespace ProtonVPN.Update.Test.Contracts
{
    [TestClass]
    public class FileContractTest
    {
        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_IsNull()
        {
            var file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };

            var result = file.Equals(null);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeTrue_WhenOther_IsSelf()
        {
            var file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb"};

            var result = file.Equals(file);

            result.Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_IsNotFileContract()
        {
            var file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };

            var result = file.Equals(new { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" });

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeTrue_WhenOther_IsEqual()
        {
            var file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };
            var other = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };

            var result = file.Equals(other);

            result.Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_Url_IsDifferent()
        {
            var file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };
            var other = new FileContract { Url = "http://ubiquito.com/abc.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };

            var result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_Url_IsNull()
        {
            var file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };
            var other = new FileContract { Url = null, Sha512CheckSum = "123456789", Arguments = "/qb" };

            var result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenThis_Url_IsNull()
        {
            var file = new FileContract { Url = null, Sha512CheckSum = "123456789", Arguments = "/qb" };
            var other = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };

            var result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeTrue_WhenBoth_Urls_AreNull()
        {
            var file = new FileContract { Url = null, Sha512CheckSum = "123456789", Arguments = "/qb" };
            var other = new FileContract { Url = null, Sha512CheckSum = "123456789", Arguments = "/qb" };

            var result = file.Equals(other);

            result.Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_Sha512CheckSum_IsDifferent()
        {
            var file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };
            var other = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "abcdefghij", Arguments = "/qb" };

            var result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_Sha512CheckSum_IsNull()
        {
            var file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };
            var other = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = null, Arguments = "/qb" };

            var result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenThis_Sha512CheckSum_IsNull()
        {
            var file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = null, Arguments = "/qb" };
            var other = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };

            var result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeTrue_WhenBoth_Sha512CheckSum_AreNull()
        {
            var file = new FileContract { Url = null, Sha512CheckSum = null, Arguments = "/qb" };
            var other = new FileContract { Url = null, Sha512CheckSum = null, Arguments = "/qb" };

            var result = file.Equals(other);

            result.Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_Arguments_IsDifferent()
        {
            var file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };
            var other = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "" };

            var result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_Arguments_IsNull()
        {
            var file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };
            var other = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = null };

            var result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenThis_Arguments_IsNull()
        {
            var file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = null };
            var other = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };

            var result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeTrue_WhenBoth_Arguments_AreNull()
        {
            var file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = null };
            var other = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = null };

            var result = file.Equals(other);

            result.Should().BeTrue();
        }
    }
}
