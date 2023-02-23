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
using ProtonVPN.Tests.Common;
using ProtonVPN.Update.Files.Validatable;

namespace ProtonVPN.Update.Tests.Files.Validatable
{
    [TestClass]
    public class CachedFileValidatorTest
    {
        private IFileValidator _origin;
        private IFileValidator _cachedFileValidator;

        [TestInitialize]
        public void TestInitialize()
        {
            _origin = Substitute.For<IFileValidator>();
            _cachedFileValidator = new CachedFileValidator(_origin);
        }

        [TestMethod]
        public async Task Valid_ShouldCall_Origin_Valid_WithArguments()
        {
            const string filename = "TestData\\ProtonVPN_win_v1.0.0.exe";
            const string checkSum = "The expected check sum";

            await _cachedFileValidator.Valid(filename, checkSum);

            await _origin.Received(1).Valid(filename, checkSum);
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task Valid_ShouldBe_Origin_Valid(bool value)
        {
            const string filename = "TestData\\ProtonVPN_win_v1.5.2.exe";
            _origin.Valid("", "").ReturnsForAnyArgs(Task.FromResult(value));

            bool result = await _cachedFileValidator.Valid(filename, "checkSum");

            result.Should().Be(value);
        }

        [TestMethod]
        public async Task Valid_ShouldBeTrue_SecondTime_WenFileHasNotChanged()
        {
            const string filename = "TestData\\ProtonVPN_win_v1.0.0.exe";
            const string checkSum = "b1dc9dbd738a5f98b7f5e920ffcc5ba9db42517e";

            _origin.Valid("", "").ReturnsForAnyArgs(Task.FromResult(true));
            (await _cachedFileValidator.Valid(filename, checkSum)).Should().BeTrue();
            _origin.ClearReceivedCalls();

            bool result = await _cachedFileValidator.Valid(filename, checkSum);

            result.Should().BeTrue();
        }

        [TestMethod]
        public async Task Valid_ShouldNotCall_Origin_SecondTime_WenFileHasNotChanged()
        {
            const string filename = "TestData\\ProtonVPN_win_v1.0.0.exe";
            const string checkSum = "b1dc9dbd738a5f98b7f5e920ffcc5ba9db42517e";

            _origin.Valid("", "").ReturnsForAnyArgs(Task.FromResult(true));
            (await _cachedFileValidator.Valid(filename, checkSum)).Should().BeTrue();
            _origin.ClearReceivedCalls();

            await _cachedFileValidator.Valid(filename, checkSum);

            await _origin.DidNotReceiveWithAnyArgs().Valid("", "");
        }

        [TestMethod]
        public async Task Valid_ShouldCall_Origin_WenFilename_HasChanged()
        {
            const string filename = "TestData\\ProtonVPN_win_v1.0.0.exe";
            const string checkSum = "b1dc9dbd738a5f98b7f5e920ffcc5ba9db42517e";
            const string changedFilename = "TestData\\ProtonVPN_win_v1.5.0.exe";

            _origin.Valid("", "").ReturnsForAnyArgs(Task.FromResult(true));
            (await _cachedFileValidator.Valid(filename, checkSum)).Should().BeTrue();
            _origin.ClearReceivedCalls();

            await _cachedFileValidator.Valid(changedFilename, checkSum);

            await _origin.Received().Valid(changedFilename, checkSum);
        }

        [TestMethod]
        public async Task Valid_ShouldCall_Origin_WenCheckSum_HasChanged()
        {
            const string filename = "TestData\\ProtonVPN_win_v1.0.0.exe";
            const string checkSum = "b1dc9dbd738a5f98b7f5e920ffcc5ba9db42517e";
            const string changedCheckSum = "Changed Check Sum";

            _origin.Valid("", "").ReturnsForAnyArgs(Task.FromResult(true));
            (await _cachedFileValidator.Valid(filename, checkSum)).Should().BeTrue();
            _origin.ClearReceivedCalls();

            await _cachedFileValidator.Valid(filename, changedCheckSum);

            await _origin.Received().Valid(filename, changedCheckSum);
        }

        [TestMethod]
        public async Task Valid_ShouldCall_Origin_WenFileLastWriteTime_HasChanged()
        {
            string updatesPath = TestConfig.GetFolderPath();
            CopyFile("ProtonVPN_win_v1.5.1.exe", updatesPath);
            string filename = Path.Combine(updatesPath, "ProtonVPN_win_v1.5.1.exe");
            const string checkSum = "ba6b5ca2db65ff7817e3336a386e7525c01dc639";

            _origin.Valid("", "").ReturnsForAnyArgs(Task.FromResult(true));
            (await _cachedFileValidator.Valid(filename, checkSum)).Should().BeTrue();

            _origin.ClearReceivedCalls();
            File.SetLastWriteTimeUtc(filename, new DateTime(2000, 04, 28, 12, 15, 33));

            await _cachedFileValidator.Valid(filename, checkSum);

            await _origin.Received().Valid(filename, checkSum);
        }

        [TestMethod]
        public async Task Valid_ShouldCall_Origin_WenFileLength_HasChanged()
        {
            string updatesPath = TestConfig.GetFolderPath();
            CopyFile("ProtonVPN_win_v1.5.1.exe", updatesPath);
            string filename = Path.Combine(updatesPath, "ProtonVPN_win_v1.5.1.exe");
            const string checkSum = "ba6b5ca2db65ff7817e3336a386e7525c01dc639";

            _origin.Valid("", "").ReturnsForAnyArgs(Task.FromResult(true));
            (await _cachedFileValidator.Valid(filename, checkSum)).Should().BeTrue();

            _origin.ClearReceivedCalls();
            DateTime lastWriteTime = File.GetLastWriteTimeUtc(filename);
            CopyFile("win-update.json", updatesPath, "ProtonVPN_win_v1.5.1.exe");
            File.SetLastWriteTimeUtc(filename, lastWriteTime);

            await _cachedFileValidator.Valid(filename, checkSum);

            await _origin.Received().Valid(filename, checkSum);
        }

        [TestMethod]
        public async Task Valid_ShouldBeFalse_WenFileDoesNotExist()
        {
            string updatesPath = TestConfig.GetFolderPath();
            CopyFile("ProtonVPN_win_v1.5.1.exe", updatesPath);
            string filename = Path.Combine(updatesPath, "ProtonVPN_win_v1.5.1.exe");
            const string checkSum = "ba6b5ca2db65ff7817e3336a386e7525c01dc639";

            _origin.Valid("", "").ReturnsForAnyArgs(Task.FromResult(true));
            (await _cachedFileValidator.Valid(filename, checkSum)).Should().BeTrue();

            _origin.ClearReceivedCalls();
            _origin.Valid("", "").ReturnsForAnyArgs(Task.FromResult(false));
            File.Delete(filename);

            bool result = await _cachedFileValidator.Valid(filename, checkSum);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Valid_ShouldPassException_WhenOriginThrows()
        {
            const string filename = "TestData\\ProtonVPN_win_v1.0.0.exe";
            _origin.Valid("", "").ThrowsForAnyArgs<SomeException>();

            Func<Task> action = () => _cachedFileValidator.Valid(filename, "");

            action.Should().ThrowAsync<SomeException>();
        }

        [TestMethod]
        public void Valid_ShouldPassException_WhenOriginThrowsAsync()
        {
            const string filename = "TestData\\ProtonVPN_win_v1.0.0.exe";
            _origin.Valid("", "").ReturnsForAnyArgs(Task.FromException<bool>(new SomeException()));

            Func<Task> action = () => _cachedFileValidator.Valid(filename, "");

            action.Should().ThrowAsync<SomeException>();
        }

        #region Helpers

        private class SomeException : Exception { }

        private static void CopyFile(string sourcePath, string destPath, string newFilename = null)
        {
            if (!string.IsNullOrEmpty(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            string filename = !string.IsNullOrEmpty(newFilename) ? newFilename : Path.GetFileName(sourcePath);
            string destFullPath = Path.Combine(destPath ?? "", filename ?? "");

            File.Copy(TestConfig.GetFolderPath(sourcePath), destFullPath, true);
        }

        #endregion
    }
}