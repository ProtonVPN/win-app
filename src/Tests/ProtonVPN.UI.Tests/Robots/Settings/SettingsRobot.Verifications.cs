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

using System.Net.Sockets;
using System.Net;
using FlaUI.Core.AutomationElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Robots.Settings;

public partial class SettingsRobot
{
    private const string NETSHIELD_NO_BLOCK = "netshield-0.protonvpn.net";
    private const string NETSHIELD_MALWARE_ENDPOINT = "netshield-1.protonvpn.net";
    private const string NETSHIELD_ADS_ENDPOINT = "netshield-2.protonvpn.net";
    private string WireguardDnsAddress => NetworkUtils.GetDnsAddress("ProtonVPN");
    private string OpenVpnDnsAddress => NetworkUtils.GetDnsAddress("ProtonVPN TUN");

    public SettingsRobot VerifyProtocolSettingsCard(string expectedProtocol)
    {
        Button settingsCard = ProtocolSettingsCard;

        Assert.IsNotNull(settingsCard);

        Assert.IsNotNull(settingsCard.FindFirstDescendant(cf => cf.ByText(expectedProtocol)));

        return this;
    }

    public SettingsRobot VerifyProtocolSettingsPage()
    {
        Assert.IsNotNull(GoBackButton);
        return this;
    }

    public SettingsRobot VerifySmartProtocolIsChecked()
    {
        RadioButton smartProtocolRadioButton = SmartProtocolRadioButton;
        RadioButton wireGuardProtocolRadioButton = WireGuardProtocolRadioButton;

        Assert.IsNotNull(smartProtocolRadioButton);
        Assert.IsTrue(smartProtocolRadioButton.IsChecked);

        Assert.IsNotNull(wireGuardProtocolRadioButton);
        Assert.IsFalse(wireGuardProtocolRadioButton.IsChecked);

        return this;
    }

    public SettingsRobot VerifyWireGuardProtocolIsChecked()
    {
        RadioButton smartProtocolRadioButton = SmartProtocolRadioButton;
        RadioButton wireGuardProtocolRadioButton = WireGuardProtocolRadioButton;

        Assert.IsNotNull(smartProtocolRadioButton);
        Assert.IsFalse(smartProtocolRadioButton.IsChecked);

        Assert.IsNotNull(wireGuardProtocolRadioButton);
        Assert.IsTrue(wireGuardProtocolRadioButton.IsChecked);

        return this;
    }

    public SettingsRobot VerifyCustomDnsIsEnabled()
    {
        Assert.IsTrue(CustomDnsToggle.IsToggled);
        return this;
    }

    public SettingsRobot VerifyNetshieldIsDisabled()
    {
        Assert.IsFalse(NetshieldToggle.IsToggled);
        return this;
    }

    public SettingsRobot VerifyCustomDnsIsSet(string dnsAddress)
    {
        Assert.IsTrue(DoesContainDnsAddress(dnsAddress), DnsAdressErrorMessage(dnsAddress));
        return this;
    }

    public SettingsRobot VerifyCustomDnsIsNotSet(string dnsAdress)
    {
        Assert.IsFalse(DoesContainDnsAddress(dnsAdress), DnsAdressErrorMessage(dnsAdress));
        return this;
    }

    public SettingsRobot VerifyNetshieldIsNotBlocking()
    {
        VerifyIfDnsIsResolved(NETSHIELD_NO_BLOCK);
        VerifyIfDnsIsResolved(NETSHIELD_MALWARE_ENDPOINT);
        VerifyIfDnsIsResolved(NETSHIELD_ADS_ENDPOINT);
        return this;
    }

    public SettingsRobot VerifyNetshieldIsBlocking()
    {
        VerifyIfDnsIsResolved(NETSHIELD_NO_BLOCK);
        VerifyIfDnsIsNotResolved(NETSHIELD_MALWARE_ENDPOINT);
        VerifyIfDnsIsNotResolved(NETSHIELD_ADS_ENDPOINT);
        return this;
    }

    private void VerifyIfDnsIsResolved(string url)
    {
        Assert.IsTrue(TryToResolveDns(url), $"Dns was not resolved for {url}.");
    }

    private void VerifyIfDnsIsNotResolved(string url)
    {
        Assert.IsFalse(TryToResolveDns(url), $"DNS was resolved for {url}");
    }

    private string DnsAdressErrorMessage(string expectedDnsAddress)
    {
        return $"Wireguard dns address: {WireguardDnsAddress}. OpenVPN dns address: {OpenVpnDnsAddress}. Expected dns value: {expectedDnsAddress}";
    }

    private bool DoesContainDnsAddress(string expectedDnsAddress)
    {
        return WireguardDnsAddress == expectedDnsAddress || OpenVpnDnsAddress == expectedDnsAddress;
    }

    private static bool TryToResolveDns(string url)
    {
        bool isConnected = true;
        try
        {
            Dns.GetHostEntry(url);
        }
        catch (SocketException)
        {
            isConnected = false;
        }
        return isConnected;
    }
}