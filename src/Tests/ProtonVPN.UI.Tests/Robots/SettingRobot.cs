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

using System.Threading;
using System.Collections.Generic;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.UiTools;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Robots;

public class SettingRobot
{
    private const string NETSHIELD_NO_BLOCK = "netshield-0.protonvpn.net";
    private const string NETSHIELD_MALWARE_ENDPOINT = "netshield-1.protonvpn.net";
    private const string NETSHIELD_ADS_ENDPOINT = "netshield-2.protonvpn.net";
    private static readonly string[] _netShieldAdultContentDomains = { "0-100c.cn", "0-1du.com", "0-24sexcams.com", "0-6babylee.cn", "0-900.com" };

    protected Element SettingsPage = Element.ByAutomationId("SettingsPage");
    protected Element ApplyButton = Element.ByAutomationId("ApplyButton");
    protected Element CloseSettingsButton = Element.ByAutomationId("CloseSettingsButton");
    protected Element ReconnectButton = Element.ByName("Reconnect");
    protected Element SettingsButton = Element.ByAutomationId("SettingsButton");

    protected Element NetShieldSettingsCard = Element.ByAutomationId("NetShieldSettingsCard");
    protected Element KillSwitchSettingsCard = Element.ByAutomationId("KillSwitchSettingsCard");
    protected Element ProtocolSettingsCard = Element.ByAutomationId("ProtocolSettingsCard");
    protected Element AdvancedSettingsCard = Element.ByAutomationId("AdvancedSettingsCard");
    protected Element PortForwardingSettingsCard = Element.ByAutomationId("PortForwardingSettingsCard");
    protected Element SplitTunnelingSettingsCard = Element.ByAutomationId("SplitTunnelingSettingsCard");
    protected Element VpnAcceleratorSettingsCard = Element.ByAutomationId("VpnAcceleratorSettingsCard");
    protected Element ConnectionPreferencesSettingsCard = Element.ByAutomationId("ConnectionPreferencesSettingsCard");
    protected Element ExcludedLocationSelectorButton = Element.ByAutomationId("SelectorButton");
    protected Element RemoveExcludedLocationButton = Element.ByAutomationId("RemoveExcludedLocationButton");
    protected Element PortForwardingToggle = Element.ByAutomationId("PortForwardingToggle");
    protected Element CopyPortNumberButton = Element.ByAutomationId("CopyPortNumberCondensedButton");

    protected Element AutoStartupSettingsCard = Element.ByAutomationId("AutoStartupSettingsCard");
    protected Element ReportIssueSettingsCard = Element.ByAutomationId("ReportIssueSettingsCard");
    protected Element AboutSettingsCard = Element.ByAutomationId("AboutSettingsCard");
    protected Element GoBackButton = Element.ByAutomationId("GoBackButton");
    protected Element AccountButton = Element.ByAutomationId("AccountButton");
    protected Element SignOutButton = Element.ByName("Sign out");
    protected Element PrimaryActionButton = Element.ByAutomationId("PrimaryButton");
    protected Element CancelButton = Element.ByAutomationId("CloseButton");
    protected Element ExitTheAppButton = Element.ByName("Exit the app");
    protected Element ExitButton = Element.ByName("Exit");
    protected Element ChangeLogLabel = Element.ByName("Changelog");
    protected Element LicensingLabel = Element.ByAutomationId("LicensingTextBlock");
    protected Element LearnMoreButton = Element.ByName("Learn more");
    protected Element CurrentVersionLabel = Element.ByAutomationId("CurrentVersionLabel");
    protected Element DefaultConnectionDropdown = Element.ByAutomationId("DefaultConnectionDropdown");

    protected Element NetshieldToggle = Element.ByAutomationId("NetshieldToggle");
    protected Element NetShieldLevelOneRadioButton = Element.ByAutomationId("NetShieldLevelOne");
    protected Element NetShieldLevelTwoRadioButton = Element.ByAutomationId("NetShieldLevelTwo");
    protected Element NetShieldLevelThreeRadioButton = Element.ByAutomationId("NetShieldLevelThree");
    protected Element KillSwitchToggle = Element.ByAutomationId("KillSwitchToggle");
    protected Element KillSiwtchStandardRadioButton = Element.ByAutomationId("StandardKillSwitchRadioButton");
    protected Element KillSwitchAdvancedRadioButton = Element.ByAutomationId("AdvancedKillSwitchRadioButton");

    protected Element NatTypeStrictRadioButton = Element.ByAutomationId("StrictNatTypeRadioButton");
    protected Element NatTypeModerateRadioButton = Element.ByAutomationId("ModerateNatTypeRadioButton");

    protected Element AutoLaunchToggle = Element.ByAutomationId("AutoLaunchToggle");
    protected Element AutoConnectToggle = Element.ByAutomationId("AutoConnectToggle");

