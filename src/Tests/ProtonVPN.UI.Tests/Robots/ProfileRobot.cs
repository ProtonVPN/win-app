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

using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using NUnit.Framework;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.Core.Definitions;
using FlaUI.Core.AutomationElements;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.UiTools;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.Enums.Locations;

namespace ProtonVPN.UI.Tests.Robots;

public class ProfileRobot
{
    private static readonly string _countryDropdownTranslated = LanguageHelper.GetTranslatedString("Connections_Country");
    private static readonly string _middleCountryDropdownTranslated = LanguageHelper.GetTranslatedString("Connections_Country_Middle");
    private static readonly string _cityDropdownTranslated = LanguageHelper.GetTranslatedString("Connections_City");

    protected Element CountryDropdown = Element.ByName(_countryDropdownTranslated).And(Element.ByClassName("ComboBox"));
    protected Element MiddleCountryDropdown = Element.ByName(_middleCountryDropdownTranslated).And(Element.ByClassName("ComboBox"));
    protected Element CityDropdown = Element.ByName(_cityDropdownTranslated).And(Element.ByClassName("ComboBox"));

    protected Element ProfileNameTextBox = Element.ByAutomationId("ProfileNameTextBox");

    protected Element ApplyButton = Element.ByAutomationId("ApplyButton");

    protected Element CloseButton = Element.ByAutomationId("CloseSettingsButton");

    protected Element ToggleSettingsButton = Element.ByAutomationId("ToggleExpanderButton");

    protected Element NetShieldDropDown = Element.ByAutomationId("NetShieldDropDown");

    protected Element NetShieldOffMenuItem = Element.ByAutomationId("NetShieldOffMenuItem");

    protected Element NetShieldLevelOneMenuItem = Element.ByAutomationId("NetShieldLevelOneMenuItem");

    protected Element NetShieldLevelTwoMenuItem = Element.ByAutomationId("NetShieldLevelTwoMenuItem");

    protected Element NetShieldLevelThreeMenuItem = Element.ByAutomationId("NetShieldLevelThreeMenuItem");

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

    protected Element ConnectionTypes = Element.ByClassName("ListBoxItem");

    protected Element ConnectAndGoDropDown = Element.ByAutomationId("ConnectAndGoDropDown");

    protected Element ConnectAndGoOffMenuItem = Element.ByAutomationId("ConnectAndGoOffMenuItem");

    protected Element ConnectAndGoWebsiteMenuItem = Element.ByAutomationId("ConnectAndGoWebsiteMenuItem");

    protected Element ConnectAndGoApplicationMenuItem = Element.ByAutomationId("ConnectAndGoApplicationMenuItem");

    protected Element ConnectAndGoParent = Element.ByAutomationId("ConnectAndGoParentSection");

    protected Element ConnectAndGoAppSelector = Element.ByAutomationId("ConnectAndGoAppSelector");

    protected Element ConnectAndGoPrivateModeCheckbox = Element.ByAutomationId("ConnectAndGoPrivateModeCheckbox");

    public ProfileRobot SetProfileName(string profileName)
    {
        ProfileNameTextBox.SetText(profileName);
        return this;
    }

    public ProfileRobot CloseProfile()
    {
        CloseButton.Invoke();
        return this;
    }

    public ProfileRobot SaveProfile()
    {
        ApplyButton.Invoke();
        return this;
    }

    public ProfileRobot SelectConnectionType(ConnectionType connectionType)
    {
        string? connectionTypeName = null;

        switch (connectionType)
        {
            case ConnectionType.Standard:
                connectionTypeName = LanguageHelper.GetTranslatedString("Server_Feature_None");
                break;
            case ConnectionType.SecureCore:
                connectionTypeName = LanguageHelper.GetTranslatedString("Server_Feature_SecureCore");
                break;
            case ConnectionType.P2P:
                connectionTypeName = LanguageHelper.GetTranslatedString("Server_Feature_P2P");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, "Unhandled connection type");
        }

        AutomationElement connectionTypeElement = ConnectionTypes.FindAllElements()
            .First(item => item.FindFirstDescendant(cf => cf.ByControlType(ControlType.Text)
            .And(cf.ByName(connectionTypeName))) != null);

        connectionTypeElement.Click();

