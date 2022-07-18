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

using System.Net;
using System.Net.NetworkInformation;
using FlaUI.Core.AutomationElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProtonVPN.UI.Test.FlaUI.Windows
{
    public class SettingsWindow : FlaUIActions
    {
        private AutomationElement StartMinimizedComboBox => ElementByAutomationId("StartMinimizedCombobox").AsComboBox();
        private ListBoxItem OptionDisabled => StartMinimizedComboBox.FindFirstChild().AsListBoxItem();
        private AutomationElement SettingsCloseButton => ElementByAutomationId("ModalCloseButton");
        private CheckBox ConnectOnBootCheckBox => ElementByAutomationId("ConnectOnBootCheckbox").AsCheckBox();
        private AutomationElement ConnectionTab => ElementByName("Connection").FindFirstChild();
        private CheckBox CustomDnsCheckBox => ElementByAutomationId("CheckBoxCustomDnsServers").AsCheckBox();
        private Button ContinueButton => ElementByAutomationId("ContinueButton").AsButton();
        private TextBox CustomDnsIPTextBox => ElementByAutomationId("InputIpv4Address").AsTextBox();
        private AutomationElement AddCustomDnsButton => ElementByAutomationId("SettingsPlusButton");
        private AutomationElement RemoveCustomDnsButton => ElementByAutomationId("DeleteButton").FindFirstChild();
        private AutomationElement ReconnectionButton => ElementByName("Reconnect");
            
        public SettingsWindow DisableStartToTray()
        {
            OptionDisabled.Select();
            return this;
        }

        public SettingsWindow ClickOnConnectOnBoot()
        {
            ConnectOnBootCheckBox.Click();
            return this;
        }

        public HomeWindow CloseSettings()
        {
            SettingsCloseButton.Click();
            return new HomeWindow();
        }

        public SettingsWindow NavigateToConnectionTab()
        {
            ConnectionTab.Click();
            return this;
        }

        public SettingsWindow ClickOnCustomDnsCheckBox()
        {
            CustomDnsCheckBox.Click();
            return this;
        }

        public SettingsWindow PressContinueToDisableNetshield()
        {
            ContinueButton.Invoke();
            return this;
        }

        public SettingsWindow CheckIfDnsAddressMatches(string dnsAddress)
        {
            string dnsAddressFromPC = GetDnsAddressForAdapter();
            Assert.AreEqual(dnsAddress, dnsAddressFromPC, "Desired dns address " + dnsAddress + " does not match Windows dns address " + dnsAddressFromPC);
            return this;
        }

        public SettingsWindow CheckIfDnsAddressDoesNotMatch(string dnsAddress)
        {
            Assert.AreNotEqual(dnsAddress, GetDnsAddressForAdapter());
            return this;
        }

        public SettingsWindow EnterCustomDnsAddress(string ipv4Address)
        {
            CustomDnsIPTextBox.Enter(ipv4Address);
            AddCustomDnsButton.Click();
            return this;
        }

        public SettingsWindow RemoveCustomDns()
        {
            RemoveCustomDnsButton.Click();
            return this;
        }

        public HomeWindow PressReconnect()
        {
            ReconnectionButton.Click();
            return new HomeWindow();
        }

        private string GetDnsAddressForAdapter()
        {
            string dnsAddress = null;
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                IPAddressCollection dnsServers = adapterProperties.DnsAddresses;
                if (adapter.Description.Contains("WireGuard Tunnel"))
                {
                    foreach (IPAddress dns in dnsServers)
                    {
                        dnsAddress = dns.ToString();
                    }
                }
            }
            return dnsAddress;
        }
    }
}
