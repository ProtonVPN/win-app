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

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Vpn.OpenVpn.Arguments;

namespace ProtonVPN.Vpn.Tests.OpenVpn.Arguments
{
    [TestClass]
    public class TlsVerifyArgumentsTest
    {
        private OpenVpnConfig _config;

        [TestInitialize]
        public void TestInitialize()
        {
            _config = Substitute.For<OpenVpnConfig>();
        }

        [TestMethod]
        public void Enumerable_ShouldContain_ExpectedNumberOfOptions()
        {
            // Arrange
            _config.TlsExportCertFolder = "ExportCert";
            _config.TlsVerifyExePath = "ProtonVPN.TlsVerify.exe";
            TlsVerifyArguments subject = new TlsVerifyArguments(_config, "nl-101.proton.com");

            // Act
            List<string> result = subject.ToList();

            // Assert
            result.Should().HaveCount(3);
        }

        [TestMethod]
        public void Enumerable_ShouldContain_SetEnvOption()
        {
            // Arrange
            const string serverName = "nl-1.proton.com";

            _config.TlsExportCertFolder = "ExportCert";
            _config.TlsVerifyExePath = "ProtonVPN.TlsVerify.exe";
            TlsVerifyArguments subject = new TlsVerifyArguments(_config, serverName);

            // Act
            List<string> result = subject.ToList();

            // Assert
            result.Should().Contain($"--setenv peer_dns_name \"{serverName}\"");
        }

        [TestMethod]
        public void Enumerable_ShouldContain_TlsExportCertOption()
        {
            // Arrange
            const string exportCertFolder = "C:\\ProgramData\\ExportCert";

            _config.TlsExportCertFolder = exportCertFolder;
            _config.TlsVerifyExePath = "ProtonVPN.TlsVerify.exe";
            TlsVerifyArguments subject = new TlsVerifyArguments(_config, "gb-15.proton.com");

            // Act
            List<string> result = subject.ToList();

            // Assert
            result.Should().Contain($"--tls-export-cert \"{exportCertFolder}\"");
        }

        [TestMethod]
        public void Enumerable_ShouldContain_TlsVerifyOption()
        {
            // Arrange
            const string tlsVerifyExePath = "C:\\Program Files\\TlsVerify.exe";

            _config.TlsExportCertFolder = "ExportCert";
            _config.TlsVerifyExePath = tlsVerifyExePath;
            TlsVerifyArguments subject = new TlsVerifyArguments(_config, "gb-15.proton.com");

            // Act
            List<string> result = subject.ToList();

            // Assert
            result.Should().Contain($"--tls-verify \"{tlsVerifyExePath}\"");
        }
    }
}
