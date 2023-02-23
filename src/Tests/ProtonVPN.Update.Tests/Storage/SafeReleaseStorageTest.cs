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
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NSubstitute;
using ProtonVPN.Update.Releases;
using ProtonVPN.Update.Storage;

namespace ProtonVPN.Update.Tests.Storage
{
    [TestClass]
    public class SafeReleaseStorageTest
    {
        private IReleaseStorage _origin;

        [TestInitialize]
        public void TestInitialize()
        {
            _origin = Substitute.For<IReleaseStorage>();
        }

        [TestMethod]
        public async Task Releases_ShouldCall_Origin_Releases()
        {
            SafeReleaseStorage storage = new SafeReleaseStorage(_origin);

            await storage.Releases();

            await _origin.Received(1).Releases();
        }

        [TestMethod]
        public void Releases_ShouldPassException_WhenOriginThrows()
        {
            _origin.When(x => x.Releases()).Throw<Exception>();
            SafeReleaseStorage storage = new SafeReleaseStorage(_origin);

            Func<Task> action = () => storage.Releases();

            action.Should().ThrowAsync<Exception>();
        }

        [TestMethod]
        public void Releases_ShouldThrow_AppUpdateException_WhenOriginThrows()
        {
            Exception[] exceptions =
            {
                new HttpRequestException(),
                new OperationCanceledException(),
                new SocketException(),
                new JsonException()
            };

            foreach (Exception exception in exceptions)
            {
                Releases_ShouldThrow_AppUpdateException_WhenOriginThrows(exception);
                Releases_ShouldThrow_AppUpdateException_WhenOriginThrowsAsync(exception);
            }
        }

        private void Releases_ShouldThrow_AppUpdateException_WhenOriginThrows(Exception ex)
        {
            TestInitialize();
            _origin.When(x => x.Releases()).Throw(ex);
            SafeReleaseStorage storage = new SafeReleaseStorage(_origin);

            Func<Task> action = () => storage.Releases();

            action.Should().ThrowAsync<AppUpdateException>();
        }

        private void Releases_ShouldThrow_AppUpdateException_WhenOriginThrowsAsync(Exception ex)
        {
            TestInitialize();
            _origin.Releases().Returns(Task.FromException<IEnumerable<Release>>(ex));
            SafeReleaseStorage storage = new SafeReleaseStorage(_origin);

            Func<Task> action = () => storage.Releases();

            action.Should().ThrowAsync<AppUpdateException>();
        }
    }
}