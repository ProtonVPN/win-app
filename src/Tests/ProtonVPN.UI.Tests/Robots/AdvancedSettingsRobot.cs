/*
 * Copyright (c) 2026 Proton AG
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

using System.Collections.Generic;
using NUnit.Framework;
using ProtonVPN.UI.Tests.UiTools;

namespace ProtonVPN.UI.Tests.Robots;

public class AdvancedSettingsRobot
{
    protected Element CustomDnsSettingCard = Element.ByAutomationId("CustomDnsServersSettingsCard");
    protected Element CustomDnsToggle = Element.ByAutomationId("CustomDnsToggle");
    protected Element EnableButton = Element.ByName("Enable");
    protected Element DnsServersSelectorSettingsCard = Element.ByAutomationId("DnsServersSelectorSettingsCard");
    protected Element NatTypeCard = Element.ByAutomationId("NatTypeSettingsCard");
    protected Element LanConnectionsSettingsCard = Element.ByAutomationId("LanConnectionsSettingsCard");
    protected Element AllowLanToggle = Element.ByAutomationId("AllowLanConnectionsToggleSwitch");

    public AdvancedSettingsRobot NavigateToLan()
    {
        LanConnectionsSettingsCard.Click();
        return this;
    }

    public AdvancedSettingsRobot NavigateToCustomDns()
    {
        CustomDnsSettingCard.Click();
        return this;
    }

    public AdvancedSettingsRobot NavigateToNatSettings()
    {
        NatTypeCard.Click();
        return this;
    }

    public AdvancedSettingsRobot EnableLanToggle()
    {
        if (!AllowLanToggle.IsToggled())
        {
            AllowLanToggle.Toggle();
        }
        return this;
    }

    public AdvancedSettingsRobot DisableLanToggle()
    {
        if (AllowLanToggle.IsToggled())
        {
            AllowLanToggle.Toggle();
        }
        return this;
    }

    public AdvancedSettingsRobot EnableCustomDnsToggle()
    {
        if (!CustomDnsToggle.IsToggled())
        {
            CustomDnsToggle.Toggle();
        }
        return this;
    }

    public AdvancedSettingsRobot DisableCustomDnsToggle()
    {
        if (CustomDnsToggle.IsToggled())
        {
            CustomDnsToggle.Toggle();
        }
        return this;
    }

    public AdvancedSettingsRobot PressEnable()
    {
        EnableButton.Click();
        return this;
    }

    public AdvancedSettingsRobot EditCustomDnsServers()
    {
        DnsServersSelectorSettingsCard.Click();
        return this;
    }

    public class Verifications : AdvancedSettingsRobot
    {
        public Verifications IsLanEnabled()
        {
            Assert.That(AllowLanToggle.IsToggled(), Is.True);
            return this;
        }

        public Verifications IsCustomDnsEnabled()
        {
            Assert.That(CustomDnsToggle.IsToggled(), Is.True);
            return this;
        }

        public Verifications IsCustomDnsDisabled()
        {
            Assert.That(CustomDnsToggle.IsToggled(), Is.False);
            return this;
        }

        public Verifications CustomDnsContainsIpAddress(string ip)
        {
            List<string> allChildren = DnsServersSelectorSettingsCard.GetAllChildrenNames();
            Assert.That(allChildren, Does.Contain(ip));
            return this;
        }

        public Verifications CustomDnsDoesNotContainIpAddress(string ip)
        {
            List<string> allChildren = DnsServersSelectorSettingsCard.GetAllChildrenNames();
            Assert.That(allChildren, Does.Not.Contain(ip));
            return this;
        }
    }

    public Verifications Verify => new();
}