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
using static ProtonVPN.UI.Tests.TestsHelper.TestConstants;

namespace ProtonVPN.UI.Tests.Robots;

public class SettingRobot
{
    private const string NETSHIELD_NO_BLOCK = "netshield-0.protonvpn.net";
    private const string NETSHIELD_MALWARE_ENDPOINT = "netshield-1.protonvpn.net";
    private const string NETSHIELD_ADS_ENDPOINT = "netshield-2.protonvpn.net";

    protected Element SettingsPage = Element.ByAutomationId("SettingsPage");
    protected Element ApplyButton = Element.ByAutomationId("ApplyButton");
    protected Element CloseSettingsButton = Element.ByAutomationId("CloseSettingsButton");
    protected Element ReconnectButton = Element.ByName("Reconnect");
    protected Element SettingsButton = Element.ByAutomationId("SettingsButton");
    protected Element LastDefaultConnectionRadioButton = Element.ByAutomationId("LastDefaultConnectionRadioButton");
    protected Element FastestDefaultConnectionRadioButton = Element.ByAutomationId("FastestDefaultConnectionRadioButton");
    protected Element RandomDefaultConnectionRadioButton = Element.ByAutomationId("RandomDefaultConnectionRadioButton");

    protected Element NetShieldSettingsCard = Element.ByAutomationId("NetShieldSettingsCard");
    protected Element ProtocolSettingsCard = Element.ByAutomationId("ProtocolSettingsCard");
    protected Element AdvancedSettingsCard = Element.ByAutomationId("AdvancedSettingsCard");
    protected Element PortForwardingSettingsCard = Element.ByAutomationId("PortForwardingSettingsCard");
    protected Element SplitTunnelingSettingsCard = Element.ByAutomationId("SplitTunnelingSettingsCard");
    protected Element VpnAcceleratorSettingsCard = Element.ByAutomationId("VpnAcceleratorSettingsCard");
    protected Element DefaultConnectionSettingsCard = Element.ByAutomationId("DefaultConnectionSettingsCard");

    protected Element AutoStartupSettingsCard = Element.ByAutomationId("AutoStartupSettingsCard");
    protected Element ReportIssueSettingsCard = Element.ByAutomationId("ReportIssueSettingsCard");
    protected Element AboutSettingsCard = Element.ByAutomationId("AboutSettingsCard");
    protected Element GoBackButton = Element.ByAutomationId("GoBackButton");
    protected Element AccountButton = Element.ByAutomationId("AccountButton");
    protected Element SignOutButton = Element.ByName("Sign out");
    protected Element PrimaryActionButton = Element.ByAutomationId("PrimaryButton");
    protected Element CancelButton = Element.ByAutomationId("CloseButton");
    protected Element ExitTheAppButton = Element.ByName("Exit the app");
    protected Element ChangeLogLabel = Element.ByName("Changelog");
    protected Element LicensingLabel = Element.ByAutomationId("LicensingTextBlock");
    protected Element LearnMoreButton = Element.ByName("Learn more");
    protected Element CurrentVersionLabel = Element.ByAutomationId("CurrentVersionLabel");

    protected Element NetshieldToggle = Element.ByAutomationId("NetshieldToggle");
    protected Element NetShieldLevelOneRadioButton = Element.ByAutomationId("NetShieldLevelOne");
    protected Element NetShieldLevelTwoRadioButton = Element.ByAutomationId("NetShieldLevelTwo");

    protected Element AutoLaunchToggle = Element.ByAutomationId("AutoLaunchToggle");
    protected Element AutoConnectToggle = Element.ByAutomationId("AutoConnectToggle");

    protected Element OpenVpnTcpProtocolRadioButton = Element.ByAutomationId("OpenVpnTcpProtocolRadioButton");
    protected Element OpenVpnUdpProtocolRadioButton = Element.ByAutomationId("OpenVpnUdpProtocolRadioButton");
    protected Element WireGuardUdpProtocolRadioButton = Element.ByAutomationId("WireGuardUdpProtocolRadioButton");
    protected Element WireGuardTlsProtocolRadioButton = Element.ByAutomationId("WireGuardTlsProtocolRadioButton");
    protected Element WireGuardTcpProtocolRadioButton = Element.ByAutomationId("WireGuardTcpProtocolRadioButton");
    
