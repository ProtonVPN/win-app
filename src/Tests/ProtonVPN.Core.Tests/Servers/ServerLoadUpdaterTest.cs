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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Windows;

namespace ProtonVPN.Core.Tests.Servers
{
    [TestClass]
    public class ServerLoadUpdaterTest
    {
        private ServerManager _serverManager;
        private IScheduler _scheduler;
        private IEventAggregator _eventAggregator;
        private IMainWindowState _mainWindowState;
        private IApiServers _apiServers;
        private ISingleActionFactory _singleActionFactory;
        private ILastServerLoadTimeProvider _lastServerLoadTimeProvider;
        private readonly SyncSingleTask _task = new();

        [TestInitialize]
        public void Initialize()
        {
            IUserStorage userStorage = Substitute.For<IUserStorage>();
            IAppSettings appSettings = Substitute.For<IAppSettings>();
            ILogger logger = Substitute.For<ILogger>();
            _serverManager = Substitute.For<ServerManager>(userStorage, appSettings, logger);
            _scheduler = Substitute.For<IScheduler>();
            _eventAggregator = Substitute.For<IEventAggregator>();
            _mainWindowState = Substitute.For<IMainWindowState>();
            _apiServers = Substitute.For<IApiServers>();
            _singleActionFactory = Substitute.For<ISingleActionFactory>();
            _lastServerLoadTimeProvider = Substitute.For<ILastServerLoadTimeProvider>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _serverManager = null;
            _scheduler = null;
            _eventAggregator = null;
            _mainWindowState = null;
            _apiServers = null;
            _singleActionFactory = null;
            _lastServerLoadTimeProvider = null;
        }

        [TestMethod]
        public async Task ItShouldStartUpdatingServerLoads()
        {
            // Arrange
            ServerLoadUpdater sut = GetServerLoadUpdater(DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)));

            // Act
            await sut.HandleAsync(new WindowStateMessage(true, WindowState.Normal), new CancellationTokenSource().Token);

            // Assert
            _task.Started.Should().BeTrue();
        }

        [TestMethod]
        public async Task ItShouldNotStartUpdatingServerLoads()
        {
            // Arrange
            ServerLoadUpdater sut = GetServerLoadUpdater(DateTime.UtcNow);

            // Act
            await sut.HandleAsync(new WindowStateMessage(true, WindowState.Normal), new CancellationTokenSource().Token);

            // Assert
            _task.Started.Should().BeFalse();
        }

        private ServerLoadUpdater GetServerLoadUpdater(DateTime lastCheck)
        {
            var interval = TimeSpan.FromSeconds(10);

            _lastServerLoadTimeProvider.LastChecked().Returns(lastCheck);
            _singleActionFactory.GetSingleAction(Arg.Any<Func<Task>>()).Returns(_task);

            return new ServerLoadUpdater(
                interval,
                _serverManager,
                _scheduler,
                _eventAggregator,
                _mainWindowState,
                _apiServers,
                _singleActionFactory,
                _lastServerLoadTimeProvider);
        }
    }

    internal class SyncSingleTask : ISingleAction
    {
        public bool Started;

        public event EventHandler<TaskCompletedEventArgs> Completed;

        public Task Task { get; }

        public bool IsRunning { get; }

        public Task Run()
        {
            Started = true;
            return Task.CompletedTask;
        }

        public void Cancel() => throw new NotImplementedException();
    }
}
