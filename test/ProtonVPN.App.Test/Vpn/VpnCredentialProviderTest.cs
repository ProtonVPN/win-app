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

using System.Linq;
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
        private readonly Common.Configuration.Config _config = new Common.Configuration.Config
        {
            VpnUsernameSuffix = "+pw"
        };

        private static string _username = "username";
        private readonly User _user = new User
        {
            VpnUsername = _username,
            VpnPassword = "password"
        };

        private readonly string[] _netShieldSuffixes = { "+f1", "+f2", "+f3" };

        // p - proton, w - windows
        private readonly string _clientIdentifierSuffix = "pw";

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
            _userStorage.User().Returns(_user);
            _appSettings.NetShieldMode.Returns(mode);
            _appSettings.NetShieldEnabled.Returns(true);
            _appSettings.FeatureNetShieldEnabled.Returns(true);

            var sut = new VpnCredentialProvider(_config, _appSettings, _userStorage);

            // Assert
            sut.Credentials().Username.Replace(_username, string.Empty)
                .Contains(suffix)
                .Should()
                .BeTrue();
        }

        [TestMethod]
        public void Credentials_Should_NotContainFSuffix_UsernameIfNetShieldIsDisabled()
        {
            // Arrange
            _userStorage.User().Returns(_user);
            _appSettings.NetShieldMode.Returns(2);
            _appSettings.NetShieldEnabled.Returns(false);

            var sut = new VpnCredentialProvider(_config, _appSettings, _userStorage);

            // Assert
            _netShieldSuffixes.Any(s => s.Contains(sut.Credentials().Username)).Should().BeFalse();
        }

        [TestMethod]
        public void Credentials_ShouldNotModifyUsernameIfNetShieldIsDisabled()
        {
            // Arrange
            _userStorage.User().Returns(_user);
            _appSettings.NetShieldMode.Returns(2);
            _appSettings.NetShieldEnabled.Returns(true);
            _appSettings.FeatureNetShieldEnabled.Returns(false);

            var sut = new VpnCredentialProvider(_config, _appSettings, _userStorage);

            // Assert
            _netShieldSuffixes.Any(s => s.Contains(sut.Credentials().Username)).Should().BeFalse();
        }

        [TestMethod]
        public void Credentials_ShouldContainClientIdentifier()
        {
            // Arrange
            _userStorage.User().Returns(_user);
            var sut = new VpnCredentialProvider(_config, _appSettings, _userStorage);

            // Assert
            sut.Credentials().Username.Contains(_clientIdentifierSuffix).Should().BeTrue();
        }
    }
}
