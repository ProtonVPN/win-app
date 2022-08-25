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

using System.Threading;
using FlaUI.Core.AutomationElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.UI.Test.TestsHelper;

namespace ProtonVPN.UI.Test.Results
{
    public class SettingsResult : UIActions
    {
        private CheckBox ModerateNatCheckBox => ElementByAutomationId("ModerateNatCheckbox").AsCheckBox();

        public SettingsResult CheckIfDnsAddressDoesNotMatch(string dnsAddress)
        {
            Assert.AreNotEqual(dnsAddress, DnsUtils.GetDnsAddress("WireGuard Tunnel"));
            return this;
        }

        public SettingsResult CheckIfSettingsAreDisplayed()
        {
            WaitUntilElementExistsByName("Start Minimized", TestConstants.VeryShortTimeout);
            WaitUntilElementExistsByName("Start on boot", TestConstants.VeryShortTimeout);
            WaitUntilElementExistsByName("Connect on app start", TestConstants.VeryShortTimeout);
            WaitUntilElementExistsByName("Show Notifications", TestConstants.VeryShortTimeout);
            WaitUntilElementExistsByName("Early Access", TestConstants.VeryShortTimeout);
            return this;
        }

        public SettingsResult CheckIfCustomDnsAddressWasNotAdded()
        {
            CheckIfDoesNotExistsByAutomationId("DeleteButton");
            return this;
        }

        public SettingsResult CheckIfDnsAddressMatches(string dnsAddress)
        {
            string adapterDnsAddress = DnsUtils.GetDnsAddress("WireGuard Tunnel");
            Assert.AreEqual(dnsAddress, adapterDnsAddress, $"Desired dns address {dnsAddress} does not match Windows adapter dns address {adapterDnsAddress}");
            return this;
        }

        public SettingsResult CheckIfModerateNatIsEnabled()
        {
            Assert.IsTrue(ModerateNatCheckBox.IsChecked.Value, "Moderate NAT checkbox status is: " + ModerateNatCheckBox.IsChecked.Value);
            return this;
        }

        public SettingsResult CheckIfModerateNatIsDisabled()
        {
            Assert.IsFalse(ModerateNatCheckBox.IsChecked.Value, "Moderate NAT checkbox status is: " + ModerateNatCheckBox.IsChecked.Value);
            return this;
        }
    }
}
