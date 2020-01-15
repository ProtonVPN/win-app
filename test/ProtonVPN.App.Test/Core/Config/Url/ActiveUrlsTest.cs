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
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Config.Url;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ProtonVPN.App.Test.Core.Config.Url
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
            var properties = typeof(ActiveUrls)
                .GetProperties()
                .Where(p => p.PropertyType == typeof(IActiveUrl))
                .Select(p => p.Name);

            foreach (var name in properties)
            {
                Property_Uri_ShouldBe_FromConfig(name);
            }
        }

        private void Property_Uri_ShouldBe_FromConfig(string propertyName)
        {
            // Arrange
            const string url = "https://proton.vpn/some";
            var config = new Common.Configuration.Config();
            config.Urls.GetType().GetProperty(propertyName)?.SetValue(config.Urls, url);
            var item = new ActiveUrls(config, _processes);
            // Act
            var result = item.GetType().GetProperty(propertyName).GetValue(item) as IActiveUrl;
            // Assert
            result.Uri.OriginalString.Should().Be(url);
        }
    }
}
