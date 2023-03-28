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
    [Category("UI")]
    public class AccountTests : TestSession
    {
        private readonly HomeWindow _homeWindow = new HomeWindow();
        private readonly LoginWindow _loginWindow = new LoginWindow();
        private readonly HomeResult _homeResult = new HomeResult();

        [Test]
        public void UsernameIsDisplayedInAccountSection()
        {
            _loginWindow.SignIn(TestUserData.GetPlusUser());
            _homeWindow.NavigateToAccount();
            _homeResult.VerifyLoggedInAsTextIs(TestUserData.GetPlusUser().Username);
        }

        [Test]
        public void RestrictedAccountConnections()
        {
            _loginWindow.SignIn(TestUserData.GetFreeUser());

            _homeWindow.MoveMouseOnCountry("Canada");
            _homeResult.CheckIfConnectButtonIsNotDisplayed();

            _homeWindow.NavigateToProfilesTab()
                .ConnectViaProfile("SecureCore");
            _homeResult.CheckIfUpgradeRequiredModalIsShownSecureCore();
            _homeWindow.ClickWindowsCloseButton();

            _homeWindow.ConnectViaProfile("PaidCountry");
            _homeResult.CheckIfUpgradeRequiredModalIsShown();
            _homeWindow.ClickWindowsCloseButton();

            _homeWindow.PerformConnectionViaMap("MX");
            _homeResult.CheckIfUpgradeRequiredModalIsShown();
        }

        [SetUp]
        public void SetUp()
        {
            DeleteUserConfig();
            LaunchApp();
        }

        [TearDown]
        public void CleanUp()
        {
            Cleanup();
        }
    }
}
