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
using ProtonVPN.BugReporting;

namespace ProtonVPN.App.Tests.BugReporting
{
    [TestClass]
    public class EmailValidatorTest
    {
        [TestMethod]
        public void IsValid_ShouldBeFalse_WhenEmail_IsNull()
        {
            // Act
            bool result = EmailValidator.IsValid(null);
            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsValid_ShouldBeFalse_WhenEmail_IsEmpty()
        {
            // Act
            bool result = EmailValidator.IsValid(string.Empty);
            // Assert
            result.Should().BeFalse();
        }

        [DataTestMethod]
        [DataRow("email@domain.com")]
        [DataRow("joe.smith@domain.com")]
        [DataRow("email@subdomain.domain.com")]
        [DataRow("joe+smith@domain.com")]
        [DataRow("joe-smith@domain.com")]
        [DataRow("email@123.123.123.123")]
        [DataRow("email@[123.123.123.123]")]
        [DataRow("\"email\"@domain.com")]
        [DataRow("1234567890@domain.com")]
        [DataRow("email@domain-a.com")]
        [DataRow("_@domain.com")]
        public void IsValid_ShouldBeTrue_WhenEmail_IsValid(string email)
        {
            // Act
            bool result = EmailValidator.IsValid(email);
            // Assert
            result.Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow("email")]
        [DataRow("^%#$@#$#@%@#.com")]
        [DataRow("@domain.com")]
        [DataRow("Joe Smith <email@domain.com>")]
        [DataRow("email.domain.com")]
        [DataRow("email@domain@domain.com")]
        [DataRow("joe..smith@domain.com ")]
        [DataRow("email@domain.com (Joe Smith)")]
        public void IsValid_ShouldBeFalse_WhenEmail_IsNotValid(string email)
        {
            // Act
            bool result = EmailValidator.IsValid(email);
            // Assert
            result.Should().BeFalse();
        }
    }
}
