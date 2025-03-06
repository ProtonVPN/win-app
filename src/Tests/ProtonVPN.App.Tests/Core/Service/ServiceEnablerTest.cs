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
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Service;
using ProtonVPN.Exiting;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.App.Tests.Core.Service
{
    [TestClass]
    public class ServiceEnablerTest
    {
        private const string APP_KILL_EXCEPTION_MESSAGE = "App kill converted to exception in unit tests";

        private ILogger _logger;
        private IModals _modals;
        private IAppExitInvoker _appExitInvoker;

        [TestInitialize]
        public void Initialize()
        {
            _logger = Substitute.For<ILogger>();
            _modals = Substitute.For<IModals>();
            _appExitInvoker = Substitute.For<IAppExitInvoker>();
            _appExitInvoker.When(x => x.Kill()).Do(_ => throw new Exception(APP_KILL_EXCEPTION_MESSAGE));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _logger = null;
            _modals = null;
            _appExitInvoker = null;
        }

        [TestMethod]
        public async Task ItShouldTryToEnableServiceWhenItIsDisabled()
        {
            // Arrange
            IService service = Substitute.For<IService>();
            service.Enabled().Returns(false);
            _modals.ShowAsync<IModal>().Returns(true);

            // Act
            ServiceEnabler sut = new(_logger, _modals, _appExitInvoker);
            await sut.GetServiceEnabledResultAsync(service);

            // Assert
            service.Received().Enable();
        }

        [TestMethod]
        public async Task ItShouldNotTryToEnableServiceWhenItIsEnabled()
        {
            // Arrange
            IService service = Substitute.For<IService>();
            service.Enabled().Returns(true);

            // Act
            ServiceEnabler sut = new(_logger, _modals, _appExitInvoker);
            await sut.GetServiceEnabledResultAsync(service);

            // Assert
            await _modals.Received(0).ShowAsync<IModal>();
        }

        [TestMethod]
        public async Task ItShouldNotTryToEnableServiceWhenUserClosedTheModal()
        {
            // Arrange
            IService service = Substitute.For<IService>();
            service.Enabled().Returns(false);
            _modals.ShowAsync<IModal>().Returns(false);

            // Act
            ServiceEnabler sut = new(_logger, _modals, _appExitInvoker);
            try
            {
                await sut.GetServiceEnabledResultAsync(service);
                Assert.Fail("The method should have thrown an exception to simulate the app kill, as defined in the Initialize() method");
            }
            // Assert
            catch (Exception ex)
            {
                Assert.AreEqual(APP_KILL_EXCEPTION_MESSAGE, ex.Message);
            }

            service.Received(0).Enable();
        }
    }
}