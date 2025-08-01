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

using System;
using System.Linq;
using System.Threading;
using FlaUI.Core.Tools;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.UiTools;

namespace ProtonVPN.UI.Tests.Robots;
public class AdvancedSettingsRobot
{
    protected Element CustomDnsSettingCard = Element.ByAutomationId("CustomDnsServersSettingsCard");
    protected Element CustomDnsToggle = Element.ByAutomationId("CustomDnsToggle");
    protected Element EnableButton = Element.ByName("Enable");
    protected Element CustomDnsTextBox = Element.ByAutomationId("CustomDnsIpAddressBox");
    protected Element AddButton = Element.ByAutomationId("AddButton");
    protected Element TrashButton = Element.ByAutomationId("TrashIcon");
    protected Element NatTypeCard = Element.ByAutomationId("NatTypeSettingsCard");

    private string WireguardDnsAddress => NetworkUtils.GetDnsAddresses("ProtonVPN").FirstOrDefault();
    private string OpenVpnDnsAddress => NetworkUtils.GetDnsAddresses("ProtonVPN TUN").FirstOrDefault();

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

    public AdvancedSettingsRobot ToggleCustomDnsSetting()
    {
        CustomDnsToggle.Click();
        return this;
    }

    public AdvancedSettingsRobot PressEnable()
    {
        EnableButton.Click();
        return this;
    }

    public AdvancedSettingsRobot SetCustomDns(string ipAddress)
    {
        CustomDnsTextBox.SetText(ipAddress);
        AddButton.Click();
        return this;
    }

    public AdvancedSettingsRobot TickCustomDnsServer(string ipAddress)
    {
        Element.ByName(ipAddress).Click();
        return this;
    }

    public AdvancedSettingsRobot RemoveCustomDnsServer()
    {
        TrashButton.Click();
        return this;
    }

    public class Verifications : AdvancedSettingsRobot
    {
        public Verifications IsCustomDnsAddressSet(string dnsAddress)
        {
            RetryResult<bool> retry = Retry.WhileFalse(
                () => {
                    return DoesContainDnsAddress(dnsAddress);
                },
                TestConstants.FiveSecondsTimeout, TestConstants.RetryInterval);

            if (!retry.Success)
            {
                throw new Exception(DnsAdressErrorMessage(dnsAddress));
            }
            return this;
        }

        public Verifications IsCustomDnsAddressNotSet(string dnsAddress)
        {
            RetryResult<bool> retry = Retry.WhileTrue(
                () => {
                    return DoesContainDnsAddress(dnsAddress);
                },
                TestConstants.FiveSecondsTimeout, TestConstants.RetryInterval);

            if (!retry.Success)
            {
                throw new Exception(DnsAdressErrorMessage(dnsAddress));
            }
            return this;
        }

        private string DnsAdressErrorMessage(string expectedDnsAddress)
        {
            return $"Wireguard dns address: {WireguardDnsAddress}. OpenVPN dns address: {OpenVpnDnsAddress}. Expected dns value: {expectedDnsAddress}";
        }

        private bool DoesContainDnsAddress(string expectedDnsAddress)
        {
            return WireguardDnsAddress == expectedDnsAddress || OpenVpnDnsAddress == expectedDnsAddress;
        }
    }

    public Verifications Verify => new();
}
