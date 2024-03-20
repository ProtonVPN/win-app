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

namespace ProtonVPN.UI.Tests.Robots.Settings;

public partial class SettingsRobot : UIActions
{
    protected Button GoBackButton => ElementByAutomationId("GoBackButton").AsButton();
    protected Button NetShieldSettingsCard => ElementByAutomationId("NetShieldSettingsCard").AsButton();
    protected Button KillSwitchSettingsCard => ElementByAutomationId("KillSwitchSettingsCard").AsButton();
    protected Button PortForwardingSettingsCard => ElementByAutomationId("PortForwardingSettingsCard").AsButton();
    protected Button SplitTunnelingSettingsCard => ElementByAutomationId("SplitTunnelingSettingsCard").AsButton();
    protected Button ProtocolSettingsCard => ElementByAutomationId("ProtocolSettingsCard").AsButton();
    protected Button VpnAcceleratorSettingsCard => ElementByAutomationId("VpnAcceleratorSettingsCard").AsButton();
    protected Button AdvancedSettingsCard => ElementByAutomationId("AdvancedSettingsCard").AsButton();
    protected Button AutoStartupSettingsCard => ElementByAutomationId("AutoStartupSettingsCard").AsButton();
    protected Button NotificationsSettingsCard => ElementByAutomationId("NotificationsSettingsCard").AsButton();
    protected Button LanguagesSettingsCard => ElementByAutomationId("LanguagesSettingsCard").AsButton();
    protected Button ThemesSettingsCard => ElementByAutomationId("ThemesSettingsCard").AsButton();
    protected Button BetaAccessSettingsCard => ElementByAutomationId("BetaAccessSettingsCard").AsButton();
    protected Button SupportCenterSettingsCard => ElementByAutomationId("SupportCenterSettingsCard").AsButton();
    protected Button ReportIssueSettingsCard => ElementByAutomationId("ReportIssueSettingsCard").AsButton();
    protected Button DebugLogsSettingsCard => ElementByAutomationId("DebugLogsSettingsCard").AsButton();
    protected Button CensorshipSettingsCard => ElementByAutomationId("CensorshipSettingsCard").AsButton();
    protected Button AlternativeRoutingSettingsCard => ElementByAutomationId("AlternativeRoutingSettingsCard").AsButton();
    protected Button CustomDnsServersSettingsCard => ElementByAutomationId("CustomDnsServersSettingsCard").AsButton();
    protected Button NatTypeSettingsCard => ElementByAutomationId("NatTypeSettingsCard").AsButton();
    protected ToggleButton CustomDnsToggle => ElementByAutomationId("CustomDnsToggle").AsToggleButton();
    protected Button RestoreDefaultSettingsButton => ElementByAutomationId("RestoreDefaultSettingsButton").AsButton();
    protected RadioButton SmartProtocolRadioButton => ElementByAutomationId("SmartProtocolRadioButton").AsRadioButton();
    protected RadioButton WireGuardProtocolRadioButton => ElementByAutomationId("WireGuardProtocolRadioButton").AsRadioButton();
    protected RadioButton OpenVpnUdpProtocolRadioButton => ElementByAutomationId("OpenVpnUdpProtocolRadioButton").AsRadioButton();
    protected RadioButton OpenVpnTcpProtocolRadioButton => ElementByAutomationId("OpenVpnTcpProtocolRadioButton").AsRadioButton();
    protected ToggleButton NetshieldToggle => ElementByAutomationId("NetshieldToggle").AsToggleButton();
    protected Button PrimaryButton => ElementByAutomationId("PrimaryButton").AsButton();
    protected TextBox CustomDnsIpAddressTextBox => ElementByAutomationId("CustomDnsIpAddressBox").AsTextBox();
    protected Button AddButton => ElementByAutomationId("AddButton").AsButton();
    protected Button ApplyButton => ElementByAutomationId("ApplyButton").AsButton();
    protected Button TrashIconButton => ElementByAutomationId("TrashIcon").AsButton();
    protected Button SplitTunnelingButton => ElementByXpath("//Button[@Name='Split tunneling']").AsButton();
    protected TextBox IpTextBlock => ElementByName("Add IPv4 address").AsTextBox();
    protected Button AddIpButton => ElementByAutomationId("AddIpAddressButton").AsButton();
    protected AutomationElement SplitTunnelingApp(string splitTunnelingAppName)
    {
        return ElementByName(splitTunnelingAppName);
    }

    protected CheckBox GetCustomDnsCheckBox(string dnsAddress)
    {
        return ElementByName(dnsAddress).AsCheckBox();
    }
}