        return this;
    }

    public ProfileRobot SelectCountry(Country countryName)
    {
        CountryDropdown
            .Click()
            .SelectDropdownItem(countryName.GetName());
        return this;
    }

    public ProfileRobot SelectMiddleCountry(Country middleCountryName)
    {
        MiddleCountryDropdown
            .Click()
            .SelectDropdownItem(TestConstants.ViaPrefix + middleCountryName.GetName());
        return this;
    }

    public ProfileRobot SelectCity(City cityName)
    {
        CityDropdown
            .Click()
            .SelectDropdownItem(cityName.GetEnumValue());
        return this;
    }

    public ProfileRobot ExpandSettingsSection()
    {
        ToggleSettingsButton.Click();
        // Remove when VPNWIN-2599 is implemented.
        Thread.Sleep(TestConstants.AnimationDelay);
        return this;
    }

    public ProfileRobot DisableNetShield()
    {
        NetShieldDropDown.Click();
        // Remove when VPNWIN-2599 is implemented.
        Thread.Sleep(TestConstants.AnimationDelay);
        NetShieldOffMenuItem.DoubleClick();
        // Remove when VPNWIN-2599 is implemented.
        Thread.Sleep(TestConstants.AnimationDelay);
        return this;
    }

    public ProfileRobot SelectProtocol(Protocol protocol)
    {
        ProtocolsDropDown.Click();
        switch (protocol)
        {
            case Protocol.OpenVpnUdp:
                OpenVpnUdpProtocolMenuItem.Invoke();
                break;
            case Protocol.OpenVpnTcp:
                OpenVpnTcpProtocolMenuItem.Invoke();
                break;
            case Protocol.WireGuardTcp:
                WireGuardTcpProtocolMenuItem.Invoke();
                break;
            case Protocol.WireGuardTls:
                WireGuardTlsProtocolMenuItem.Invoke();
                break;
            case Protocol.WireGuardUdp:
                WireGuardUdpProtocolMenuItem.Invoke();
                break;
        }
        return this;
    }

    public ProfileRobot SelectNetShieldMode(NetShieldMode netShieldMode)
    {
        NetShieldDropDown.Click();

        Thread.Sleep(TestConstants.AnimationDelay);

        switch (netShieldMode)
        {
            case NetShieldMode.BlockMalwareOnly:
                NetShieldLevelOneMenuItem.DoubleClick();
                break;
            case NetShieldMode.BlockAdsMalwareTrackers:
                NetShieldLevelTwoMenuItem.DoubleClick();
                break;
            case NetShieldMode.BlockAdsMalwareTrackersAdultContent:
                NetShieldLevelThreeMenuItem.DoubleClick();
                break;
        }

        return this;
    }

    public ProfileRobot SelectPortForwarding(bool state)
    {
        PortForwardingDropDown.Click();

        Thread.Sleep(TestConstants.AnimationDelay);

        Element portForwardingMenuItem = state ? PortForwardingOnMenuItem : PortForwardingOffMenuItem;
        portForwardingMenuItem.DoubleClick();

        return this;
    }

    public ProfileRobot SelectConnectAndGoOption(ConnectAndGoOption option)
    {
        ConnectAndGoDropDown.Click();

        Thread.Sleep(TestConstants.AnimationDelay);

        switch (option)
        {
            case ConnectAndGoOption.Off:
                ConnectAndGoOffMenuItem.DoubleClick();
                break;
            case ConnectAndGoOption.OpenWebsite:
                ConnectAndGoWebsiteMenuItem.DoubleClick();
                break;
            case ConnectAndGoOption.OpenApp:
                ConnectAndGoApplicationMenuItem.DoubleClick();
                break;
        }

        return this;
    }

    public ProfileRobot SelectConnectAndGoApp(string appPath)
    {
        Thread.Sleep(TestConstants.NavigationDelay);
        ConnectAndGoAppSelector.Click();
        Thread.Sleep(TestConstants.NavigationDelay);
        HandleExplorer(appPath);
        return this;
    }

    public ProfileRobot TypeConnectAndGoWebsite(string websiteUrl, bool usePrivateMode = false)
    {
        AutomationElement[] children = ConnectAndGoParent.GetControlType(ControlType.Edit);
        children[0].AsTextBox().Text = websiteUrl;

        if (usePrivateMode)
        {
            ConnectAndGoPrivateModeCheckbox.Click();
        }

        return this;
    }

    public class Verifications : ProfileRobot
    {
        public Verifications IsAppSelected(string appName)
        {
            List<string> allChildren = ConnectAndGoAppSelector.GetAllChildrenNames();
            Assert.That(allChildren, Does.Contain(appName));
            return this;
        }

        public Verifications ProfileNameEquals(string profileName)
        {
            ProfileNameTextBox.TextBoxEquals(profileName);
            return this;
        }
    }

    private ProfileRobot HandleExplorer(string appPath)
    {
        Thread.Sleep(TestConstants.FiveSecondsTimeout);
        Keyboard.Type(appPath);
        Thread.Sleep(TestConstants.OneSecondTimeout);
        Keyboard.Press(VirtualKeyShort.TAB);
        Keyboard.Press(VirtualKeyShort.TAB);
        Thread.Sleep(TestConstants.OneSecondTimeout);
        Keyboard.Press(VirtualKeyShort.ENTER);
        Thread.Sleep(TestConstants.OneSecondTimeout);
        return this;
    }

    public Verifications Verify => new();
}