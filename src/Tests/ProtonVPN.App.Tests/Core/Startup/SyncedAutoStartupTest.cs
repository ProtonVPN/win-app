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

using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Startup;

namespace ProtonVPN.App.Tests.Core.Startup
{
    [TestClass]
    public class SyncedAutoStartupTest
    {
        private ISyncableAutoStartup _origin;

        [TestInitialize]
        public void TestInitialize()
        {
            _origin = Substitute.For<ISyncableAutoStartup>();
        }

        [TestMethod]
        public void Sync_ShouldCall_OriginSync()
        {
            SyncedAutoStartup startup = new(_origin);

            startup.Sync();

            _origin.Received(1).Sync();
        }

        [TestMethod]
        public void OnAppSettingsChanged_ShouldCall_OriginSync_WhenPropertyNameIs_StartOnBoot()
        {
            SyncedAutoStartup startup = new(_origin);

            startup.OnAppSettingsChanged(new PropertyChangedEventArgs(nameof(IAppSettings.StartOnBoot)));

            _origin.Received(1).Sync();
        }

        [TestMethod]
        public void OnAppSettingsChanged_ShouldNotCall_OriginSync_WhenPropertyNameIsNot_StartOnBoot()
        {
            SyncedAutoStartup startup = new(_origin);

            startup.OnAppSettingsChanged(new PropertyChangedEventArgs("SomePropertyName"));

            _origin.DidNotReceive().Sync();
        }
    }
}