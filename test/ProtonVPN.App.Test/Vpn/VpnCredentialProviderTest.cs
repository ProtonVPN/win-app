/*
 * Copyright (c) 2021 Proton Technologies AG
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
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Crypto;
using ProtonVPN.Vpn;

namespace ProtonVPN.App.Test.Vpn
{
    [TestClass]
    public class VpnCredentialProviderTest
    {
        private const string CERTIFICATE_PEM = "-----BEGIN CERTIFICATE-----\r\ncHVibGljS2V5\r\n-----END CERTIFICATE-----";

        private readonly IAppSettings _appSettings;
        private readonly IUserStorage _userStorage;
        private readonly IAuthCredentialManager _authCredentialManager;

        private readonly Common.Configuration.Config _config = new()
        {
            VpnUsernameSuffix = "+pw"
        };

        private static string _username = "username";
        private readonly User _user = new()
        {
            VpnUsername = _username,
            VpnPassword = "password"
        };

        private readonly string[] _netShieldSuffixes = { "+f1", "+f2", "+f3" };
        private readonly PublicKey _publicKey = new("cHVibGljS2V5", KeyAlgorithm.Unknown);
        private readonly SecretKey _secretKey = new("c2VjcmV0S2V5", KeyAlgorithm.Unknown);
        // p - proton, w - windows
        private readonly string _clientIdentifierSuffix = "pw";

        public VpnCredentialProviderTest()
        {
            _appSettings = Substitute.For<IAppSettings>();
            _userStorage = Substitute.For<IUserStorage>();
            _authCredentialManager = Substitute.For<IAuthCredentialManager>();

            _appSettings.AuthenticationCertificatePem.Returns(CERTIFICATE_PEM);
            _authCredentialManager.GenerateAsync().Returns(new AuthCredential(new AsymmetricKeyPair(_secretKey, _publicKey), CERTIFICATE_PEM));
        }

        [DataTestMethod]
        [DataRow(1, "+f1")]
        [DataRow(2, "+f2")]
        [DataRow(3, "+f3")]
        public async Task Credentials_Should_Suffix_Username(int mode, string suffix)
        {
            // Arrange
            _userStorage.User().Returns(_user);
            _appSettings.NetShieldMode.Returns(mode);
            _appSettings.IsNetShieldEnabled().Returns(true);

            VpnCredentialProvider sut = CreateVpnCredentialProvider();
            Result<VpnCredentials> credentials = await sut.Credentials();

            // Assert
            credentials.Value.Username.Replace(_username, string.Empty)
                .Contains(suffix)
                .Should()
                .BeTrue();
        }

        private VpnCredentialProvider CreateVpnCredentialProvider()
        {
            return new(_config, _appSettings, _userStorage, _authCredentialManager);
        }

        [TestMethod]
        public async Task Credentials_Should_NotContainFSuffix_UsernameIfNetShieldIsDisabled()
        {
            // Arrange
            _userStorage.User().Returns(_user);
            _appSettings.NetShieldMode.Returns(2);
            _appSettings.NetShieldEnabled.Returns(false);

            VpnCredentialProvider sut = CreateVpnCredentialProvider();
            Result<VpnCredentials> credentials = await sut.Credentials();

            // Assert
            _netShieldSuffixes.Any(s => s.Contains(credentials.Value.Username)).Should().BeFalse();
        }

        [TestMethod]
        public async Task Credentials_ShouldNotModifyUsernameIfNetShieldIsDisabled()
        {
            // Arrange
            _userStorage.User().Returns(_user);
            _appSettings.NetShieldMode.Returns(2);
            _appSettings.NetShieldEnabled.Returns(true);
            _appSettings.FeatureNetShieldEnabled.Returns(false);

            VpnCredentialProvider sut = CreateVpnCredentialProvider();
            Result<VpnCredentials> credentials = await sut.Credentials();

            // Assert
            _netShieldSuffixes.Any(s => s.Contains(credentials.Value.Username)).Should().BeFalse();
        }

        [TestMethod]
        public async Task Credentials_ShouldContainClientIdentifier()
        {
            // Arrange
            _userStorage.User().Returns(_user);
            VpnCredentialProvider sut = CreateVpnCredentialProvider();
            Result<VpnCredentials> credentials = await sut.Credentials();

            // Assert
            credentials.Value.Username.Contains(_clientIdentifierSuffix).Should().BeTrue();
        }
    }
}