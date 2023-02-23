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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Startup;

namespace ProtonVPN.App.Tests.Core.Startup
{
    [TestClass]
    public class SyncableAutoStartupTest
    {
        private IAppSettings _appSettings;
        private IAutoStartup _autoStartup;
        private IScheduler _scheduler;

        [TestInitialize]
        public void TestInitialize()
        {
            _appSettings = Substitute.For<IAppSettings>();
            _autoStartup = Substitute.For<IAutoStartup>();
            _scheduler = Substitute.For<IScheduler>();
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Sync_ShouldSet_AutoStartup_Enabled(bool value)
        {
            _appSettings.StartOnBoot.Returns(value);
            _autoStartup.Enabled.Returns(value);
            SyncableAutoStartup startup = new SyncableAutoStartup(_appSettings, _scheduler, _autoStartup);

            startup.Sync();

            _autoStartup.Received(1).Enabled = value;
        }

        [TestMethod]
        public void Sync_ShouldSchedule_SyncBack_WhenSyncFails()
        {
            _appSettings.StartOnBoot.Returns(true);
            _autoStartup.Enabled.Returns(false);
            _autoStartup.When(x => x.Enabled = Arg.Any<bool>()).Do(x => { _autoStartup.Enabled.Returns(false); });
            SyncableAutoStartup startup = new SyncableAutoStartup(_appSettings, _scheduler, _autoStartup);

            startup.Sync();

            _appSettings.DidNotReceive().StartOnBoot = Arg.Any<bool>();
            _scheduler.Received(1).Schedule(Arg.Any<Action>());
        }

        [TestMethod]
        public void Sync_ShouldSet_AllSettings_StartOnBoot_WhenSyncFailed()
        {
            Action syncBack = null;
            _appSettings.StartOnBoot.Returns(true);
            _autoStartup.Enabled.Returns(false);
            _autoStartup.When(x => x.Enabled = Arg.Any<bool>()).Do(x => { _autoStartup.Enabled.Returns(false); });
            _scheduler.When(x => x.Schedule(Arg.Any<Action>())).Do(x => syncBack = x.Arg<Action>());
            SyncableAutoStartup startup = new SyncableAutoStartup(_appSettings, _scheduler, _autoStartup);

            startup.Sync();
            syncBack();

            _appSettings.Received(1).StartOnBoot = false;
        }

        [TestMethod]
        public void Sync_ShouldPrevent_Recursion()
        {
            Action syncBack = null;
            _appSettings.StartOnBoot.Returns(true);
            _autoStartup.Enabled.Returns(false);
            _autoStartup.When(x => x.Enabled = Arg.Any<bool>()).Do(x => { _autoStartup.Enabled.Returns(!x.Arg<bool>()); });
            _scheduler.When(x => x.Schedule(Arg.Any<Action>())).Do(x => syncBack = x.Arg<Action>());
            SyncableAutoStartup startup = new SyncableAutoStartup(_appSettings, _scheduler, _autoStartup);
            _appSettings.When(x => x.StartOnBoot = Arg.Any<bool>()).Do(x => startup.Sync());

            startup.Sync();
            syncBack();

            _autoStartup.Received(1).Enabled = true;
            _appSettings.Received(1).StartOnBoot = false;
        }
    }
}
