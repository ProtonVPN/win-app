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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Config.Url;

namespace ProtonVPN.App.Tests.Core.Config.Url
{
    [TestClass]
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public class UrlConfigTest
    {
        private IOsProcesses _processes;

        [TestInitialize]
        public void TestInitialize()
        {
            _processes = Substitute.For<IOsProcesses>();
        }

        [TestMethod]
        public void Properties_Uri_ShouldBe_FromConfig()
        {
            IEnumerable<string> properties = typeof(ActiveUrls)
                                             .GetProperties()
                                             .Where(p => p.PropertyType == typeof(IActiveUrl))
                                             .Select(p => p.Name);

            foreach (string name in properties)
            {
                Property_Uri_ShouldBe_FromConfig(name);
            }
        }

        private void Property_Uri_ShouldBe_FromConfig(string propertyName)
        {
            // Arrange
            const string url = "https://proton.vpn/some";
            IConfiguration config = new Common.Configuration.Config();
            config.Urls.GetType().GetProperty(propertyName)?.SetValue(config.Urls, url);
            ActiveUrls item = new(config, _processes);
            // Act
            IActiveUrl result = item.GetType().GetProperty(propertyName).GetValue(item) as IActiveUrl;
            // Assert
            result.Uri.OriginalString.Should().Be(url);
        }
    }
}