    public SettingRobot OpenSettings()
    {
        SettingsButton.Click();
        Thread.Sleep(TestConstants.NavigationDelay);
        return this;
    }

    public SettingRobot OpenSettingsViaShortcut()
    {
        Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.OEM_COMMA);
        return this;
    }

    public SettingRobot CloseSettings()
    {
        CloseSettingsButton.Invoke();
        Thread.Sleep(TestConstants.NavigationDelay);
        return this;
    }

    public SettingRobot OpenNetShieldSettings()
    {
        NetShieldSettingsCard.Click();
        Thread.Sleep(TestConstants.NavigationDelay);
        return this;
    }

    public SettingRobot OpenProtocolSettings()
    {
        ProtocolSettingsCard.Click();
        Thread.Sleep(TestConstants.NavigationDelay);
        return this;
    }

    public SettingRobot OpenAdvancedSettings()
    {
        AdvancedSettingsCard.Click();
        Thread.Sleep(TestConstants.NavigationDelay);
        return this;
    }

    public SettingRobot OpenPortForwardingSettings()
    {
        PortForwardingSettingsCard.Click();
        Thread.Sleep(TestConstants.NavigationDelay);
        return this;
    }

    public SettingRobot OpenSplitTunnelingSettingsCard()
    {
        SplitTunnelingSettingsCard.Click();
        Thread.Sleep(TestConstants.NavigationDelay);
        return this;
    }

    public SettingRobot OpenVpnAcceleratorSettingsCard()
    {
        VpnAcceleratorSettingsCard.Click();
        Thread.Sleep(TestConstants.NavigationDelay);
        return this;
    }

    public SettingRobot OpenDefaultConnectionSettingsCard()
    {
        DefaultConnectionSettingsCard.ScrollIntoView();
        DefaultConnectionSettingsCard.Click();
        Thread.Sleep(TestConstants.NavigationDelay);
        return this;
    }

    public SettingRobot SignOut()
    {
        // Due to focus issues double click is required to trigger click event.
        SignOutButton.DoubleClick();
        return this;
    }

    public SettingRobot OpenAutoStartupSettings()
    {
        AutoStartupSettingsCard.Click();
        Thread.Sleep(TestConstants.NavigationDelay);
        return this;
    }

    public SettingRobot ExpandAccountDropdown()
    {
        AccountButton.Invoke();
        // Remove when VPNWIN-2599 is implemented.
        Thread.Sleep(TestConstants.AnimationDelay);
        return this;
    }

    public SettingRobot OpenSplitTunnelingSettings()
    {
        SplitTunnelingSettingsCard.Click();
        // It takes some time to load split tunneling list. There are no hooks to wait for.
        Thread.Sleep(1000);
        return this;
    }

    public SettingRobot OpenBugReportSetting()
    {
        ReportIssueSettingsCard.ScrollIntoView();
        ReportIssueSettingsCard.Click();
        Thread.Sleep(TestConstants.NavigationDelay);
        return this;
    }

    public SettingRobot ScrollToAboutSection()
    {
        AboutSettingsCard.ScrollIntoView();
        return this;
    }

    public SettingRobot OpenAboutSection()
    {
        AboutSettingsCard.Click();
        Thread.Sleep(TestConstants.NavigationDelay);
        return this;
    }

    public SettingRobot ConfirmSignOut()
    {
        PrimaryActionButton.Click();
        return this;
    }

    public SettingRobot CancelSignOut()
    {
        CancelButton.Click();
        return this;
    }

    public SettingRobot ExitTheApp()
    {
        ExitTheAppButton.DoubleClick();
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
        NetshieldToggle.Toggle();
        return this;
    }

    public SettingRobot ToggleAutoLaunchSetting()
    {
        AutoLaunchToggle.Toggle();
        return this;
    }

    public SettingRobot ToggleAutoConnectionSetting()
    {
        AutoConnectToggle.Toggle();
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

    public SettingRobot CloseSettingsUsingEscButton()
    {
        Keyboard.Type(VirtualKeyShort.ESC);
        return this;
    }

    public SettingRobot PressLearnMore()
    {
        LearnMoreButton.Click();
        return this;
    }

    public SettingRobot SelectLastConnectionOption()
    {
        LastDefaultConnectionRadioButton.Click();
        return this;
    }

    public SettingRobot SelectFastestConnectionOption()
    {
        FastestDefaultConnectionRadioButton.Click();
        return this;
    }

    public SettingRobot SelectProfileDefaultConnectionOption(string profileName)
    {
        Element.ByName(profileName).Click();
        return this;
    }

    public class Verifications : SettingRobot
    {
        public Verifications IsNetshieldBlocking(NetShieldMode netShieldMode)
        {
            NetworkUtils.FlushDns();
            CommonAssertions.AssertDnsIsResolved(NETSHIELD_NO_BLOCK);

            switch (netShieldMode)
            {
                case NetShieldMode.BlockMalwareOnly:
                    CommonAssertions.AssertDnsIsNotResolved(NETSHIELD_MALWARE_ENDPOINT);
                    CommonAssertions.AssertDnsIsResolved(NETSHIELD_ADS_ENDPOINT);
                    break;
                case NetShieldMode.BlockAdsMalwareTrackers:
                    CommonAssertions.AssertDnsIsNotResolved(NETSHIELD_MALWARE_ENDPOINT);
                    CommonAssertions.AssertDnsIsNotResolved(NETSHIELD_ADS_ENDPOINT);
                    break;
            }

            return this;
        }

        public Verifications IsNetshieldDisableStateDisplayed()
        {
            NetShieldSettingsCard.FindChild(Element.ByName("Off")).WaitUntilDisplayed();
            return this;
        }

        public Verifications IsNetshieldEnabledStateDisplayed()
        {
            NetShieldSettingsCard.FindChild(Element.ByName("On")).WaitUntilDisplayed();
            return this;
        }

        public Verifications IsNetshieldNotBlocking()
        {
            NetworkUtils.FlushDns();
            CommonAssertions.AssertDnsIsResolved(NETSHIELD_NO_BLOCK);
            CommonAssertions.AssertDnsIsResolved(NETSHIELD_MALWARE_ENDPOINT);
            CommonAssertions.AssertDnsIsResolved(NETSHIELD_ADS_ENDPOINT);
            return this;
        }

        public Verifications IsSettingsPageDisplayed()
        {
            SettingsPage.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsSettingsPageNotDisplayed()
        {
            SettingsPage.DoesNotExist();
            return this;
        }

        public Verifications IsChangelogDispalyed()
        {
            ChangeLogLabel.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsLicensingDisplayed()
        {
            LicensingLabel.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsCorrectAppVersionDisplayedInAboutSettingsCard(string appVersion)
        {
            Element.ByName($"App version: {appVersion}");
            return this;
        }

        public Verifications IsCorrectAppVersionDisplayedInAboutSection(string appVersion)
        {
            CurrentVersionLabel.TextEquals(appVersion);
            return this;
        }

        public Verifications IsAutoConnectEnabled()
        {
            AutoConnectToggle.IsToggled();
            return this;
        }
    }

    public SettingRobot SelectDefaultConnectionType(VpnConnectionOptions option)
    {
        switch (option)
        {
            case VpnConnectionOptions.Fast:
                FastestDefaultConnectionRadioButton.Click();
                FastestDefaultConnectionRadioButton.AssertChecked();
                break;

            case VpnConnectionOptions.Random:
                RandomDefaultConnectionRadioButton.Click();
                RandomDefaultConnectionRadioButton.AssertChecked();
                break;

            case VpnConnectionOptions.Last:
                LastDefaultConnectionRadioButton.Click();
                LastDefaultConnectionRadioButton.AssertChecked();
                break;
        }
        return this;
    }

    public Verifications Verify => new();
}