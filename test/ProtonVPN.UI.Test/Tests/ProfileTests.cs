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

using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.UI.Test.ApiClient;
using ProtonVPN.UI.Test.Pages;
using ProtonVPN.UI.Test.TestsHelper;
using Profile = ProtonVPN.UI.Test.Actions.Profile;

namespace ProtonVPN.UI.Test.Tests
{
    [TestClass]
    public class ProfileTests : Profile
    {
        readonly LoginWindow loginWindow = new LoginWindow();
        readonly MainWindow mainWindow = new MainWindow();
        readonly ProfileWindow profileWindow = new ProfileWindow();

        [TestMethod]
        public void DefaultProfilesOptions()
        {
            loginWindow.LoginWithPlusUser();
            mainWindow.ClickHamburgerMenu().HamburgerMenu.ClickProfiles();
            profileWindow.VerifyWindowIsOpened();
        }

        [TestMethod]
        public void TryToCreateProfileWithoutName()
        {
            loginWindow.LoginWithPlusUser();

            mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickProfiles();

            profileWindow.ClickToCreateNewProfile()
                .EnterProfileName("")
                .ClickSaveButton()
                .VerifyProfileNameErrorDisplayed();
        }

        [TestMethod]
        public void TryToCreateProfileWithoutCountry()
        {
            loginWindow.LoginWithPlusUser();

            mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickProfiles();

            profileWindow.ClickToCreateNewProfile()
                .EnterProfileName("Standard Profile")
                .ClickSaveButton()
                .VerifyCountryErrorDisplayed();
        }

        [TestMethod]
        public void TryToCreateStandardProfile()
        {
            loginWindow.LoginWithPlusUser();

            mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickProfiles();

            profileWindow.ClickToCreateNewProfile()
                .EnterProfileName("@Standard Profile")
                .SelectCountryFromList("Belgium")
                .SelectServerFromList("BE#1")
                .ClickSaveButton();

            Thread.Sleep(1000);

            profileWindow.VerifyProfileExists("@Standard Profile");
        }

        [TestMethod]
        public void TryToCreateSecureCoreProfile()
        {
            loginWindow.LoginWithPlusUser();

            mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickProfiles();

            profileWindow.ClickToCreateNewProfile()
                .ClickSecureCore()
                .EnterProfileName("@Secure Core Profile")
                .SelectCountryFromList("Belgium")
                .SelectServerFromList("IS-BE#1")
                .ClickSaveButton();
            Thread.Sleep(1000);

            profileWindow.VerifyProfileExists("@Secure Core Profile");
        }

        [TestMethod]
        public void TryToCreateP2PProfile()
        {
            loginWindow.LoginWithPlusUser();

            mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickProfiles();

            profileWindow.ClickToCreateNewProfile()
                .ClickP2P()
                .EnterProfileName("@P2P Profile")
                .SelectServerFromList("CH#5")
                .ClickSaveButton();
            Thread.Sleep(1000);

            profileWindow.VerifyProfileExists("@P2P Profile");
        }

        [TestMethod]
        public void TryToCreateTorProfile()
        {
            loginWindow.LoginWithPlusUser();

            mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickProfiles();

            profileWindow.ClickToCreateNewProfile()
                .ClickTor()
                .EnterProfileName("@Tor Profile")
                .SelectServerFromList("CH#18-TOR")
                .ClickSaveButton();

            profileWindow.VerifyProfileExists("@Tor Profile");
        }

        [TestMethod]
        public void DeleteProfile()
        {
            var profileName = "@ProfileToDelete";
            loginWindow.LoginWithPlusUser();

            mainWindow.ClickHamburgerMenu().HamburgerMenu.ClickProfiles();

            profileWindow.ClickToCreateNewProfile()
                .EnterProfileName(profileName)
                .SelectCountryFromList("Belgium")
                .SelectServerFromList("BE#1")
                .ClickSaveButton();

            Thread.Sleep(2000);

            profileWindow.DeleteProfileByByName(profileName);
            Thread.Sleep(1000);
            profileWindow.ClickContinueDeletion();
            profileWindow.VerifyProfileDoesNotExist(profileName);
        }

        [TestMethod]
        public void EditProfile()
        {
            var profileName = "@ProfileToEdit";
            var newProfileName = "@ProfileToEdit-v2";

            loginWindow.LoginWithPlusUser();
            mainWindow.ClickHamburgerMenu().HamburgerMenu.ClickProfiles();

            profileWindow.ClickToCreateNewProfile()
                .EnterProfileName(profileName)
                .SelectCountryFromList("Belgium")
                .SelectServerFromList("BE#1")
                .ClickSaveButton();

            Thread.Sleep(500);

            profileWindow.ClickEditProfile(profileName)
                .ClearProfileName()
                .EnterProfileName(newProfileName)
                .ClickSaveButton();

            Thread.Sleep(500);
            profileWindow.VerifyProfileExists(newProfileName);
        }

        [TestMethod]
        public void ConnectToCreatedProfile()
        {
            var profileName = "@ProfileToConnect";

            loginWindow.LoginWithPlusUser();
            mainWindow.ClickHamburgerMenu().HamburgerMenu.ClickProfiles();
            
            profileWindow.ClickToCreateNewProfile()
                .EnterProfileName(profileName)
                .SelectCountryFromList("Belgium")
                .SelectServerFromList("BE#1")
                .ClickSaveButton();

            Thread.Sleep(1000);

            profileWindow.ConnectToProfile(profileName);
            mainWindow.VerifyConnecting();
        }

        [TestMethod]
        public void DiscardNewProfile()
        {
            loginWindow.LoginWithPlusUser();

            mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickProfiles();

            Thread.Sleep(200);

            profileWindow.ClickToCreateNewProfile()
                .EnterProfileName("@ProfileToDiscard")
                .SelectCountryFromList("Belgium")
                .SelectServerFromList("BE#1")
                .ClickToCancel()
                .ClickToDiscard();

            profileWindow.VerifyProfileDoesNotExist("@ProfileToDiscard");
        }

        [TestInitialize]
        public async Task TestInitialize()
        {
            CreateSession();

            var api = new Api(TestUserData.GetPlusUser().Username, TestUserData.GetPlusUser().Password);
            await api.Login();
            await api.DeleteProfiles();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            TearDown();
        }
    }
}
