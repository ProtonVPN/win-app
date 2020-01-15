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
using NSubstitute;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Vpn;

namespace ProtonVPN.App.Test.Vpn
{
    [TestClass]
    public class VpnCredentialProviderTest
    {
        private IAppSettings _appSettings;
        private IUserStorage _userStorage;

        public VpnCredentialProviderTest()
        {
            _appSettings = Substitute.For<IAppSettings>();
            _userStorage = Substitute.For<IUserStorage>();
        }

        [DataTestMethod]
        [DataRow(1, "+f1")]
        [DataRow(2, "+f2")]
        [DataRow(3, "+f3")]
        public void Credentials_Should_Suffix_Username(int mode, string suffix)
        {
            // Arrange
            var user = new User
            {
                VpnUsername = "username",
                VpnPassword = "password"
            };

            _userStorage.User().Returns(user);
            _appSettings.NetShieldMode.Returns(mode);
            _appSettings.NetShieldEnabled.Returns(true);

            var sut = new VpnCredentialProvider(_appSettings, _userStorage);

            // Assert
            sut.Credentials().Username.Should().Be(user.VpnUsername + suffix);
        }

        [TestMethod]
        public void Credentials_Should_NotModify_Username()
        {
            // Arrange
            var username = "username";
            var user = new User
            {
                VpnUsername = username,
                VpnPassword = "password"
            };

            _userStorage.User().Returns(user);
            _appSettings.NetShieldMode.Returns(2);
            _appSettings.NetShieldEnabled.Returns(false);

            var sut = new VpnCredentialProvider(_appSettings, _userStorage);

            // Assert
            sut.Credentials().Username.Should().Be(username);
        }
    }
}