    protected Element ProtonProtocolsToggle = Element.ByName("Proton protocols").And(Element.ByClassName("NamedContainerAutomationPeer"));
    protected Element OpenVpnTcpProtocolRadioButton = Element.ByAutomationId("OpenVpnTcpProtocolRadioButton");
    protected Element OpenVpnUdpProtocolRadioButton = Element.ByAutomationId("OpenVpnUdpProtocolRadioButton");
    protected Element WireGuardUdpProtocolRadioButton = Element.ByAutomationId("WireGuardUdpProtocolRadioButton");
    protected Element WireGuardTlsProtocolRadioButton = Element.ByAutomationId("WireGuardTlsProtocolRadioButton");
    protected Element WireGuardTcpProtocolRadioButton = Element.ByAutomationId("WireGuardTcpProtocolRadioButton");
    protected Element SmartProtocolRadioButton = Element.ByAutomationId("SmartProtocolRadioButton");
    protected Element ExitProtonPopUp = Element.ByName("Exit Proton VPN?");

    public SettingRobot OpenSettings()
    {
        Thread.Sleep(TestConstants.NavigationDelay);
        SettingsButton.ClickUntilElementDisappears();
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

    public SettingRobot OpenKillSwitchSettings()
    {
        KillSwitchSettingsCard.Click();
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

    public SettingRobot OpenConnectionPreferencesSettingsCard()
    {
        ConnectionPreferencesSettingsCard.ScrollIntoView();
        ConnectionPreferencesSettingsCard.Click();
        Thread.Sleep(TestConstants.NavigationDelay);
        return this;
    }

    public SettingRobot OpenExcludedLocationsSelector()
    {
        ExcludedLocationSelectorButton.Click();
        Thread.Sleep(TestConstants.AnimationDelay);
        return this;
    }

    public SettingRobot SelectExcludedCountry(string countryName)
    {
        Element.ByName(countryName).Click();
        Thread.Sleep(TestConstants.AnimationDelay);
        RemoveExcludedLocationButton.WaitUntilDisplayed();
        return this;
    }

    public SettingRobot SearchExcludedLocations(string searchText)
    {
        Keyboard.Type(searchText);
        Thread.Sleep(TestConstants.AnimationDelay);
        return this;
    }

    public SettingRobot RemoveFirstExcludedLocation()
    {
        RemoveExcludedLocationButton.Click();
        Thread.Sleep(TestConstants.AnimationDelay);
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
        AutoStartupSettingsCard.ScrollIntoView();
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

    public SettingRobot ExitTheAppWithConfirmation()
    {
        ExitTheAppButton.DoubleClick();
        ExitProtonPopUp.WaitUntilDisplayed();
        ExitButton.Click();
        return this;
    }

    public SettingRobot ToggleProtun()
    {
        ProtonProtocolsToggle.FindChild(Element.ByClassName("ToggleSwitch")).Toggle();
        return this;
    }

    public SettingRobot SelectProtocol(TestConstants.Protocol protocol)
    {
        switch (protocol)
        {
            case TestConstants.Protocol.Smart:
                SmartProtocolRadioButton.Click();
                break;

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

    public SettingRobot ToggleKillSwitchSetting()
    {
        KillSwitchToggle.Toggle();
        return this;
    }
    public SettingRobot DisableKillSwitch()
    {
        if (KillSwitchToggle.IsToggled())
        {
            KillSwitchToggle.Toggle();
            ApplyButton.Invoke();
        }
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

    public SettingRobot TogglePortForwardingnSetting()
    {
        PortForwardingToggle.Toggle();
        return this;
    }

    public SettingRobot ClickCopyPortNumber()
    {
        CopyPortNumberButton.Invoke();
        return this;
    }

    public SettingRobot SelectNatType(NatType natType)
    {
        if (natType == NatType.Strict)
        {
            NatTypeStrictRadioButton.Click();
        }
        else if (natType == NatType.Moderate)
        {
            NatTypeModerateRadioButton.Click();
        }

        return this;
    }

    public SettingRobot SelectNetShieldMode(NetShieldMode netShieldMode)
    {
        switch (netShieldMode)
        {
            case NetShieldMode.BlockMalwareOnly:
                NetShieldLevelOneRadioButton.Click();
                break;
            case NetShieldMode.BlockAdsMalwareTrackers:
                NetShieldLevelTwoRadioButton.Click();
                break;
            case NetShieldMode.BlockAdsMalwareTrackersAdultContent:
                NetShieldLevelThreeRadioButton.Click();
                break;
        }

        return this;
    }

    public SettingRobot SelectKillSwitchMode(KillSwitchMode killSwitchMode)
    {
        if (killSwitchMode == KillSwitchMode.Standard)
        {
            KillSiwtchStandardRadioButton.Click();
        }
        else if (killSwitchMode == KillSwitchMode.Advanced)
        {
            KillSwitchAdvancedRadioButton.Click();
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
        return SelectDefaultConnectionType(VpnConnectionOptions.Last);
    }

    public SettingRobot SelectFastestConnectionOption()
    {
        return SelectDefaultConnectionType(VpnConnectionOptions.Fast);
    }

    public SettingRobot SelectProfileDefaultConnectionOption(string profileName)
    {
        Element.ByName(profileName).Click();
        return this;
    }

    public class Verifications : SettingRobot
    {
        public Verifications IsCorrectAccountInfoDisplayed(string accountName, string accountPlan)
        {
            Thread.Sleep(TestConstants.OneSecondTimeout);
            List<string> allChildren = AccountButton.GetAllChildrenNames();
            Assert.That(allChildren, Does.Contain(accountName));
            Assert.That(allChildren, Does.Contain(accountPlan));
            return this;
        }

        public Verifications IsProtunEnabled()
        {
            ProtonProtocolsToggle.FindChild(Element.ByClassName("ToggleSwitch")).IsToggled();
            return this;
        }

        public Verifications IsNetshieldBlocking(NetShieldMode netShieldMode)
        {
            NetworkUtils.FlushDns();
            CommonAssertions.AssertDnsIsResolved(NETSHIELD_NO_BLOCK);

            switch (netShieldMode)
            {
                case NetShieldMode.BlockMalwareOnly:
                    CommonAssertions.AssertDnsIsNotResolved(NETSHIELD_MALWARE_ENDPOINT);
                    CommonAssertions.AssertDnsIsResolved(NETSHIELD_ADS_ENDPOINT);
                    CommonAssertions.AssertDnsIsResolved(_netShieldAdultContentDomains[0]);
                    break;
                case NetShieldMode.BlockAdsMalwareTrackers:
                    CommonAssertions.AssertDnsIsNotResolved(NETSHIELD_MALWARE_ENDPOINT);
                    CommonAssertions.AssertDnsIsNotResolved(NETSHIELD_ADS_ENDPOINT);
                    CommonAssertions.AssertDnsIsResolved(_netShieldAdultContentDomains[0]);
                    break;
                case NetShieldMode.BlockAdsMalwareTrackersAdultContent:
                    CommonAssertions.AssertDnsIsNotResolved(NETSHIELD_MALWARE_ENDPOINT);
                    CommonAssertions.AssertDnsIsNotResolved(NETSHIELD_ADS_ENDPOINT);
                    foreach (string adultContentDomain in _netShieldAdultContentDomains)
                    {
                        CommonAssertions.AssertDnsIsNotResolved(adultContentDomain);
                    }
                    break;
            }

            return this;
        }

        public Verifications IsNetshieldDisabledStateDisplayed()
        {
            NetShieldSettingsCard.FindChild(Element.ByName("Off")).WaitUntilDisplayed();
            return this;
        }

        public Verifications IsNetshieldEnabledStateDisplayed()
        {
            NetShieldSettingsCard.FindChild(Element.ByName("On")).WaitUntilDisplayed();
            return this;
        }

        public Verifications IsKillSwitchDisabledStateDisplayed()
        {
            KillSwitchSettingsCard.FindChild(Element.ByName("Off")).WaitUntilDisplayed();
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

        public Verifications IsExcludedLocationDisplayed(string countryName)
        {
            SettingsPage.FindDescendant(Element.ByName(countryName)).WaitUntilExists();
            return this;
        }

        public Verifications IsExcludedLocationNotDisplayed(string countryName)
        {
            SettingsPage.FindDescendant(Element.ByName(countryName)).DoesNotExist();
            return this;
        }

        public Verifications IsRemoveExcludedLocationButtonDisplayed()
        {
            RemoveExcludedLocationButton.WaitUntilDisplayed();
            return this;
        }
    }

    public SettingRobot SelectDefaultConnectionType(VpnConnectionOptions option)
    {
        Element settingsDefaultConnectionComboBox = SettingsPage
            .FindDescendant(DefaultConnectionDropdown);
        
        settingsDefaultConnectionComboBox.Click();
        Thread.Sleep(TestConstants.AnimationDelay);

        string optionName = option switch
        {
            VpnConnectionOptions.Fast => "Fastest country",
            VpnConnectionOptions.Random => "Random country",
            VpnConnectionOptions.Last => "Last connection",
            _ => throw new System.NotImplementedException($"VpnConnectionOption '{option}' is not supported in Settings."),
        };

        Element.ByName(optionName).Click();
        Thread.Sleep(TestConstants.AnimationDelay);

        settingsDefaultConnectionComboBox.ComboBoxSelectedEquals(optionName);

        return this;
    }

    public Verifications Verify => new();
}