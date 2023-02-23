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
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Update.Files.Downloadable;

namespace ProtonVPN.Update.Tests.Files.Downloadable
{
    [TestClass]
    public class SafeDownloadableFileTest
    {
        private IDownloadableFile _origin;

        [TestInitialize]
        public void TestInitialize()
        {
            _origin = Substitute.For<IDownloadableFile>();
        }

        [TestMethod]
        public async Task Download_ShouldCall_Origin_Download_WithArguments()
        {
            const string uri = "Download uri";
            const string filename = "File to launch";
            SafeDownloadableFile downloadable = new SafeDownloadableFile(_origin);

            await downloadable.Download(uri, filename);

            await _origin.Received(1).Download(uri, filename);
        }

        [TestMethod]
        public void Launch_ShouldPassException_WhenOriginThrows()
        {
            _origin.WhenForAnyArgs(x => x.Download("", "")).Throw<SomeException>();
            SafeDownloadableFile downloadable = new SafeDownloadableFile(_origin);

            Func<Task> action = () => downloadable.Download("", "");

            action.Should().ThrowAsync<SomeException>();
        }

        [TestMethod]
        public void Launch_ShouldThrow_AppUpdateException_WhenOriginThrows_CommunicationException()
        {
            Exception[] exceptions =
            {
                new HttpRequestException(),
                new OperationCanceledException(),
                new SocketException()
            };

            foreach (Exception exception in exceptions)
            {
                Launch_ShouldThrow_AppUpdateException_WhenOriginThrows(exception);
            }
        }

        private void Launch_ShouldThrow_AppUpdateException_WhenOriginThrows(Exception ex)
        {
            TestInitialize();
            _origin.WhenForAnyArgs(x => x.Download("", "")).Throw(ex);
            SafeDownloadableFile downloadable = new SafeDownloadableFile(_origin);

            Func<Task> action = () => downloadable.Download("", "");

            action.Should().ThrowAsync<AppUpdateException>();
        }

        #region Helpers

        private class SomeException : Exception { }

        #endregion
    }
}
