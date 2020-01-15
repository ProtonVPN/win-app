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

using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Service.Config;

namespace ProtonVPN.Service.Test.Config
{
    [TestClass]
    public class ConfigDirectoriesTest
    {
        [TestMethod]
        public void Prepare_ShouldCreate_ServiceLogFolder()
        {
            // Arrange
            const string path = nameof(Prepare_ShouldCreate_ServiceLogFolder);
            if (Directory.Exists(path))
            {
                Directory.Delete(path);
            }

            var config = new Common.Configuration.Config
            {
                ServiceLogFolder = path,
                OpenVpn =
                {
                    TlsExportCertFolder = $"{path}-1"
                }
            };
            var subject = new ConfigDirectories(config);

            // Act
            subject.Prepare();
            // Assert
            Directory.Exists(path).Should().BeTrue();
        }

        [TestMethod]
        public void Prepare_ShouldSucceed_When_ServiceLogFolder_Exists()
        {
            // Arrange
            const string path = nameof(Prepare_ShouldSucceed_When_ServiceLogFolder_Exists);
            Directory.CreateDirectory(path);

            var config = new Common.Configuration.Config
            {
                ServiceLogFolder = path,
                OpenVpn =
                {
                    TlsExportCertFolder = $"{path}-1"
                }
            };
            var subject = new ConfigDirectories(config);

            // Act
            Action action = () => subject.Prepare();
            // Assert
            action.Should().NotThrow();
        }

        [TestMethod]
        public void Prepare_ShouldCreate_TlsExportCertFolder()
        {
            // Arrange
            const string path = nameof(Prepare_ShouldCreate_TlsExportCertFolder);
            if (Directory.Exists(path))
            {
                Directory.Delete(path);
            }

            var config = new Common.Configuration.Config
            {
                ServiceLogFolder = $"{path}-1",
                OpenVpn =
                {
                    TlsExportCertFolder = path
                }
            };
            var subject = new ConfigDirectories(config);
            
            // Act
            subject.Prepare();
            // Assert
            Directory.Exists(path).Should().BeTrue();
        }

        [TestMethod]
        public void Prepare_ShouldSucceed_When_TlsExportCertFolder_Exists()
        {
            // Arrange
            const string path = nameof(Prepare_ShouldSucceed_When_TlsExportCertFolder_Exists);
            Directory.CreateDirectory(path);

            var config = new Common.Configuration.Config
            {
                ServiceLogFolder = $"{path}-1",
                OpenVpn =
                {
                    TlsExportCertFolder = path
                }
            };
            var subject = new ConfigDirectories(config);

            // Act
            Action action = () => subject.Prepare();
            // Assert
            action.Should().NotThrow();
        }
    }
}
