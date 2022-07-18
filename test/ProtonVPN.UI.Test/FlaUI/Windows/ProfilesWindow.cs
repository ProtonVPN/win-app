/*
 * Copyright (c) 2022 Proton
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
using ProtonVPN.UI.Test.FlaUI.Utils;

namespace ProtonVPN.UI.Test.FlaUI.Windows
{
    public class ProfilesWindow : FlaUIActions
    {
        private Button CreateProfileButton => ElementByAutomationId("NewProfileButton").AsButton();
        private TextBox ProfileNameInput => ElementByAutomationId("textSource").AsTextBox();
        private ListBoxItem CountryFirstOption => ElementByAutomationId("CountryBox").AsComboBox().FindFirstChild().AsListBoxItem();
        private ListBoxItem ServerFirstOption => ElementByAutomationId("ServerBox").AsComboBox().FindChildAt(3).AsListBoxItem();
        private Button SaveButton => ElementByName("Save").AsButton();
        private Button ProfileConnectButton(string profileName) => ElementByAutomationId(profileName).AsButton();

        public ProfilesWindow CreateStandartProfile(string profileName)
        {
            WaitUntilElementExistsByAutomationId("NewProfileButton", TestConstants.ShortTimeout);
            CreateProfileButton.Invoke();
            ProfileNameInput.Enter(profileName);
            CountryFirstOption.Select();
            ServerFirstOption.Select();
            SaveButton.Invoke();
            return this;
        }

        public ProfilesWindow ConnectToProfile(string profileName)
        {
            WaitUntilElementExistsByAutomationId($"Connect-{profileName}", TestConstants.ShortTimeout);
            ProfileConnectButton($"Connect-{profileName}").Invoke();
            return this;
        }
    }
}
