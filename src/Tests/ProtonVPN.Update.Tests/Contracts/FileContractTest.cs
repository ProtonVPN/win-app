/*
 * Copyright (c) 2022 Proton Technologies AG
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

namespace ProtonVPN.Update.Tests.Contracts
{
    [TestClass]
    public class FileContractTest
    {
        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_IsNull()
        {
            FileContract file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };

            bool result = file.Equals(null);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeTrue_WhenOther_IsSelf()
        {
            FileContract file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb"};

            bool result = file.Equals(file);

            result.Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_IsNotFileContract()
        {
            FileContract file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };

            bool result = file.Equals(new { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" });

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeTrue_WhenOther_IsEqual()
        {
            FileContract file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };
            FileContract other = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };

            bool result = file.Equals(other);

            result.Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_Url_IsDifferent()
        {
            FileContract file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };
            FileContract other = new FileContract { Url = "http://ubiquito.com/abc.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };

            bool result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_Url_IsNull()
        {
            FileContract file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };
            FileContract other = new FileContract { Url = null, Sha512CheckSum = "123456789", Arguments = "/qb" };

            bool result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenThis_Url_IsNull()
        {
            FileContract file = new FileContract { Url = null, Sha512CheckSum = "123456789", Arguments = "/qb" };
            FileContract other = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };

            bool result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeTrue_WhenBoth_Urls_AreNull()
        {
            FileContract file = new FileContract { Url = null, Sha512CheckSum = "123456789", Arguments = "/qb" };
            FileContract other = new FileContract { Url = null, Sha512CheckSum = "123456789", Arguments = "/qb" };

            bool result = file.Equals(other);

            result.Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_Sha512CheckSum_IsDifferent()
        {
            FileContract file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };
            FileContract other = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "abcdefghij", Arguments = "/qb" };

            bool result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_Sha512CheckSum_IsNull()
        {
            FileContract file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };
            FileContract other = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = null, Arguments = "/qb" };

            bool result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenThis_Sha512CheckSum_IsNull()
        {
            FileContract file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = null, Arguments = "/qb" };
            FileContract other = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };

            bool result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeTrue_WhenBoth_Sha512CheckSum_AreNull()
        {
            FileContract file = new FileContract { Url = null, Sha512CheckSum = null, Arguments = "/qb" };
            FileContract other = new FileContract { Url = null, Sha512CheckSum = null, Arguments = "/qb" };

            bool result = file.Equals(other);

            result.Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_Arguments_IsDifferent()
        {
            FileContract file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };
            FileContract other = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "" };

            bool result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenOther_Arguments_IsNull()
        {
            FileContract file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };
            FileContract other = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = null };

            bool result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeFalse_WhenThis_Arguments_IsNull()
        {
            FileContract file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = null };
            FileContract other = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = "/qb" };

            bool result = file.Equals(other);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldBeTrue_WhenBoth_Arguments_AreNull()
        {
            FileContract file = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = null };
            FileContract other = new FileContract { Url = "https://protonvpn.com/download.exe", Sha512CheckSum = "123456789", Arguments = null };

            bool result = file.Equals(other);

            result.Should().BeTrue();
        }
    }
}
