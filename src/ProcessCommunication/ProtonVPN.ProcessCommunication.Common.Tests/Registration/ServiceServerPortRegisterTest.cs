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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.ProcessCommunication.Common.Registration;
using ProtonVPN.ProcessCommunication.Common.Tests.Mocks;

namespace ProtonVPN.ProcessCommunication.Common.Tests.Registration
{
    // Initialize() before each call to make sure no cache is being used
    [TestClass]
    public class ServiceServerPortRegisterTest
    {
        private MockOfRegistryEditor _registryEditor;
        private ILogger _logger;
        private ServiceServerPortRegister _serviceServerPortRegister;

        [TestInitialize]
        public void Initialize()
        {
            _registryEditor = new MockOfRegistryEditor();
            _logger = Substitute.For<ILogger>();
            _serviceServerPortRegister = new ServiceServerPortRegister(_registryEditor, _logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _registryEditor = null;
            _logger = null;
            _serviceServerPortRegister = null;
        }

        [TestMethod]
        public void TestReadOnce_WhenNothingIsWritten()
        {
            int? result = _serviceServerPortRegister.ReadOnce();

            Assert.IsNull(result);
        }

        [TestMethod]
        public void TestWrite_ReadOnce_Delete_ReadOnce()
        {
            //Write
            int timestamp = GetCurrentDayMilliseconds();
            _serviceServerPortRegister.Write(timestamp);

            //ReadOnce
            int? result = _serviceServerPortRegister.ReadOnce();
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Value, timestamp);

            //Delete
            _serviceServerPortRegister.Delete();

            //ReadOnce
            int? result2 = _serviceServerPortRegister.ReadOnce();
            Assert.IsNull(result2);
        }

        private int GetCurrentDayMilliseconds()
        {
            DateTime utcNow = DateTime.UtcNow;
            return (int)utcNow.Subtract(new DateTime(utcNow.Year, utcNow.Month, utcNow.Day)).TotalMilliseconds;
        }

        [TestMethod]
        public async Task TestWrite_ReadAsync_Delete_ReadOnce()
        {
            //Write
            int timestamp = GetCurrentDayMilliseconds();
            _serviceServerPortRegister.Write(timestamp);

            //ReadAsync
            CancellationTokenSource cts = new();
            cts.Cancel();
            int result = await _serviceServerPortRegister.ReadAsync(cts.Token);
            Assert.AreEqual(result, timestamp);

            //Delete
            _serviceServerPortRegister.Delete();

            //ReadOnce
            int? result2 = _serviceServerPortRegister.ReadOnce();
            Assert.IsNull(result2);
        }

        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task TestReadAsync_WhenNothingIsWritten()
        {
            CancellationTokenSource cts = new();
            cts.Cancel();

            await _serviceServerPortRegister.ReadAsync(cts.Token);
        }

        [TestMethod]
        public async Task TestReadAsync_WhenSomethingIsWrittenLater()
        {
            //ReadAsync start
            CancellationTokenSource cts = new();
            Task<int> readAsyncTask = _serviceServerPortRegister.ReadAsync(cts.Token);
            await Task.Delay(TimeSpan.FromMilliseconds(500));

            //Write
            int timestamp = GetCurrentDayMilliseconds();
            _serviceServerPortRegister.Write(timestamp);

            //ReadAsync await
            int result = await readAsyncTask;
            Assert.AreEqual(result, timestamp);

            //Delete
            _serviceServerPortRegister.Delete();

            //ReadOnce
            int? result2 = _serviceServerPortRegister.ReadOnce();
            Assert.IsNull(result2);
        }

        [TestMethod]
        public void TestDelete()
        {
            //ReadOnce
            int? result = _serviceServerPortRegister.ReadOnce();
            Assert.IsNull(result);

            //Delete
            _serviceServerPortRegister.Delete();

            //ReadOnce
            int? result2 = _serviceServerPortRegister.ReadOnce();
            Assert.IsNull(result2);
        }
    }
}