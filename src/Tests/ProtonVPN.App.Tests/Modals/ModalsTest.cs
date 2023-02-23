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
using System.Dynamic;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Modals;
using ProtonVPN.Modals;
using ProtonVPN.Resource;

namespace ProtonVPN.App.Tests.Modals
{
    [TestClass]
    public class ModalsTest
    {
        private IScheduler _scheduler;
        private IComponentContext _container;
        private IWindowManager _windowManager;
        private IModal _modal;
        private IModalWindows _modalWindows;

        [TestInitialize]
        public void TestInitialize()
        {
            _scheduler = Substitute.For<IScheduler>();
            _scheduler.Schedule(Arg.Any<Func<bool?>>()).Returns(c => c.Arg<Func<bool?>>()());

            _windowManager = Substitute.For<IWindowManager>();
            _modalWindows = Substitute.For<IModalWindows>();
            _modal = Substitute.For<IModal>();

            ContainerBuilder builder = new ContainerBuilder();
            builder.Register(c => new WrappedModal(_modal)).AsSelf().SingleInstance();
            _container = builder.Build();
        }

        [TestMethod]
        public void Show_ShouldCall_Modal_BeforeOpenModal()
        {
            ProtonVPN.Modals.Modals modals = new ProtonVPN.Modals.Modals(_scheduler, _container, _windowManager, _modalWindows);
            dynamic options = new ExpandoObject();

            modals.Show<WrappedModal>(options);

            _modal.Received(1).BeforeOpenModal(options);
        }

        [TestMethod]
        public void Show_ShouldCall_WindowManager_ShowDialog()
        {
            WrappedModal wrappedModal = _container.Resolve<WrappedModal>();
            ProtonVPN.Modals.Modals modals = new ProtonVPN.Modals.Modals(_scheduler, _container, _windowManager, _modalWindows);

            modals.Show<WrappedModal>();

            _windowManager.Received(1).ShowDialog(
                wrappedModal,
                Arg.Is<object>(o => o == null),
                Arg.Any<IDictionary<string, object>>());
        }

        [TestMethod]
        public void Show_ShouldCall_WindowManager_ShowDialog_WithDefaultSettings()
        {
            ProtonVPN.Modals.Modals modals = new ProtonVPN.Modals.Modals(_scheduler, _container, _windowManager, _modalWindows);

            modals.Show<WrappedModal>();

            _windowManager.Received(1).ShowDialog(
                Arg.Any<object>(),
                Arg.Any<object>(),
                Arg.Is<IDictionary<string, object>>(d =>
                    (WindowStartupLocation) d["WindowStartupLocation"] == WindowStartupLocation.CenterOwner &&
                    (ResizeMode) d["ResizeMode"] == ResizeMode.NoResize));
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow(false)]
        [DataRow(true)]
        public void Close_ShouldCall_Dialog_TryClose(bool? dialogResult)
        {
            ProtonVPN.Modals.Modals modals = new ProtonVPN.Modals.Modals(_scheduler, _container, _windowManager, _modalWindows);

            modals.Close<WrappedModal>(dialogResult);

            _modal.Received(1).TryClose(dialogResult);
        }

        [TestMethod]
        public void CloseAll_ShouldCall_AllDialogs_TryClose()
        {
            ContainerBuilder builder = new ContainerBuilder();
            for (int i = 0; i < 3; i++)
            {
                builder.Register(c => Substitute.For<IModal>()).As<IModal>().SingleInstance();
            }
            _container = builder.Build();

            ProtonVPN.Modals.Modals modals = new ProtonVPN.Modals.Modals(_scheduler, _container, _windowManager, _modalWindows);

            modals.CloseAll();

            foreach (IModal modal in _container.Resolve<IEnumerable<IModal>>())
            {
                modal.Received(1).TryClose(false);
            }
        }

        [TestMethod]
        public void CloseAll_ShouldCall_OnlyDialogsWithNoStayOnTop_TryClose()
        {
            ContainerBuilder builder = new ContainerBuilder();
            for (int i = 0; i < 3; i++)
            {
                builder.Register(c => Substitute.For<IModal>()).As<IModal>().SingleInstance();
            }
            for (int i = 0; i < 3; i++)
            {
                IModal modal = Substitute.For<IModal>();
                modal.StayOnTop.Returns(true);
                builder.Register(c => modal).As<IModal>().SingleInstance();
            }
            _container = builder.Build();

            ProtonVPN.Modals.Modals modals = new ProtonVPN.Modals.Modals(_scheduler, _container, _windowManager, _modalWindows);

            modals.CloseAll();

            foreach (IModal modal in _container.Resolve<IEnumerable<IModal>>())
            {
                modal.Received(modal.StayOnTop ? 0 : 1).TryClose(false);
            }
        }

        [TestMethod]
        public void Show_ShouldReturnFalse_IfAlreadyOpened()
        {
            // Arrange
            WrappedModal modalViewModel = new WrappedModal(_modal);
            _modalWindows.List().Returns(new[]
            {
                new BaseModalWindow {DataContext = modalViewModel}
            });
            ProtonVPN.Modals.Modals modals = new ProtonVPN.Modals.Modals(_scheduler, _container, _windowManager, _modalWindows);

            // Assert
            modals.Show<WrappedModal>().Should().BeFalse("Modal already opened");
        }

        public class WrappedModal : IModal
        {
            private readonly IModal _modal;

            public WrappedModal(IModal modal)
            {
                _modal = modal;
            }

            public bool StayOnTop { get; protected set; }

            public void BeforeOpenModal(dynamic options) => _modal.BeforeOpenModal(options);

            public void TryClose(bool? dialogResult) => _modal.TryClose(dialogResult);
        }
    }
}
