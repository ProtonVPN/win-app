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

using System.Threading.Tasks;
using ProtonVPN.UI.Test.ApiClient;
using ProtonVPN.UI.Test.Windows;
using ProtonVPN.UI.Test.Results;
using ProtonVPN.UI.Test.TestsHelper;
using NUnit.Framework;

namespace ProtonVPN.UI.Test.Tests
{
    [TestFixture]
    public class ProfileTests : UITestSession
    {
        private readonly LoginWindow _loginWindow = new LoginWindow();
        private readonly MainWindow _mainWindow = new MainWindow();
        private readonly MainWindowResults _mainWindowResults = new MainWindowResults();
        private readonly ProfileWindow _profileWindow = new ProfileWindow();
        private readonly ProfileResult _profileResult = new ProfileResult();

        [Test]
        public void DefaultProfilesOptions()
        {
            TestCaseId = 234;

            _loginWindow.LoginWithPlusUser();

            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickProfiles();

            _profileResult.VerifyWindowIsOpened();
        }

        [Test]
        public void TryToCreateProfileWithoutName()
        {
            TestCaseId = 235;

            _loginWindow.LoginWithPlusUser();

            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickProfiles();

            _profileWindow.ClickToCreateNewProfile()
                .EnterProfileName("")
                .ClickSaveButton();
            _profileResult.VerifyProfileNameErrorDisplayed();
        }

        [Test]
        public void TryToCreateProfileWithoutCountry()
        {
            TestCaseId = 235;

            _loginWindow.LoginWithPlusUser();

            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickProfiles();

            _profileWindow.ClickToCreateNewProfile()
                .EnterProfileName("Standard Profile")
                .ClickSaveButton();
            _profileResult.VerifyCountryErrorDisplayed();
        }

        [Test]
        public void TryToCreateStandardProfile()
        {
            TestCaseId = 235;

            _loginWindow.LoginWithPlusUser();

            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickProfiles();

            _profileWindow.ClickToCreateNewProfile()
                .EnterProfileName("@Standard Profile")
                .SelectCountryFromList("Belgium")
                .SelectServerFromList("BE#1")
                .ClickSaveButton();

            _profileResult.VerifyProfileExists("@Standard Profile");
        }

        [Test]
        public void TryToCreateSecureCoreProfile()
        {
            TestCaseId = 236;

            _loginWindow.LoginWithPlusUser();

            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickProfiles();

            _profileWindow.ClickToCreateNewProfile()
                .ClickSecureCore()
                .EnterProfileName("@Secure Core Profile")
                .SelectCountryFromList("Belgium")
                .SelectServerFromList("IS-BE#1")
                .ClickSaveButton();

            _profileResult.VerifyProfileExists("@Secure Core Profile");
        }

        [Test]
        public void TryToCreateP2PProfile()
        {
            TestCaseId = 21553;

            _loginWindow.LoginWithPlusUser();

            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickProfiles();

            _profileWindow.ClickToCreateNewProfile()
                .ClickP2P()
                .EnterProfileName("@P2P Profile")
                .SelectServerFromList("CH#5")
                .ClickSaveButton();

            _profileResult.VerifyProfileExists("@P2P Profile");
        }

        [Test]
        public void TryToCreateTorProfile()
        {
            TestCaseId = 21552;

            _loginWindow.LoginWithPlusUser();

            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickProfiles();

            _profileWindow.ClickToCreateNewProfile()
                .ClickTor()
                .EnterProfileName("@Tor Profile")
                .SelectServerFromList("CH#18-TOR")
                .ClickSaveButton();

            _profileResult.VerifyProfileExists("@Tor Profile");
        }

        [Test]
        public void DeleteProfile()
        {
            TestCaseId = 239;

            var profileName = "@ProfileToDelete";
            _loginWindow.LoginWithPlusUser();

            _mainWindow.ClickHamburgerMenu().HamburgerMenu.ClickProfiles();

            _profileWindow.ClickToCreateNewProfile()
                .EnterProfileName(profileName)
                .SelectCountryFromList("Belgium")
                .SelectServerFromList("BE#1")
                .ClickSaveButton();

            _profileWindow.DeleteProfileByByName(profileName);
            _profileWindow.ClickContinueDeletion();
            _profileResult.VerifyProfileDoesNotExist(profileName);
        }

        [Test]
        public void EditProfile()
        {
            TestCaseId = 238;

            var profileName = "@ProfileToEdit";
            var newProfileName = "@ProfileToEdit-v2";

            _loginWindow.LoginWithPlusUser();
            _mainWindow.ClickHamburgerMenu().HamburgerMenu.ClickProfiles();

            _profileWindow.ClickToCreateNewProfile()
                .EnterProfileName(profileName)
                .SelectCountryFromList("Belgium")
                .SelectServerFromList("BE#1")
                .ClickSaveButton();


            _profileWindow.ClickEditProfile(profileName)
                .ClearProfileName()
                .EnterProfileName(newProfileName)
                .ClickSaveButton();

            _profileResult.VerifyProfileExists(newProfileName);
        }

        [Test]
        public void ConnectToCreatedProfile()
        {
            TestCaseId = 21551;

            var profileName = "@ProfileToConnect";

            _loginWindow.LoginWithPlusUser();
            _mainWindow.ClickHamburgerMenu().HamburgerMenu.ClickProfiles();
            
            _profileWindow.ClickToCreateNewProfile()
                .EnterProfileName(profileName)
                .SelectCountryFromList("Belgium")
                .SelectServerFromList("BE#1")
                .ClickSaveButton();


            _profileWindow.ConnectToProfile(profileName);
            _mainWindowResults.CheckIfConnected();
        }

        [Test]
        public void DiscardNewProfile()
        {
            TestCaseId = 21550;

            _loginWindow.LoginWithPlusUser();

            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickProfiles();


            _profileWindow.ClickToCreateNewProfile()
                .EnterProfileName("@ProfileToDiscard")
                .SelectCountryFromList("Belgium")
                .SelectServerFromList("BE#1")
                .ClickToCancel()
                .ClickToDiscard();

            _profileResult.VerifyProfileDoesNotExist("@ProfileToDiscard");
        }

        [SetUp]
        public async Task TestInitialize()
        {
            CreateSession();

            var api = new Api(TestUserData.GetPlusUser().Username, TestUserData.GetPlusUser().Password);
            await api.Login();
            await api.DeleteProfiles();
        }

        [TearDown]
        public void TestCleanup()
        {
            TearDown();
        }
    }
}
