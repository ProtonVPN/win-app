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
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.UiTools;

namespace ProtonVPN.UI.Tests.Robots;

public class ProfileRobot
{
    protected Element ProfileNameTextBox = Element.ByAutomationId("ProfileNameTextBox");

    protected Element ApplyButton = Element.ByAutomationId("ApplyButton");

    protected Element CloseButton = Element.ByAutomationId("CloseSettingsButton");

    protected Element ToggleSettingsButton = Element.ByAutomationId("ToggleExpanderButton");

    protected Element NetShieldDropDown = Element.ByAutomationId("NetShieldDropDown");

    protected Element NetShieldOffMenuItem = Element.ByAutomationId("NetShieldOffMenuItem");

    protected Element NetShieldLevelOneMenuItem = Element.ByAutomationId("NetShieldLevelOneMenuItem");

    protected Element NetShieldLevelTwoMenuItem = Element.ByAutomationId("NetShieldLevelTwoMenuItem");

    protected Element PortForwardingDropDown = Element.ByAutomationId("PortForwardingDropDown");

    protected Element PortForwardingOffMenuItem = Element.ByAutomationId("PortForwardingOffMenuItem");

    protected Element PortForwardingOnMenuItem = Element.ByAutomationId("PortForwardingOnMenuItem");

    protected Element ProtocolsDropDown = Element.ByAutomationId("ProtocolsDropDown");

    protected Element SmartProtocolMenuItem = Element.ByAutomationId("SmartProtocolMenuItem");

    protected Element WireGuardUdpProtocolMenuItem = Element.ByAutomationId("WireGuardUdpProtocolMenuItem");

    protected Element WireGuardTcpProtocolMenuItem = Element.ByAutomationId("WireGuardTcpProtocolMenuItem");

    protected Element WireGuardTlsProtocolMenuItem = Element.ByAutomationId("WireGuardTlsProtocolMenuItem");

    protected Element OpenVpnUdpProtocolMenuItem = Element.ByAutomationId("OpenVpnUdpProtocolMenuItem");

    protected Element OpenVpnTcpProtocolMenuItem = Element.ByAutomationId("OpenVpnTcpProtocolMenuItem");

    protected Element NatTypeDropDown = Element.ByAutomationId("NatTypeDropDown");

    protected Element StrictNatMenuItem = Element.ByAutomationId("StrictNatMenuItem");

    protected Element ModerateNatMenuItem = Element.ByAutomationId("ModerateNatMenuItem");

    public ProfileRobot SetProfileName(string profileName)
    {
        ProfileNameTextBox.SetText(profileName);
        return this;
    }

    public ProfileRobot CloseProfile()
    {
        CloseButton.Click();
        return this;
    }

    public ProfileRobot SaveProfile()
    {
        ApplyButton.Click();
        return this;
    }

    public ProfileRobot ExpandSettingsSection()
    {
        ToggleSettingsButton.Click();
        Thread.Sleep(TestConstants.AnimationDelay);
        return this;
    }

    public ProfileRobot DisableNetShield()
    {
        NetShieldDropDown.Click();
        Thread.Sleep(TestConstants.AnimationDelay);
        NetShieldOffMenuItem.Click();
        return this;
    }

    public ProfileRobot SelectNetShieldMode(NetShieldMode netShieldMode)
    {
        NetShieldDropDown.Click();

        Thread.Sleep(TestConstants.AnimationDelay);

        if (netShieldMode == NetShieldMode.BlockMalwareOnly)
        {
            NetShieldLevelOneMenuItem.Click();
        }
        else if (netShieldMode == NetShieldMode.BlockAdsMalwareTrackers)
        {
            NetShieldLevelTwoMenuItem.Click();
        }

        return this;
    }

    public class Verifications : ProfileRobot
    {
        public Verifications DoesProfileNameEqual(string profileName)
        {
            ProfileNameTextBox.TextBoxEquals(profileName);
            return this;
        }
    }

    public Verifications Verify => new Verifications();
}