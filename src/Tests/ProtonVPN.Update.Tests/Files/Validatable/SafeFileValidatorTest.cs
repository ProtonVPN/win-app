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
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProtonVPN.Update.Files.Validatable;

namespace ProtonVPN.Update.Tests.Files.Validatable
{
    [TestClass]
    public class SafeFileValidatorTest
    {
        private IFileValidator _origin;
        private IFileValidator _validatable;

        [TestInitialize]
        public void TestInitialize()
        {
            _origin = Substitute.For<IFileValidator>();
            _validatable = new SafeFileValidator(_origin);
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task Valid_ShouldBe_OriginValid(bool value)
        {
            _origin.Valid("", "").ReturnsForAnyArgs(Task.FromResult(value));

            bool result = await _validatable.Valid("filename", "checkSum");

            result.Should().Be(value);
        }

        [TestMethod]
        public async Task Valid_ShouldPass_Arguments_ToOrigin()
        {
            const string filename = "The file to check";
            const string checkSum = "The expected check sum";

            await _validatable.Valid(filename, checkSum);

            await _origin.Received(1).Valid(filename, checkSum);
        }

        [TestMethod]
        public void Valid_ShouldPassException_WhenOriginThrows()
        {
            _origin.Valid("", "").ReturnsForAnyArgs(Task.FromException<bool>(new SomeException()));

            Func<Task> action = () => _validatable.Valid("", "");

            action.Should().ThrowAsync<SomeException>();
        }

        [TestMethod]
        public void Valid_ShouldThrow_AppUpdateException_WhenOriginThrows_FileAccessException()
        {
            Exception[] exceptions =
            {
                new IOException(),
                new UnauthorizedAccessException()
            };

            foreach (Exception exception in exceptions)
            {
                Valid_ShouldThrow_AppUpdateException_WhenOriginThrows(exception);
                Valid_ShouldThrow_AppUpdateException_WhenOriginThrowsAsync(exception);
            }
        }

        private void Valid_ShouldThrow_AppUpdateException_WhenOriginThrows(Exception ex)
        {
            TestInitialize();
            _origin.Valid("", "").ThrowsForAnyArgs(ex);

            Func<Task> action = () => _validatable.Valid("", "");

            action.Should().ThrowAsync<AppUpdateException>();
        }

        private void Valid_ShouldThrow_AppUpdateException_WhenOriginThrowsAsync(Exception ex)
        {
            TestInitialize();
            _origin.Valid("", "").ReturnsForAnyArgs(Task.FromException<bool>(ex));

            Func<Task> action = () => _validatable.Valid("", "");

            action.Should().ThrowAsync<AppUpdateException>();
        }

        #region Helpers

        private class SomeException : Exception { }

        #endregion
    }
}
