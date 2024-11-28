/*
 * Copyright (c) 2024 Proton AG
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
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.UiTools;

namespace ProtonVPN.UI.Tests.Robots;

public class SettingRobot
{
    private const string NETSHIELD_NO_BLOCK = "netshield-0.protonvpn.net";
    private const string NETSHIELD_MALWARE_ENDPOINT = "netshield-1.protonvpn.net";
    private const string NETSHIELD_ADS_ENDPOINT = "netshield-2.protonvpn.net";

    protected Element ApplyButton = Element.ByAutomationId("ApplyButton");
    protected Element CloseSettingsButton = Element.ByAutomationId("CloseSettingsButton");
    protected Element ReconnectButton = Element.ByName("Reconnect");
    protected Element SettingsButton = Element.ByAutomationId("SettingsButton");
    protected Element NetShieldSettingsCard = Element.ByAutomationId("NetShieldSettingsCard");
    protected Element ProtocolSettingsCard = Element.ByAutomationId("ProtocolSettingsCard");
    protected Element AdvancedSettingsCard = Element.ByAutomationId("AdvancedSettingsCard");
    protected Element GoBackButton = Element.ByAutomationId("GoBackButton");

    protected Element NetshieldToggle = Element.ByAutomationId("NetshieldToggle");
    protected Element NetShieldLevelOneRadioButton = Element.ByAutomationId("NetShieldLevelOne");
    protected Element NetShieldLevelTwoRadioButton = Element.ByAutomationId("NetShieldLevelTwo");

    protected Element OpenVpnTcpProtocolRadioButton = Element.ByAutomationId("OpenVpnTcpProtocolRadioButton");
    protected Element OpenVpnUdpProtocolRadioButton = Element.ByAutomationId("OpenVpnUdpProtocolRadioButton");
    protected Element WireGuardUdpProtocolRadioButton = Element.ByAutomationId("WireGuardUdpProtocolRadioButton");
    protected Element WireGuardTlsProtocolRadioButton = Element.ByAutomationId("WireGuardTlsProtocolRadioButton");
    protected Element WireGuardTcpProtocolRadioButton = Element.ByAutomationId("WireGuardTcpProtocolRadioButton");
    
    public SettingRobot OpenSettings()
    {
        SettingsButton.Click();
        return this;
    }

    public SettingRobot CloseSettings()
    {
        CloseSettingsButton.Click();
        return this;
    }

    public SettingRobot OpenNetShieldSettings()
    {
        NetShieldSettingsCard.Click();
        // Remove when VPNWIN-2261 is implemented.
        Thread.Sleep(TestConstants.AnimationDelay);
        return this;
    }

    public SettingRobot OpenAdvancedSettings()
    {
        AdvancedSettingsCard.Click();
        return this;
    }

    public SettingRobot OpenProtocolSettings()
    {
        ProtocolSettingsCard.Click();
        // Remove when VPNWIN-2261 is implemented.
        Thread.Sleep(TestConstants.AnimationDelay);
        return this;
    }

    public SettingRobot SelectProtocol(TestConstants.Protocol protocol)
    {
        switch (protocol)
        {
            case TestConstants.Protocol.OpenVpnUdp:
                OpenVpnUdpProtocolRadioButton.Click();
                break;

            case TestConstants.Protocol.OpenVpnTcp:
                OpenVpnTcpProtocolRadioButton.Click();
                break;

            case TestConstants.Protocol.WireGuardTcp:
                WireGuardTcpProtocolRadioButton.Click();
                break;

            case TestConstants.Protocol.WireGuardTls:
                WireGuardTlsProtocolRadioButton.Click();
                break;

            case TestConstants.Protocol.WireGuardUdp:
                WireGuardUdpProtocolRadioButton.Click();
                break;
        }
        return this;
    }

    public SettingRobot ToggleNetShieldSetting()
    {
        NetshieldToggle.Click();
        return this;
    }

    public SettingRobot SelectNetShieldMode(NetShieldMode netShieldMode)
    {
        if (netShieldMode == NetShieldMode.BlockMalwareOnly)
        {
            NetShieldLevelOneRadioButton.Click();
        }
        else if (netShieldMode == NetShieldMode.BlockAdsMalwareTrackers)
        {
            NetShieldLevelTwoRadioButton.Click();
        }

        return this;
    }

    public SettingRobot ApplySettings()
    {
        ApplyButton.Invoke();
        return this;
    }

    public SettingRobot Reconnect()
    {
        ReconnectButton.Click();
        return this;
    }

    public SettingRobot GoBack()
    {
        GoBackButton.Click();
        return this;
    }

    public class Verifications : SettingRobot
    {
        public Verifications NetshieldIsBlocking(NetShieldMode netShieldMode)
        {
            NetworkUtils.FlushDns();
            CommonAssertions.AssertDnsIsResolved(NETSHIELD_NO_BLOCK);

            if (netShieldMode is NetShieldMode.BlockMalwareOnly or NetShieldMode.BlockAdsMalwareTrackers)
            {
                CommonAssertions.AssertDnsIsNotResolved(NETSHIELD_MALWARE_ENDPOINT);
            }

            if (netShieldMode is NetShieldMode.BlockAdsMalwareTrackers)
            {
                CommonAssertions.AssertDnsIsNotResolved(NETSHIELD_ADS_ENDPOINT);
            }

            return this;
        }

        public Verifications NetshieldShowsDisableState()
        {
            NetShieldSettingsCard.FindChild(Element.ByName("Off")).WaitUntilDisplayed();
            return this;
        }

        public Verifications NetshieldShowsEnabledState()
        {
            NetShieldSettingsCard.FindChild(Element.ByName("On")).WaitUntilDisplayed();
            return this;
        }

        public Verifications NetshieldIsNotBlocking()
        {
            NetworkUtils.FlushDns();
            CommonAssertions.AssertDnsIsResolved(NETSHIELD_NO_BLOCK);
            CommonAssertions.AssertDnsIsResolved(NETSHIELD_MALWARE_ENDPOINT);
            CommonAssertions.AssertDnsIsResolved(NETSHIELD_ADS_ENDPOINT);
            return this;
        }
    }

    public Verifications Verify => new();
}