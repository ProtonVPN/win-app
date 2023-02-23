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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Update.Files.Launchable;

namespace ProtonVPN.Update.Tests.Files.Launchable
{
    [TestClass]
    public class LaunchableFileTest
    {
        private IOsProcesses _processes;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _processes = Substitute.For<IOsProcesses>();
        }

        [TestMethod]
        public void Launch_ShouldCreate_ElevatedProcess()
        {
            const string filename = "File to launch";
            const string args = "Launch arguments";
            LaunchableFile launchable = new LaunchableFile(_processes);

            launchable.Launch(filename, args);

            _processes.Received(1).ElevatedProcess(filename, args);
        }

        [TestMethod]
        public void Launch_ShouldStart_ElevatedProcess()
        {
            LaunchableFile launchable = new LaunchableFile(_processes);

            launchable.Launch("", "");

            _processes.ElevatedProcess("", "").Received(1).Start();
        }

        [TestMethod]
        public void Valid_ShouldPassException_WhenOsProcesses_ElevatedProcess_Throws()
        {
            _processes.ElevatedProcess("", "").ThrowsForAnyArgs<Exception>();
            LaunchableFile launchable = new LaunchableFile(_processes);

            Action action = () => launchable.Launch("", "");

            action.Should().Throw<Exception>();
        }

        [TestMethod]
        public void Valid_ShouldPassException_WhenElevatedProcess_Start_Throws()
        {
            _processes.ElevatedProcess("", "").WhenForAnyArgs(x => x.Start()).Throws<Exception>();
            LaunchableFile launchable = new LaunchableFile(_processes);

            Action action = () => launchable.Launch("", "");

            action.Should().Throw<Exception>();
        }
    }
}
