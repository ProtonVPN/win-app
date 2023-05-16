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

namespace ProtonVPN.UI.Tests.Robots
{
    public class HomeRobot : UIActions
    {
        private Button ConnectButton => ElementByName("Connect").AsButton();
        private Button DisconnectButton => ElementByName("Disconnect").AsButton();
        private Button Recents => ElementByName("Recents").AsButton();
        private Button CancelConnectionButton => ElementByName("Cancel").AsButton();
        private Button RecentsSecondaryButton => ElementByAutomationId("btnSecondary").AsButton();
        private Button RemoveButton => ElementByName("Remove").AsButton();

        public HomeRobot QuickConnect()
        {
            ConnectButton.Invoke();
            return this;
        }

        public HomeRobot CancelConnection()
        {
            CancelConnectionButton.Click();
            return this;
        }

        public HomeRobot Disconnect()
        {
            DisconnectButton.Click();
            return this;
        }

        public HomeRobot ExpandRecents()
        {
            Recents.Click();
            return this;
        }

        public HomeRobot DeleteRecent()
        {
            RecentsSecondaryButton.Click();
            RemoveButton.Invoke();
            return this;
        }

        public HomeRobot WaitUntilConnected()
        {
            WaitUntilElementExistsByName("Protected", TestConstants.ShortTimeout);
            return this;
        }
    }
}
