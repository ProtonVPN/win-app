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

using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.Verification;

namespace ProtonVPN.UI.Tests.Tests
{
    [TestFixture]
    [Category("Connection")]

    public class ConnectionTests : TestSession
    {
        private HomeRobot _homeRobot = new HomeRobot();
        private HomeVerify _homeVerify = new HomeVerify();

        [SetUp]
        public void TestInitialize()
        {
            DeleteUserConfig();
            LaunchApp();
        }

        [Test]
        public void QuickConnect()
        {
            _homeRobot.QuickConnect();
            _homeVerify.CheckIfConnectionStateIsShown();
            _homeRobot.WaitUntilConnected()
                .Disconnect();
            _homeVerify.CheckIfDisconnected();
        }

        [Test]
        public void CancelConnection()
        {
            _homeRobot.QuickConnect();
            _homeVerify.CheckIfConnectionStateIsShown();
            _homeRobot.CancelConnection();
            _homeVerify.CheckIfDisconnected();
        }

        [Test]
        public void RecentsAreAddedAfterConnection()
        {
            _homeRobot.QuickConnect()
                .Disconnect()
                .ExpandRecents();
            //For some reason label has to contain whitespace at the end
            _homeVerify.CheckIfRecentIsShown("Fastest country ");
        }

        [Test]
        public void DeleteAllRecentConnections()
        {
            _homeRobot.QuickConnect()
                .Disconnect()
                .ExpandRecents()
                .DeleteRecent();
            _homeVerify.CheckIfRecentIsDeleted();
        }

        [TearDown]
        public void TestCleanup()
        {
            Cleanup();
        }
    }
}
