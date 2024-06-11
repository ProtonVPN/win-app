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
using static ProtonVPN.UI.Tests.TestsHelper.TestConstants;

namespace ProtonVPN.UI.Tests.Robots.Settings;

public partial class SettingsRobot
{
    public SettingsRobot DoNavigateToProtocolSettingsPage()
    {
        return NavigateToSpecificSettingsPage(ProtocolSettingsCard);
    }

    public SettingsRobot DoNavigateToVpnAcceleratorSettingsPage()
    {
        return NavigateToSpecificSettingsPage(VpnAcceleratorSettingsCard);
    }

    public SettingsRobot DoNavigateToAdvancedSettingsPage()
    {
        return NavigateToSpecificSettingsPage(AdvancedSettingsCard);
    }

    public SettingsRobot DoNavigateToAutoStartupSettingsPage()
    {
        return NavigateToSpecificSettingsPage(AutoStartupSettingsCard);
    }

    public SettingsRobot DoNavigateToDebugLogsSettingsPage()
    {
        return NavigateToSpecificSettingsPage(DebugLogsSettingsCard);
    }

    public SettingsRobot DoNavigateToCensorshipSettingsPage()
    {
        return NavigateToSpecificSettingsPage(CensorshipSettingsCard);
    }

    public SettingsRobot DoNavigateToCustomDnsServersSettingsPage()
    {
        return DoNavigateToAdvancedSettingsPage()
            .NavigateToSpecificSettingsPage(CustomDnsServersSettingsCard);
    }

    public SettingsRobot DoReportAnIssue()
    {
        ReportIssueSettingsCard.FocusAndClick();
        return this;
    }

    public SettingsRobot DoSelectNetshield()
    {
        NetshieldToggle.Click();
        return this;
    }

    public SettingsRobot DoClickCustomDnsToggle()
    {
        CustomDnsToggle.Click();
        return this;
    }

    public SettingsRobot DoEnableOnWarning()
    {
        PrimaryButton.Click();
        return this;
    }

    public SettingsRobot DoApplyChanges()
    {
        ApplyButton.Click();
        return this;
    }

    public SettingsRobot DoSetCustomDnsIpAddress(string ipAddress)
    {
        CustomDnsIpAddressTextBox.Text = ipAddress;
        AddButton.Click();
        return this;
    }

    public SettingsRobot DoClickCustomDnsAddress(string dnsAddress)
    {
        GetCustomDnsCheckBox(dnsAddress).Click();
        return this;
    }

    public SettingsRobot DoClickTrashIcon()
    {
        TrashIconButton.Click();
        return this;
    }

    public SettingsRobot EnableSplitTunneling()
    {
        SplitTunnelingButton.Click();
        return this;
    }

    public SettingsRobot ExcludeApp(string name)
    {
        SplitTunnelingApp(name).Click();
        return this;
    }

    public SettingsRobot ExcludeIp(string ip)
    {
        IpTextBlock.Text = ip;
        AddIpButton.Invoke();
        return this;
    }

    public SettingsRobot DoSelectProtocol(Protocol protocol)
    {
        switch(protocol)
        {
            case Protocol.WireGuardUdp:
                WireGuardUdpProtocolRadioButton.Click();
                break;
            case Protocol.WireGuardTcp:
                WireGuardTcpProtocolRadioButton.Click();
                break;
            case Protocol.WireGuardTls:
                WireGuardTlsProtocolRadioButton.Click();
                break;
            case Protocol.OpenVpnUdp:
                OpenVpnUdpProtocolRadioButton.Click();
                break;
            case Protocol.OpenVpnTcp:
                OpenVpnTcpProtocolRadioButton.Click();
                break;
        }
        return this;
    }

    private SettingsRobot NavigateToSpecificSettingsPage(Button settingsCard)
    {
        settingsCard.FocusAndClick();

        // Navigation triggers an animation that may occasionnally fails the tests. Wait for animation to complete before moving on.
        this.Wait(TestConstants.DefaultAnimationDelay);

        return this;
    }
}