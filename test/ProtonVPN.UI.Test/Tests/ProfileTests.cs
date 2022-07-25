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
    public class ProfileTests : TestSession
    {
        private readonly LoginWindow _loginWindow = new LoginWindow();
        private readonly HomeWindow _homeWindow = new HomeWindow();
        private readonly ProfilesWindow _profileWindow = new ProfilesWindow();
        private readonly ProfilesResult _profileResult = new ProfilesResult();

        [Test]
        public void TryToCreateStandardProfile()
        {
            TestCaseId = 235;
            _profileWindow.PressCreateNewProfile()
                .CreateProfile(TestConstants.ProfileName);
            _profileResult.CheckIfProfileIsDisplayed(TestConstants.ProfileName);
        }

        [Test]
        public void DefaultProfilesOptions()
        {
            TestCaseId = 234;

            _profileResult.CheckIfDefaultProfilesAreDisplayed();
        }

        [Test]
        public void TryToCreateProfileWithoutName()
        {
            TestCaseId = 235;

            _profileWindow.PressCreateNewProfile()
                .CreateProfile("");
            _profileResult.CheckIfProfileNameErrorDisplayed();
        }

        [Test]
        public void TryToCreateProfileWithoutCountry()
        {
            TestCaseId = 235;

            _profileWindow.PressCreateNewProfile()
                .CreateStandartProfileWithoutCountry(TestConstants.ProfileName);
            _profileResult.CheckIfCountryErrorDisplayed();
        }

        [Test]
        public void TryToCreateSecureCoreProfile()
        {
            TestCaseId = 236;

            _profileWindow.PressCreateNewProfile()
                .SelectSecureCoreOption()
                .CreateProfile(TestConstants.ProfileName);      
            _profileResult.CheckIfProfileIsDisplayed(TestConstants.ProfileName);
        }

        [Test]
        public void TryToCreateP2PProfile()
        {
            TestCaseId = 21553;

            _profileWindow.PressCreateNewProfile()
               .SelectP2POption()
               .CreateProfileWithoutCountry(TestConstants.ProfileName);
            _profileResult.CheckIfProfileIsDisplayed(TestConstants.ProfileName);
        }

        [Test]
        public void TryToCreateTorProfile()
        {
            TestCaseId = 21552;

            _profileWindow.PressCreateNewProfile()
               .SelectTorOption()
               .CreateProfileWithoutCountry(TestConstants.ProfileName);
            _profileResult.CheckIfProfileIsDisplayed(TestConstants.ProfileName);
        }

        [Test]
        public void DeleteProfile()
        {
            TestCaseId = 239;

            _profileWindow.PressCreateNewProfile()
                .CreateProfile(TestConstants.ProfileName);
            _profileResult.CheckIfProfileIsDisplayed(TestConstants.ProfileName);

            _profileWindow.DeleteProfileByByName(TestConstants.ProfileName);
            _profileResult.CheckIfProfileIsNotDisplayed(TestConstants.ProfileName);
        }

        [Test]
        public void EditProfile()
        {
            TestCaseId = 238;

            string newProfileName = "@ProfileToEdit-v2";

            _profileWindow.PressCreateNewProfile()
                .CreateProfile(TestConstants.ProfileName);
            _profileResult.CheckIfProfileIsDisplayed(TestConstants.ProfileName);


            _profileWindow.EditProfileName(TestConstants.ProfileName, newProfileName);
            _profileResult.CheckIfProfileIsNotDisplayed(newProfileName);
        }

        [Test]
        public void DiscardNewProfile()
        {
            TestCaseId = 21550;

            _profileWindow.PressCreateNewProfile()
                .EnterProfileName(TestConstants.ProfileName)
                .DiscardProfile();

            _profileResult.CheckIfProfileIsNotDisplayed(TestConstants.ProfileName);
        }

        [SetUp]
        public void TestInitialize()
        {
            DeleteUserConfig();
            DeleteProfiles();
            LaunchApp();
            _loginWindow.SignIn(TestUserData.GetPlusUser());
            _homeWindow.NavigateToProfiles();
        }

        [TearDown]
        public void TestCleanup()
        {
            Cleanup();
        }
    }
}
