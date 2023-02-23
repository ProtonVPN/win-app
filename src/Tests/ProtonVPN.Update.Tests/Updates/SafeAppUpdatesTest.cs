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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppUpdateLogs;
using ProtonVPN.Update.Updates;

// ReSharper disable ObjectCreationAsStatement

namespace ProtonVPN.Update.Tests.Updates
{
    [TestClass]
    public class SafeAppUpdatesTest
    {
        private ILogger _logger;
        private IAppUpdates _appUpdates;

        #region Initialization

        [TestInitialize]
        public void TestInitialize()
        {
            _logger = Substitute.For<ILogger>();
            _appUpdates = Substitute.For<IAppUpdates>();
        }

        #endregion

        #region SafeAppUpdates

        [TestMethod]
        public void SafeAppUpdates_ShouldTrow_WhenLogger_IsNull()
        {
            Action f = () => new SafeAppUpdates(null, _appUpdates);

            f.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void SafeAppUpdates_ShouldTrow_WhenOrigin_IsNull()
        {
            Action f = () => new SafeAppUpdates(_logger, null);

            f.Should().Throw<ArgumentException>();
        }

        #endregion

        #region Test: Cleanup

        [TestMethod]
        public void Cleanup_ShouldCall_Origin_Cleanup()
        {
            SafeAppUpdates updates = new SafeAppUpdates(_logger, _appUpdates);

            updates.Cleanup();

            _appUpdates.Received().Cleanup();
        }

        [TestMethod]
        public void Cleanup_ShouldSuppress_AppUpdateException()
        {
            _appUpdates.When(x => x.Cleanup()).Do(x => throw new AppUpdateException(""));
            SafeAppUpdates updates = new SafeAppUpdates(_logger, _appUpdates);

            Action action = () => updates.Cleanup();

            action.Should().NotThrow();
        }

        [TestMethod]
        public void Cleanup_ShouldLog_AppUpdateException()
        {
            _appUpdates.When(x => x.Cleanup()).Do(x => throw new AppUpdateException(""));
            SafeAppUpdates updates = new SafeAppUpdates(_logger, _appUpdates);

            updates.Cleanup();

            _logger.ReceivedWithAnyArgs().Error<AppUpdateLog>("");
        }

        [TestMethod]
        public void Cleanup_ShouldPass_Exception()
        {
            _appUpdates.When(x => x.Cleanup()).Do(x => throw new SomeException());
            SafeAppUpdates updates = new SafeAppUpdates(_logger, _appUpdates);

            Action action = () => updates.Cleanup();

            action.Should().Throw<SomeException>();
        }

        #endregion

        #region Helpers

        private class SomeException : Exception { }

        #endregion
    }
}
