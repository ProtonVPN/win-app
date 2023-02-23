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

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.OS.Registry;
using ProtonVPN.Core.Startup;

namespace ProtonVPN.App.Tests.Core.Startup
{
    [TestClass]
    public class AutoStartupTest
    {
        private ICurrentUserStartupRecord _startupRecord;

        [TestInitialize]
        public void TestInitialize()
        {
            _startupRecord = Substitute.For<ICurrentUserStartupRecord>();
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Enabled_ShouldBe_StartupRecord_Exists(bool value)
        {
            _startupRecord.Exists().Returns(value);
            AutoStartup startup = new AutoStartup(_startupRecord);

            bool result = startup.Enabled;

            _startupRecord.Received().Exists();
            result.Should().Be(value);
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
        public void Enabled_ShouldCreate_StartupRecord_WhenItNotExists()
        {
            _startupRecord.Exists().Returns(false);
            AutoStartup startup = new AutoStartup(_startupRecord);

            startup.Enabled = true;

            _startupRecord.Received().Create();
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
        public void Enabled_ShouldNotCreate_StartupRecord_WhenItExists_AndIsValid()
        {
            _startupRecord.Exists().Returns(true);
            _startupRecord.Valid().Returns(true);
            AutoStartup startup = new AutoStartup(_startupRecord);

            startup.Enabled = true;

            _startupRecord.DidNotReceive().Create();
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
        public void Enabled_ShouldReCreate_StartupRecord_WhenItExists_AndIsNotValid()
        {
            _startupRecord.Exists().Returns(true);
            _startupRecord.Valid().Returns(false);
            AutoStartup startup = new AutoStartup(_startupRecord);

            startup.Enabled = true;

            _startupRecord.Received().Remove();
            _startupRecord.Received().Create();
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
        public void Enabled_ShouldRemove_StartupRecord_WhenItExists()
        {
            _startupRecord.Exists().Returns(true);
            AutoStartup startup = new AutoStartup(_startupRecord);

            startup.Enabled = false;

            _startupRecord.Received().Remove();
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
        public void Enabled_ShouldNotRemove_StartupRecord_WhenItNotExists()
        {
            _startupRecord.Exists().Returns(false);
            AutoStartup startup = new AutoStartup(_startupRecord);

            startup.Enabled = false;

            _startupRecord.DidNotReceive().Remove();
        }
    }
}
