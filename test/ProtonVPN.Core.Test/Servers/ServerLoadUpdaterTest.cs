using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Window;

namespace ProtonVPN.Core.Test.Servers
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
        private readonly SyncSingleTask _task = new SyncSingleTask();

        [TestInitialize]
        public void Initialize()
        {
            var userStorage = Substitute.For<IUserStorage>();
            _serverManager = Substitute.For<ServerManager>(userStorage);
            _scheduler = Substitute.For<IScheduler>();
            _eventAggregator = Substitute.For<IEventAggregator>();
            _mainWindowState = Substitute.For<IMainWindowState>();
            _apiServers = Substitute.For<IApiServers>();
            _singleActionFactory = Substitute.For<ISingleActionFactory>();
            _lastServerLoadTimeProvider = Substitute.For<ILastServerLoadTimeProvider>();
        }

        [TestMethod]
        public void ItShouldStartUpdatingServerLoads()
        {
            // Arrange
            var sut = GetServerLoadUpdater(DateTime.Now.Subtract(TimeSpan.FromHours(1)));

            // Act
            sut.Handle(new WindowStateMessage(true));

            // Assert
            _task.Started.Should().BeTrue();
        }

        [TestMethod]
        public void ItShouldNotStartUpdatingServerLoads()
        {
            // Arrange
            var sut = GetServerLoadUpdater(DateTime.Now);

            // Act
            sut.Handle(new WindowStateMessage(true));

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
