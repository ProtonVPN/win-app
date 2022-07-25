/*
 * Copyright (c) 2022 Proton Technologies AG
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
using ProtonVPN.UI.Test.Results;
using ProtonVPN.UI.Test.TestsHelper;
using ProtonVPN.UI.Test.Windows;

namespace ProtonVPN.UI.Test.Tests
{
    [TestFixture]
    [Category("UI")]
    public class SettingsTests : TestSession
    {
        private readonly LoginWindow _loginWindow = new LoginWindow();
        private readonly SettingsWindow _settingsWindow = new SettingsWindow();
        private readonly SettingsResult _settingsResult = new SettingsResult();

        [Test]
        public void CheckIfSettingsGeneralTabHasAllInfo()
        {
            TestCaseId = 21555;

            _settingsResult.CheckIfSettingsAreDisplayed();
        }

        [Test]
        public void CheckIfInvalidDnsIsNotPermitted()
        {
            TestCaseId = 4580;

            _settingsWindow.NavigateToConnectionTab()
                .ClickOnCustomDnsCheckBox()
                .PressContinueToDisableNetshield()
                .EnterCustomDnsAddress("1.A.B.4");
            _settingsResult.CheckIfCustomDnsAddressWasNotAdded();
        }

        [SetUp]
        public void TestInitialize()
        {
            DeleteUserConfig();
            LaunchApp();
            _loginWindow.SignIn(TestUserData.GetPlusUser())
                .NavigateToSettings();
        }

        [TearDown]
        public void TestCleanup()
        {
            Cleanup();
        }
    }
}
