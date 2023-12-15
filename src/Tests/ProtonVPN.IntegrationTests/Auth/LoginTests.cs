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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Api.Contracts.Users;
using ProtonVPN.Core.Auth;

namespace ProtonVPN.IntegrationTests.Auth
{
    [TestClass]
    public class LoginTests : AuthenticatedUserTests
    {
        [TestMethod]
        public async Task ItShouldLoginTheUser()
        {
            // Arrange
            SetApiResponsesForAuth();

            // Act
            AuthResult authResult = await MakeUserAuth(USERNAME, CORRECT_PASSWORD);

            // Assert
            authResult.Success.Should().BeTrue();
        }

        [TestMethod]
        public async Task ItShouldNotLoginTheUserWithIncorrectPassword()
        {
            // Arrange
            SetApiResponsesForAuth();

            // Act
            AuthResult authResult = await MakeUserAuth(USERNAME, WRONG_PASSWORD);

            // Assert
            authResult.Success.Should().BeFalse();
        }

        [TestMethod]
        public async Task ItShouldFailAndAskToProvideTwoFactorCode()
        {
            // Arrange
            SetApiResponsesForAuthWithTwoFactor();

            // Act
            AuthResult authResult = await MakeUserAuth(USERNAME, CORRECT_PASSWORD);

            // Assert
            authResult.Success.Should().BeFalse();
            authResult.Value.Should().Be(AuthError.TwoFactorRequired);
        }

        [TestMethod]
        public async Task ItShouldLoginTheUserWithSso()
        {
            // Arrange
            SetApiResponsesForSsoAuth();

            // Act
            AuthResult authResult = await MakeUserAuthWithSso(USERNAME);

            // Assert
            authResult.Success.Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow("", "", "", "")]
        [DataRow("johndoe", "johndoe@proton.me", "John Doe", "johndoe")]
        [DataRow("", "johndoe@proton.me", "John Doe", "johndoe@proton.me")]
        [DataRow("johndoe", "", "John Doe", "johndoe")]
        [DataRow("johndoe", "johndoe@proton.me", "", "johndoe")]
        [DataRow("johndoe", "", "", "johndoe")]
        [DataRow("", "johndoe@proton.me", "", "johndoe@proton.me")]
        public void ItShouldExtractValidUsername(string name, string email, string displayName, string expectedUsername)
        {
            // Arrange
            UserResponse user = new()
            {
                Name = name,
                Email = email,
                DisplayName = displayName
            };

            // Assert
            Assert.AreEqual(expectedUsername, user.GetUsername());
        }
    }
}