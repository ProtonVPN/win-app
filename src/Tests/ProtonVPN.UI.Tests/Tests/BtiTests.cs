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
        private LoginWindow _loginWindow = new LoginWindow();
        private HomeWindow _homeWindow = new HomeWindow();
        private LogsResult _logsResult = new LogsResult();
        private HomeResult _homeResult = new HomeResult();
        private SysTrayWindow _trayWindow = new SysTrayWindow();

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

            await AtlasController.MockApiAsync(ApiMocks.CERTIFICATE_ERROR_503);

            _loginWindow.SignIn(TestUserData.GetPlusUserBTI());
            _homeWindow.PressQuickConnectButton();
            _homeResult.CertificateErrorIsDisplayed();
            _homeWindow.PressCloseButton();

            await AtlasController.MockApiAsync("");

            _homeWindow.PressQuickConnectButton()
                .WaitUntilConnected();
        }

        [Test]
        public async Task DowngradeUser()
        {
            string testUser = $"test{new Random().Next()}";
            TimeSpan refreshInterval = TimeSpan.FromSeconds(10);

            GenerateAppConfig();
            ConfigHelper.SetInterval("VpnInfoCheckInterval", refreshInterval);
            string userId = await AtlasController.SeedUserAsync(testUser);

            LaunchApp();

            _loginWindow.SignIn(testUser, "a");
            await AtlasController.ExecuteQuarkAsync(Scenarios.DOWNGRADE_USER(userId));

            Thread.Sleep(refreshInterval);
            _homeWindow.MinimizeApp();
            Thread.Sleep(1000);
            _trayWindow.ClickOnProtonIcon();

            _homeResult.DowngradeModalIsDisplayed();
            _homeWindow.PressNoThanks()
                .PressGotIt();
            _homeWindow.NavigateToAccount();
            _homeResult.CheckIfUsernameIsDisplayedInAccount(testUser)
                .CheckIfCorrectPlanIsDisplayed("Proton VPN Free");
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
            await AtlasController.MockApiAsync("");
        }
    } 
}
