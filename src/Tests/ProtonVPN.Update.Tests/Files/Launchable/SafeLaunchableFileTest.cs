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
using System.ComponentModel;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Update.Files.Launchable;

namespace ProtonVPN.Update.Tests.Files.Launchable
{
    [TestClass]
    public class SafeLaunchableFileTest
    {
        private ILaunchableFile _origin;

        [TestInitialize]
        public void TestInitialize()
        {
            _origin = Substitute.For<ILaunchableFile>();
        }

        [TestMethod]
        public void Launch_ShouldCall_Origin_Launch_WithArguments()
        {
            const string filename = "File to launch";
            const string args = "Launch arguments";
            SafeLaunchableFile launchable = new SafeLaunchableFile(_origin);

            launchable.Launch(filename, args);

            _origin.Received(1).Launch(filename, args);
        }

        [TestMethod]
        public void Launch_ShouldPassException_WhenOriginThrows()
        {
            _origin.WhenForAnyArgs(x => x.Launch("", "")).Throw<SomeException>();
            SafeLaunchableFile launchable = new SafeLaunchableFile(_origin);

            Action action = () => launchable.Launch("", "");

            action.Should().Throw<SomeException>();
        }

        [TestMethod]
        public void Launch_ShouldThrow_AppUpdateException_WhenOriginThrows_ProcessException()
        {
            Exception[] exceptions =
            {
                new Win32Exception()
            };

            foreach (Exception exception in exceptions)
            {
                Launch_ShouldThrow_AppUpdateException_WhenOriginThrows(exception);
            }
        }

        private void Launch_ShouldThrow_AppUpdateException_WhenOriginThrows(Exception ex)
        {
            TestInitialize();
            _origin.WhenForAnyArgs(x => x.Launch("", "")).Throw(ex);
            SafeLaunchableFile launchable = new SafeLaunchableFile(_origin);

            Action action = () => launchable.Launch("", "");

            action.Should().Throw<AppUpdateException>();
        }

        #region Helpers

        private class SomeException : Exception { }

        #endregion
    }
}
