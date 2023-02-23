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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProtonVPN.Update.Files.UpdatesDirectory;

namespace ProtonVPN.Update.Tests.Files.UpdatesDirectory
{
    [TestClass]
    public class SafeUpdatesDirectoryTest
    {
        private IUpdatesDirectory _origin;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _origin = Substitute.For<IUpdatesDirectory>();
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "UnusedVariable")]
        public void Path_ShouldGet_OriginPath()
        {
            SafeUpdatesDirectory directory = new(_origin);

            string result = directory.Path;

            string dummy = _origin.Received(1).Path;
        }

        [TestMethod]
        public void Path_ShouldBe_FromOrigin()
        {
            const string expected = "Expected path";
            _origin.Path.Returns(expected);
            SafeUpdatesDirectory directory = new(_origin);

            string result = directory.Path;

            result.Should().Be(expected);
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "UnusedVariable")]
        public void Path_ShouldPassException_WhenOriginThrows()
        {
            _origin.Path.Throws<SomeException>();
            SafeUpdatesDirectory directory = new(_origin);

            Action action = () => { string result = directory.Path; };

            action.Should().Throw<SomeException>();
        }

        [TestMethod]
        public void Path_ShouldThrow_AppUpdateException_WhenOriginThrows_FileAccessException()
        {
            Exception[] exceptions =
            {
                new IOException(),
                new UnauthorizedAccessException()
            };

            foreach (Exception exception in exceptions)
            {
                Path_ShouldThrow_AppUpdateException_WhenOriginThrows(exception);
            }
        }

        [SuppressMessage("ReSharper", "UnusedVariable")]
        private void Path_ShouldThrow_AppUpdateException_WhenOriginThrows(Exception ex)
        {
            TestInitialize();
            _origin.Path.Throws(ex);
            SafeUpdatesDirectory directory = new(_origin);

            Action action = () => { string result = directory.Path; };

            action.Should().Throw<AppUpdateException>();
        }

        [TestMethod]
        public void Cleanup_ShouldCall_Origin()
        {
            SafeUpdatesDirectory directory = new(_origin);

            directory.Cleanup();

            _origin.Received(1).Cleanup();
        }

        [TestMethod]
        public void Cleanup_ShouldPassException_WhenOriginThrows()
        {
            _origin.When(x => x.Cleanup()).Do(_ => throw new SomeException());
            SafeUpdatesDirectory directory = new(_origin);

            Action action = () => directory.Cleanup(); 

            action.Should().Throw<SomeException>();
        }

        [TestMethod]
        public void Cleanup_ShouldThrow_AppUpdateException_WhenOriginThrows_FileAccessException()
        {
            Exception[] exceptions =
            {
                new IOException(),
                new UnauthorizedAccessException()
            };

            foreach (Exception exception in exceptions)
            {
                Cleanup_ShouldThrow_AppUpdateException_WhenOriginThrows(exception);
            }
        }

        private void Cleanup_ShouldThrow_AppUpdateException_WhenOriginThrows(Exception ex)
        {
            TestInitialize();
            _origin.When(x => x.Cleanup()).Do(_ => throw ex);
            SafeUpdatesDirectory directory = new(_origin);

            Action action = () => directory.Cleanup();

            action.Should().Throw<AppUpdateException>();
        }

        #region Helpers

        private class SomeException : Exception { }

        #endregion
    }
}
