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

using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.BugReporting.Diagnostic;
using ProtonVPN.Common.OS.Net.NetworkInterface;

namespace ProtonVPN.App.Tests.BugReporting.Diagnostic
{
    [TestClass]
    public class NetworkAdapterLogTest : BaseLogTest
    {
        private INetworkInterfaces _networkInterfaces;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            _networkInterfaces = Substitute.For<INetworkInterfaces>();
        }

        [TestMethod]
        public void ItShouldCreateLogFile()
        {
            // Arrange
            INetworkInterface[] interfaces = new[]
            {
                CreateInterface("interface1"),
                CreateInterface("interface2"),
            };
            _networkInterfaces.GetInterfaces().Returns(interfaces);

            NetworkAdapterLog log = new NetworkAdapterLog(_networkInterfaces, Config);

            // Act
            log.Write();

            // Assert
            string path = Path.Combine(TmpPath, "NetworkAdapters.txt");
            File.Exists(path).Should().BeTrue();

            string content = File.ReadAllText(path);
            content.Should().Contain("interface1");
            content.Should().Contain("interface2");
        }

        private INetworkInterface CreateInterface(string name)
        {
            INetworkInterface i = Substitute.For<INetworkInterface>();
            i.Name.Returns(name);

            return i;
        }
    }
}
