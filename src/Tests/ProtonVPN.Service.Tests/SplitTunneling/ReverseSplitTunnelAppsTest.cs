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
using ProtonVPN.Service.Contract.Settings;
using ProtonVPN.Service.Settings;
using ProtonVPN.Service.SplitTunneling;

namespace ProtonVPN.Service.Tests.SplitTunneling
{
    [TestClass]
    public class ReverseSplitTunnelAppsTest
    {
        [TestMethod]
        public void Value_ShouldContainApps()
        {
            // Arrange
            var paths = new[]
            {
                "path1", "path2", "path3"
            };
            var settings = new SplitTunnelSettingsContract
            {
                AppPaths = paths
            };
            var serviceSettings = Substitute.For<IServiceSettings>();
            serviceSettings.SplitTunnelSettings.Returns(settings);
            var sut = new IncludeModeApps(serviceSettings);

            // Assert
            sut.Value().Should().Contain(paths);
        }
    }
}
