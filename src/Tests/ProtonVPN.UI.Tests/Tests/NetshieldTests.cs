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

using NUnit.Framework;
using ProtonVPN.UI.Tests.Results;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.Windows;

namespace ProtonVPN.UI.Tests.Tests
{
    [TestFixture]
    [Category("Connection")]
    public class NetshieldTests : TestSession
    {
        private LoginWindow _loginWindow = new LoginWindow();
        private HomeWindow _homeWindow = new HomeWindow();
        private HomeResult _homeResult = new HomeResult();

        private const string NETSHIELD_NO_BLOCK = "netshield-0.protonvpn.net";
        private const string NETSHIELD_LEVEL_ONE = "netshield-1.protonvpn.net";
        private const string NETSHIELD_LEVEL_TWO = "netshield-2.protonvpn.net";

        [Test]
        [Category("Smoke")]
        public void NetshieldLevelTwo()
        {
            _homeWindow.EnableNetshieldLevelTwo()
                .PressQuickConnectButton()
                .WaitUntilConnected();

            _homeResult.CheckIfDnsIsResolved(NETSHIELD_NO_BLOCK)
                .CheckIfDnsIsNotResolved(NETSHIELD_LEVEL_ONE)
                .CheckIfDnsIsNotResolved(NETSHIELD_LEVEL_TWO);  
        }

        [Test]
        [Category("Smoke")]
        public void NetshieldLevelOne()
        {
            _homeWindow.EnableNetshieldLevelOne()
                .PressQuickConnectButton()
                .WaitUntilConnected();

            _homeResult.CheckIfDnsIsResolved(NETSHIELD_NO_BLOCK)
                .CheckIfDnsIsNotResolved(NETSHIELD_LEVEL_ONE)
                .CheckIfDnsIsResolved(NETSHIELD_LEVEL_TWO);
        }

        [Test]
        [Category("Smoke")]
        public void NetshieldOff()
        {
            _homeWindow.DisableNetshield()
                .PressQuickConnectButton()
                .WaitUntilConnected();

            _homeResult.CheckIfDnsIsResolved(NETSHIELD_NO_BLOCK)
                .CheckIfDnsIsResolved(NETSHIELD_LEVEL_ONE)
                .CheckIfDnsIsResolved(NETSHIELD_LEVEL_TWO);
        }

        [Test]
        public void EnableNetshieldWhileConnected()
        {
            _homeWindow.PressQuickConnectButton()
                .WaitUntilConnected();
            _homeResult.CheckIfDnsIsResolved(NETSHIELD_NO_BLOCK)
                .CheckIfDnsIsNotResolved(NETSHIELD_LEVEL_ONE)
                .CheckIfDnsIsResolved(NETSHIELD_LEVEL_TWO);

            NetworkUtils.DnsFlushResolverCache();

            _homeWindow.EnableNetshieldLevelTwo();
            _homeResult.CheckIfDnsIsResolved(NETSHIELD_NO_BLOCK)
                .CheckIfDnsIsNotResolved(NETSHIELD_LEVEL_ONE)
                .CheckIfDnsIsNotResolved(NETSHIELD_LEVEL_TWO);
        }


        [SetUp]
        public void TestInitialize()
        {
            DeleteUserConfig();
            LaunchApp();
            _loginWindow.SignIn(TestUserData.GetPlusUser());
        }

        [TearDown]
        public void TestCleanup()
        {
            Cleanup();
        }
    }
}
