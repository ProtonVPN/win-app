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

using FlaUI.Core.AutomationElements;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Windows
{
    public class ProfilesWindow : UIActions
    {
        private Button CreateProfileButton => ElementByAutomationId("NewProfileButton").AsButton();
        private TextBox ProfileNameInput => ElementByAutomationId("textSource").AsTextBox();
        private ListBoxItem CountryFirstOption => ElementByAutomationId("CountryBox").AsComboBox().FindChildAt(1).AsListBoxItem();
        private ListBoxItem ServerFirstOption => ElementByAutomationId("ServerBox").AsComboBox().FindChildAt(2).AsListBoxItem();
        private Button SaveButton => ElementByName("Save").AsButton();
        private AutomationElement SecureCoreButton => ElementByName("Secure Core");
        private AutomationElement P2PButton => ElementByName("P2P");
        private AutomationElement TorButton => ElementByName("Tor");
        private Button ContinueButton => ElementByAutomationId("ContinueButton").AsButton();
        private Button DiscardlButton => ElementByName("Discard").AsButton();

        public ProfilesWindow CreateProfile(string profileName)
        {
            ProfileNameInput.Enter(profileName);
            CountryFirstOption.Select();
            ServerFirstOption.Select();
            SaveButton.Invoke();
            return this;
        }

        public ProfilesWindow EnterProfileName(string profileName)
        {
            ProfileNameInput.Enter(profileName);
            return this;
        }

        public ProfilesWindow CreateProfileWithoutCountry(string profileName)
        {
            ProfileNameInput.Enter(profileName);
            ServerFirstOption.Select();
            SaveButton.Invoke();
            return this;
        }

        public ProfilesWindow CreateStandartProfileWithoutCountry(string profileName)
        {
            ProfileNameInput.Enter(profileName);
            SaveButton.Invoke();
            return this;
        }

        public ProfilesWindow ConnectToProfile(string profileName)
        {
            WaitUntilElementExistsByAutomationIdAndReturnTheElement($"Connect-{profileName}", TestConstants.VeryShortTimeout).AsButton().Invoke();
            return this;
        }

        public ProfilesWindow SelectSecureCoreOption()
        {
            SecureCoreButton.Click();
            return this;
        }
        
        public ProfilesWindow SelectTorOption()
        {
            TorButton.Click();
            return this;
        }

        public ProfilesWindow SelectP2POption()
        {
            P2PButton.Click();
            return this;
        }

        public ProfilesWindow PressCreateNewProfile()
        {
            WaitUntilElementExistsByAutomationId("NewProfileButton", TestConstants.VeryShortTimeout);
            CreateProfileButton.Invoke();
            return this;
        }

        public ProfilesWindow DeleteProfileByByName(string profileName)
        {
            WaitUntilElementExistsByAutomationIdAndReturnTheElement($"Delete-{profileName}", TestConstants.VeryShortTimeout).AsButton().Invoke();
            ContinueButton.Invoke();
            return this;
        }

        public ProfilesWindow EditProfileName(string oldProfileName, string newProfileName)
        {
            WaitUntilElementExistsByAutomationIdAndReturnTheElement($"Edit-{oldProfileName}", TestConstants.VeryShortTimeout).AsButton().AsButton().Invoke();
            ProfileNameInput.Enter(newProfileName);
            SaveButton.Click();
            return this;
        }

        public ProfilesWindow DiscardProfile()
        {
            Window.FindFirstDescendant(cf => cf.ByName("Cancel").And(cf.ByClassName("Button"))).Click();
            DiscardlButton.Invoke();
            return this;
        }
    }
}
