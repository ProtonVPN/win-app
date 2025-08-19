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
using NSubstitute;
using ProtonVPN.Common.Legacy.Vpn;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Vpn.ServerValidation;

namespace ProtonVPN.Vpn.Tests.ServerValidation;

[TestClass]
public class ServerValidatorTest
{
    private const string CONFIG_SERVER_VALIDATION_PUBLIC_KEY = "ServerValidationPublicKey";
    private const string SERVER_NAME = "protonvpn.com";
    private const string SERVER_IP = "192.168.1.1";
    private const string SERVER_LABEL = "99";
    private const string SERVER_PUBLIC_KEY_BASE_64 = "U2VydmVyUHVibGljS2V5QmFzZTY0IFByb3RvblZQTg==";
    private const string SERVER_SIGNATURE = "TestSignature";

    private ILogger _logger;
    private IConfiguration _config;
    private IEd25519SignatureValidator _ed25519SignatureValidator;
    private ServerValidator _serverValidator;

    [TestInitialize]
    public void Initialize()
    {
        _logger = Substitute.For<ILogger>();
        _config = Substitute.For<IConfiguration>();
        _ed25519SignatureValidator = Substitute.For<IEd25519SignatureValidator>();
        _serverValidator = new(_logger, _config, _ed25519SignatureValidator);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _logger = null;
        _config = null;
        _ed25519SignatureValidator = null;
        _serverValidator = null;
    }

    [TestMethod]
    public void TestValidate()
    {
        SetServerValidationPublicKeyConfig();
        SetValidSignature($"{{\"Server\":{{\"EntryIP\":\"{SERVER_IP}\",\"Label\":\"{SERVER_LABEL}\"}}}}");
        VpnHost server = CreateVpnHost();

        VpnError error = _serverValidator.Validate(server);

        error.Should().Be(VpnError.None);
    }

    private void SetValidSignature(string expectedData)
    {
        _ed25519SignatureValidator.IsValid(
            Arg.Is(expectedData),
            Arg.Is(SERVER_SIGNATURE),
            Arg.Is(CONFIG_SERVER_VALIDATION_PUBLIC_KEY)
            ).Returns(true);
    }

    private void SetServerValidationPublicKeyConfig()
    {
        _config.ServerValidationPublicKey.Returns(CONFIG_SERVER_VALIDATION_PUBLIC_KEY);
    }

    private VpnHost CreateVpnHost()
    {
        return new(
            name: SERVER_NAME,
            ip: SERVER_IP,
            label: SERVER_LABEL,
            x25519PublicKey: CreatePublicKey(),
            signature: SERVER_SIGNATURE,
            isIpv6Supported: false,
            relayIpByProtocol: null);
    }

    private PublicKey CreatePublicKey()
    {
        return new PublicKey(SERVER_PUBLIC_KEY_BASE_64, KeyAlgorithm.X25519);
    }

    [TestMethod]
    public void TestValidate_WithNullLabel()
    {
        SetServerValidationPublicKeyConfig();
        SetValidSignature($"{{\"Server\":{{\"EntryIP\":\"{SERVER_IP}\",\"Label\":null}}}}");
        VpnHost server = new(
            name: SERVER_NAME,
            ip: SERVER_IP,
            label: null,
            x25519PublicKey: CreatePublicKey(),
            signature: SERVER_SIGNATURE,
            isIpv6Supported: false,
            relayIpByProtocol: null);

        VpnError error = _serverValidator.Validate(server);

        error.Should().Be(VpnError.None);
    }

    [TestMethod]
    public void TestValidate_WithoutPublicKey()
    {
        SetServerValidationPublicKeyConfig();
        SetValidSignature($"{{\"Server\":{{\"EntryIP\":\"{SERVER_IP}\",\"Label\":\"{SERVER_LABEL}\"}}}}");
        VpnHost server = new(
            name: SERVER_NAME,
            ip: SERVER_IP,
            label: SERVER_LABEL,
            x25519PublicKey: null,
            signature: SERVER_SIGNATURE,
            isIpv6Supported: false,
            relayIpByProtocol: null);

        VpnError error = _serverValidator.Validate(server);

        error.Should().Be(VpnError.None);
    }

    [TestMethod]
    public void TestValidate_X25519PublicKeyIsTooShort()
    {
        VpnHost server = new(
            name: SERVER_NAME,
            ip: SERVER_IP,
            label: SERVER_LABEL,
            x25519PublicKey: new PublicKey(new byte[1] { 1 }, KeyAlgorithm.X25519),
            signature: SERVER_SIGNATURE,
            isIpv6Supported: false,
            relayIpByProtocol: null);

        VpnError error = _serverValidator.Validate(server);

        error.Should().Be(VpnError.ServerValidationError);
    }

    [TestMethod]
    public void TestValidate_SignatureIsNull()
    {
        VpnHost server = CreateVpnHostBySignature(null);
        VpnError error = _serverValidator.Validate(server);
        error.Should().Be(VpnError.ServerValidationError);
    }

    private VpnHost CreateVpnHostBySignature(string signature)
    {
        return new(
            name: SERVER_NAME,
            ip: SERVER_IP,
            label: SERVER_LABEL,
            x25519PublicKey: CreatePublicKey(),
            signature: signature,
            isIpv6Supported: false,
            relayIpByProtocol: null);
    }

    [TestMethod]
    public void TestValidate_SignatureIsEmpty()
    {
        VpnHost server = CreateVpnHostBySignature(string.Empty);
        VpnError error = _serverValidator.Validate(server);
        error.Should().Be(VpnError.ServerValidationError);
    }

    [TestMethod]
    public void TestValidate_SignatureIsWhitespace()
    {
        VpnHost server = CreateVpnHostBySignature(" ");
        VpnError error = _serverValidator.Validate(server);
        error.Should().Be(VpnError.ServerValidationError);
    }

    [TestMethod]
    public void TestValidate_ConfigIsNull()
    {
        _config.ServerValidationPublicKey.Returns((string)null);
        VpnHost server = CreateVpnHost();
        VpnError error = _serverValidator.Validate(server);
        error.Should().Be(VpnError.NoServerValidationPublicKey);
    }

    [TestMethod]
    public void TestValidate_ConfigIsEmpty()
    {
        _config.ServerValidationPublicKey.Returns(string.Empty);
        VpnHost server = CreateVpnHost();
        VpnError error = _serverValidator.Validate(server);
        error.Should().Be(VpnError.NoServerValidationPublicKey);
    }

    [TestMethod]
    public void TestValidate_ConfigIsWhitespace()
    {
        _config.ServerValidationPublicKey.Returns(" ");
        VpnHost server = CreateVpnHost();
        VpnError error = _serverValidator.Validate(server);
        error.Should().Be(VpnError.NoServerValidationPublicKey);
    }

    [TestMethod]
    public void TestValidate_SignatureIsInvalid()
    {
        SetServerValidationPublicKeyConfig();
        VpnHost server = CreateVpnHost();
        VpnError error = _serverValidator.Validate(server);
        error.Should().Be(VpnError.ServerValidationError);
    }
}