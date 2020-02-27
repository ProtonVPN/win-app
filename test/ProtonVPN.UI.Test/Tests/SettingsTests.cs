/*
 * Copyright (c) 2020 Proton Technologies AG
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
using ProtonVPN.UI.Test.Pages;
using System.Threading;

namespace ProtonVPN.UI.Test.Tests
{
    [TestClass]
    public class SettingsTests : UITestSession
    {
        readonly LoginWindow loginWindow = new LoginWindow();
        readonly MainWindow mainWindow = new MainWindow();
        readonly SettingsWindow settingsWindow = new SettingsWindow();

        [TestMethod]
        public void CheckIfSettingsGeneralTabHasAllInfo()
        {
            loginWindow.LoginWithFreeUser();

            mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickSettings();

            settingsWindow.ClickGeneralTab()
                .VerifySettingsAreDisplayed();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            CreateSession();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            TearDown();
        }
    }
}
