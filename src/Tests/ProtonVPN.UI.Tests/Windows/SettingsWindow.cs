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

namespace ProtonVPN.UI.Tests.Windows
{
    public class SettingsWindow : UIActions
    {
        private AutomationElement StartMinimizedComboBox => ElementByAutomationId("StartMinimizedCombobox").AsComboBox();
        private ListBoxItem OpenVpnUdpOption => StartMinimizedComboBox.FindChildAt(2).AsListBoxItem();
        private ListBoxItem OpenVpnTcpOption => StartMinimizedComboBox.FindChildAt(3).AsListBoxItem();
        private ListBoxItem OptionDisabled => StartMinimizedComboBox.FindFirstChild().AsListBoxItem();
        private AutomationElement SettingsCloseButton => ElementByAutomationId("ModalCloseButton");
        private CheckBox ConnectOnBootCheckBox => ElementByAutomationId("ConnectOnBootCheckbox").AsCheckBox();
        private AutomationElement ConnectionTab => ElementByName("Connection").FindFirstChild();
        private AutomationElement AdvancedTab => ElementByName("Advanced").FindFirstChild();
        private CheckBox CustomDnsCheckBox => ElementByAutomationId("CheckBoxCustomDnsServers").AsCheckBox();
        private Button ContinueButton => ElementByAutomationId("ContinueButton").AsButton();
        private TextBox CustomDnsIPTextBox => ElementByAutomationId("InputIpv4Address").AsTextBox();
        private AutomationElement AddCustomDnsButton => ElementByAutomationId("SettingsPlusButton");
        private AutomationElement RemoveCustomDnsButton => ElementByAutomationId("DeleteButton").FindFirstChild();
        private AutomationElement ReconnectionButton => ElementByName("Reconnect");
        private CheckBox PortForawrdingShortcutCheckBox => ElementByAutomationId("PortForwardingInQuickSettingsCheckbox").AsCheckBox();
        private CheckBox NotificationsCheckBox => ElementByAutomationId("ShowNotificationsCheckbox").AsCheckBox();
        private CheckBox PortForwardingCheckBox => ElementByAutomationId("PortForwardingCheckbox").AsCheckBox();
        private Button EnableButton => ElementByName("Enable").AsButton();

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

        public SettingsWindow NavigateToAdvancedTab()
        {
            AdvancedTab.Click();
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

        public SettingsWindow ClickOnPortForwardingShortcutCheckBox()
        {
            PortForawrdingShortcutCheckBox.Toggle();
            return this;
        }

        public SettingsWindow ClickOnNotificationsCheckBox()
        {
            NotificationsCheckBox.Click();
            return this;
        }

        public SettingsWindow TogglePortForwarding()
        {
            PortForwardingCheckBox.Click();
            EnableButton.Invoke();
            return this;
        }

        public SettingsWindow SelectProtocolOpenVpnUdp()
        {
            OpenVpnUdpOption.Select();
            return this;
        }

        public SettingsWindow SelectProtocolOpenVpnTcp()
        {
            OpenVpnTcpOption.Select();
            return this;
        }
    }
}
