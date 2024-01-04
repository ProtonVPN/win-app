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
using NUnit.Framework;
using NUnit.Framework.Constraints;
using ProtonVPN.UI.Tests.Results;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.TestsHelper.BTI;
using ProtonVPN.UI.Tests.Windows;

namespace ProtonVPN.UI.Tests.Tests
{
    [TestFixture]
    [Category("BTI")]
    public class BtiTests : TestSession
    {
        private LoginWindow _loginWindow = new();
        private HomeWindow _homeWindow = new();
        private LoginResult _loginResult = new();
        private HomeResult _homeResult = new();
        private SysTrayWindow _trayWindow = new();

        private TimeSpan _minimalRefreshInterval = TimeSpan.FromSeconds(10);
        private string _vpnInfoIntervalName = "VpnInfoCheckInterval";

        [SetUp]
        public async Task TestInitializeAsync()
        {
            await ResetEnvAsync();
            DeleteUserConfig();
        }

        [Test]
        public async Task CertificateGenerationFailureOnLogin()
        {
            LaunchApp();

            await BtiController.SetScenarioAsync(Scenarios.CERTIFICATE_ENDPOINT_503);

            _loginWindow.SignIn(TestUserData.GetPlusUserBTI());
            _homeWindow.PressQuickConnectButton();
            _homeResult.CertificateErrorIsDisplayed();
            _homeWindow.PressCloseButton();

            await BtiController.SetScenarioAsync(Scenarios.RESET);

            _homeWindow.PressQuickConnectButton()
                .WaitUntilConnected();
        }

        [Test]
        public async Task DowngradeUser()
        {
            string testUser = $"test{new Random().Next()}";
            string userId = await AtlasController.SeedUserAsync(testUser);

            ConfigHelper.SetInterval(_vpnInfoIntervalName, _minimalRefreshInterval);

            LaunchApp();

            _loginWindow.SignIn(testUser, "a");
            await AtlasController.ExecuteQuarkAsync(Scenarios.DOWNGRADE_USER(userId));

            ReopenWindow(_minimalRefreshInterval);

            _homeResult.DowngradeModalIsDisplayed();
            _homeWindow.PressNoThanks()
                .PressGotIt();
            _homeWindow.NavigateToAccount();
            _homeResult.CheckIfUsernameIsDisplayedInAccount(testUser)
                .CheckIfCorrectPlanIsDisplayed("Proton VPN Free");
        }

        [Test]
        public async Task ForceUpgradeOnLogin()
        {
            LaunchApp();

            await BtiController.SetScenarioAsync(Scenarios.FORCE_UPDATE);

            _loginWindow.EnterCredentials(TestUserData.GetPlusUserBTI());
            _loginResult.CheckIfForceUpdateModalIsDisplayed();
        }

        [Test]
        public async Task ForceUpdateWhenLoggedIn()
        {
            ConfigHelper.SetInterval(_vpnInfoIntervalName, _minimalRefreshInterval);

            LaunchApp();
            _loginWindow.SignIn(TestUserData.GetPlusUserBTI());

            await BtiController.SetScenarioAsync(Scenarios.FORCE_UPDATE);

            ReopenWindow(_minimalRefreshInterval);

            _loginResult.CheckIfForceUpdateModalIsDisplayed();

            App.Close();
            LaunchApp();

            _loginResult.CheckIfLoginWindowIsDisplayed();
        }

        [TearDown]
        public async Task TestCleanupAsync()
        {
            ClientCleanup();
            //Cooldown to make API call after VPN disconnection
            Thread.Sleep(2000);
            await ResetEnvAsync();
        }

        private async Task ResetEnvAsync()
        {
            await BtiController.SetScenarioAsync(Scenarios.UNHARDJAIL_ALL);
            await BtiController.SetScenarioAsync(Scenarios.RESET);
            await AtlasController.ExecuteQuarkAsync(Scenarios.ATLAS_UNJAIL_ALL);
        }

        private void ReopenWindow(TimeSpan refreshInterval)
        {
            _homeWindow.MinimizeApp();
            Thread.Sleep(refreshInterval);
            _trayWindow.ClickOnProtonIcon();
        }
    } 
}
