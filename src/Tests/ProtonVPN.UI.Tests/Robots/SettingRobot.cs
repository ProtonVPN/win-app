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
using System.Threading;
using System.Collections.Generic;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.UiTools;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.Enums.Locations;

namespace ProtonVPN.UI.Tests.Robots;

public class SettingRobot
{
    private const string NETSHIELD_NO_BLOCK = "netshield-0.protonvpn.net";
    private const string NETSHIELD_MALWARE_ENDPOINT = "netshield-1.protonvpn.net";
    private const string NETSHIELD_ADS_ENDPOINT = "netshield-2.protonvpn.net";
    private static readonly string[] _netShieldAdultContentDomains = { "0-6babylee.cn", "0-900.com", "0-100c.cn", "0-24sexcams.com", "0-1du.com" };

    private static readonly string _onStateTranslated = LanguageHelper.GetTranslatedString("Common_States_On");
    private static readonly string _offStateTranslated = LanguageHelper.GetTranslatedString("Common_States_Off");

    private static readonly string _reconnectButtonTranslated = LanguageHelper.GetTranslatedString("Common_Actions_Reconnect");
    private static readonly string _changeLogLabelTranslated = LanguageHelper.GetTranslatedString("Settings_About_Changelog");
    private static readonly string _learnMoreButtonTranslated = LanguageHelper.GetTranslatedString("Settings_About_LearnMore");
    private static readonly string _exitTheAppButtonTranslated = LanguageHelper.GetTranslatedString("Settings_Account_Exit");
    private static readonly string _exitProtonPopUpTranslated = LanguageHelper.GetTranslatedString("Exit_Confirmation_Title");

    private static readonly string _noLocationsAvailableTranslated = LanguageHelper.GetTranslatedString("Settings_Connection_ExcludedLocations_NoLocations");

    protected Element ReconnectButton = Element.ByName(_reconnectButtonTranslated);
    protected Element ChangeLogLabel = Element.ByName(_changeLogLabelTranslated);
    protected Element LearnMoreButton = Element.ByName(_learnMoreButtonTranslated);
    protected Element ExitTheAppButton = Element.ByName(_exitTheAppButtonTranslated);
    protected Element ExitProtonPopUp = Element.ByName(_exitProtonPopUpTranslated);

    protected Element SignOutButton = Element.ByAutomationId("SignOutMenuItem");
    protected Element SettingsPage = Element.ByAutomationId("SettingsPage");
    protected Element ApplyButton = Element.ByAutomationId("ApplyButton");
    protected Element CloseSettingsButton = Element.ByAutomationId("CloseSettingsButton");
    protected Element SettingsButton = Element.ByAutomationId("SettingsButton");

    protected Element NetShieldSettingsCard = Element.ByAutomationId("NetShieldSettingsCard");
    protected Element KillSwitchSettingsCard = Element.ByAutomationId("KillSwitchSettingsCard");
    protected Element ProtocolSettingsCard = Element.ByAutomationId("ProtocolSettingsCard");
    protected Element AdvancedSettingsCard = Element.ByAutomationId("AdvancedSettingsCard");
    protected Element PortForwardingSettingsCard = Element.ByAutomationId("PortForwardingSettingsCard");
    protected Element SplitTunnelingSettingsCard = Element.ByAutomationId("SplitTunnelingSettingsCard");
    protected Element VpnAcceleratorSettingsCard = Element.ByAutomationId("VpnAcceleratorSettingsCard");
    protected Element ConnectionPreferencesSettingsCard = Element.ByAutomationId("ConnectionPreferencesSettingsCard");
    protected Element PortForwardingToggle = Element.ByAutomationId("PortForwardingToggle");
    protected Element CopyPortNumberButton = Element.ByAutomationId("CopyPortNumberCompactButton");

    protected Element ExcludedLocationSelectorButton = Element.ByAutomationId("SelectorButton");
    protected Element ExcludedLocationSearchTextBox = Element.ByAutomationId("ExcludedLocationSearchTextBox");
    protected Element RemoveExcludedLocationButton = Element.ByAutomationId("RemoveExcludedLocationButton");

    public Element ExcludedLocationFlyout => Element.ByAutomationId("ExcludedLocationFlyout");
    protected Element NoLocationsAvailable => ExcludedLocationFlyout.FindChild(Element.ByName(_noLocationsAvailableTranslated));

    protected Element AutoStartupSettingsCard = Element.ByAutomationId("AutoStartupSettingsCard");
    protected Element ReportIssueSettingsCard = Element.ByAutomationId("ReportIssueSettingsCard");
    protected Element AboutSettingsCard = Element.ByAutomationId("AboutSettingsCard");
    protected Element GoBackButton = Element.ByAutomationId("GoBackButton");
    protected Element AccountButton = Element.ByAutomationId("AccountButton");
    protected Element PrimaryActionButton = Element.ByAutomationId("PrimaryButton");
    protected Element CancelButton = Element.ByAutomationId("CloseButton");

    protected Element LicensingLabel = Element.ByAutomationId("LicensingTextBlock");
    protected Element CurrentVersionLabel = Element.ByAutomationId("CurrentVersionLabel");
    protected Element DefaultConnectionDropdown = Element.ByAutomationId("DefaultConnectionDropdown");

    protected Element VpnAcceleratorToggle = Element.ByAutomationId("VpnAcceleratorToggle");

    protected Element NetShieldToggle = Element.ByAutomationId("NetshieldToggle");
    protected Element NetShieldLevelOneRadioButton = Element.ByAutomationId("NetShieldLevelOne");
    protected Element NetShieldLevelTwoRadioButton = Element.ByAutomationId("NetShieldLevelTwo");
    protected Element NetShieldLevelThreeRadioButton = Element.ByAutomationId("NetShieldLevelThree");
    protected Element KillSwitchToggle = Element.ByAutomationId("KillSwitchToggle");
    protected Element KillSwitchStandardRadioButton = Element.ByAutomationId("StandardKillSwitchRadioButton");
    protected Element KillSwitchAdvancedRadioButton = Element.ByAutomationId("AdvancedKillSwitchRadioButton");

    protected Element AutoLaunchToggle = Element.ByAutomationId("AutoLaunchToggle");
    protected Element AutoConnectToggle = Element.ByAutomationId("AutoConnectToggle");
    protected Element MinimizeToSystemTrayRadioButton = Element.ByAutomationId("MinimizeToSystemTrayRadioButton");
    protected Element OpenOnDesktopRadioButton = Element.ByAutomationId("OpenOnDesktopRadioButton");
    protected Element MainPage => Element.ByAutomationId("MainPage");

    protected Element NotificationsToggle = Element.ByAutomationId("NotificationsToggle");
    protected Element SupportCenterSettingsCard = Element.ByAutomationId("SupportCenterSettingsCard");
    protected Element DebugLogsSettingsCard = Element.ByAutomationId("DebugLogsSettingsCard");
    protected Element ApplicationLogsSettingsCard = Element.ByAutomationId("ApplicationLogsSettingsCard");
    protected Element ServiceLogsSettingsCard = Element.ByAutomationId("ServiceLogsSettingsCard");

    protected Element LanguageComboBox = Element.ByAutomationId("cbLanguage");

    protected Element RestoreDefaultSettingsButton = Element.ByAutomationId("RestoreDefaultSettingsButton");

    protected Element ProtonProtocolsToggle = Element.ByAutomationId("ProtonProtocolsToggle");
    protected Element OpenVpnTcpProtocolRadioButton = Element.ByAutomationId("OpenVpnTcpProtocolRadioButton");
    protected Element OpenVpnUdpProtocolRadioButton = Element.ByAutomationId("OpenVpnUdpProtocolRadioButton");
    protected Element WireGuardUdpProtocolRadioButton = Element.ByAutomationId("WireGuardUdpProtocolRadioButton");
    protected Element WireGuardTlsProtocolRadioButton = Element.ByAutomationId("WireGuardTlsProtocolRadioButton");
    protected Element WireGuardTcpProtocolRadioButton = Element.ByAutomationId("WireGuardTcpProtocolRadioButton");
    protected Element ProTunUdpProtocolRadioButton = Element.ByAutomationId("ProTunUdpProtocolRadioButton");
    protected Element ProTunTcpProtocolRadioButton = Element.ByAutomationId("ProTunTcpProtocolRadioButton");
    protected Element ProTunTlsProtocolRadioButton = Element.ByAutomationId("ProTunTlsProtocolRadioButton");
    protected Element SmartProtocolRadioButton = Element.ByAutomationId("SmartProtocolRadioButton");

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

    public SettingRobot OpenVpnAcceleratorSettings()
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

    public SettingRobot SelectExcludedCountry(Country countryName)
    {
        Thread.Sleep(TestConstants.AnimationDelay);
        Element.ByName(countryName.GetName()).Click();
        Thread.Sleep(TestConstants.AnimationDelay);
        RemoveExcludedLocationButton.WaitUntilDisplayed();
        return this;
    }

    public SettingRobot SearchExcludedLocations(string searchText, bool useKeyboard = true)
    {
        if (useKeyboard)
        {
            Keyboard.Type(searchText);
        }
        else
        {
            ExcludedLocationSearchTextBox.SetText(searchText);
        }

        Thread.Sleep(TestConstants.OneSecondTimeout);
        return this;
    }

    public SettingRobot RemoveFirstExcludedLocation()
    {
        RemoveExcludedLocationButton.Click();
        Thread.Sleep(TestConstants.AnimationDelay);
        return this;
    }

    public SettingRobot OpenAutoStartupSettings()
    {
        AutoStartupSettingsCard.ScrollIntoView().Click();
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
        Thread.Sleep(TestConstants.NavigationDelay);
        return this;
    }

    public SettingRobot OpenBugReportSetting()
    {
        ReportIssueSettingsCard.ScrollIntoView().Click();
        Thread.Sleep(TestConstants.NavigationDelay);
        return this;
    }

    public SettingRobot EnableNotificationsToggle()
    {
        if (!NotificationsToggle.ScrollIntoView().IsToggled())
        {
            NotificationsToggle.Toggle();
        }
        return this;
    }

    public SettingRobot DisableNotificationsToggle()
    {
        if (NotificationsToggle.ScrollIntoView().IsToggled())
        {
            NotificationsToggle.Toggle();
        }
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

    public SettingRobot ClickRestoreDefaultSettings()
    {
        RestoreDefaultSettingsButton.ScrollIntoView().Click();
        return this;
    }

    public SettingRobot SignOut()
    {
        SignOutButton.ClickUntilElementDisappears();
        return this;
    }

    public SettingRobot ConfirmSignOut()
    {
        PrimaryActionButton.Click(TestConstants.TwoSecondsTimeout);
        Thread.Sleep(TestConstants.UserInputSimulationDelay);
        Keyboard.Press(VirtualKeyShort.ENTER);

        try
        {
            PrimaryActionButton.Invoke(TestConstants.TwoSecondsTimeout);
        }
        catch { }

        try
        {
            PrimaryActionButton.Click(TestConstants.TwoSecondsTimeout);
        }
        catch { }

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
        PrimaryActionButton.Click();
        return this;
    }

    public SettingRobot EnableProtunToggle()
    {
        if (!ProtonProtocolsToggle.IsToggled())
        {
            ProtonProtocolsToggle.Toggle();
        }
        return this;
    }

    public SettingRobot DisableProtunToggle()
    {
        if (ProtonProtocolsToggle.IsToggled())
        {
            ProtonProtocolsToggle.Toggle();
        }
        return this;
    }

    public SettingRobot SelectProtocol(Protocol protocol)
    {
        GetProtocolRadioButton(protocol).Click();
        return this;
    }

    public SettingRobot EnableVpnAcceleratorToggle()
    {
        if (!VpnAcceleratorToggle.IsToggled())
        {
            VpnAcceleratorToggle.Toggle();
        }
        return this;
    }

    public SettingRobot DisableVpnAcceleratorToggle()
    {
        if (VpnAcceleratorToggle.IsToggled())
        {
            VpnAcceleratorToggle.Toggle();
        }
        return this;
    }

    public SettingRobot EnableNetShieldToggle()
    {
        if (!NetShieldToggle.IsToggled())
        {
            NetShieldToggle.Toggle();
        }
        return this;
    }

    public SettingRobot DisableNetShieldToggle()
    {
        if (NetShieldToggle.IsToggled())
        {
            NetShieldToggle.Toggle();
        }
        return this;
    }

    public SettingRobot EnableKillSwitchToggle()
    {
        if (!KillSwitchToggle.IsToggled())
        {
            KillSwitchToggle.Toggle();
        }
        return this;
    }

    public SettingRobot DisableKillSwitchToggle()
    {
        if (KillSwitchToggle.IsToggled())
        {
            KillSwitchToggle.Toggle();
        }
        return this;
    }

    public SettingRobot EnableAutoLaunchSetting()
    {
        if (!AutoLaunchToggle.IsToggled())
        {
            AutoLaunchToggle.Toggle();
        }
        return this;
    }

    public SettingRobot DisableAutoLaunchSetting()
    {
        if (AutoLaunchToggle.IsToggled())
        {
            AutoLaunchToggle.Toggle();
        }
        return this;
    }

    public SettingRobot SelectAutoLaunchSetting(AutoLaunchOption autoLaunchOption)
    {
        switch (autoLaunchOption)
        {
            case AutoLaunchOption.MinimizeToSystemTray:
                MinimizeToSystemTrayRadioButton.Click();
                break;
            case AutoLaunchOption.OpenOnDesktop:
                OpenOnDesktopRadioButton.Click();
                break;
        }
        return this;
    }

    public SettingRobot EnableAutoConnectionSetting()
    {
        if (!AutoConnectToggle.IsToggled())
        {
            AutoConnectToggle.Toggle();
        }
        return this;
    }

    public SettingRobot DisableAutoConnectionSetting()
    {
        if (AutoConnectToggle.IsToggled())
        {
            AutoConnectToggle.Toggle();
        }
        return this;
    }

    public SettingRobot EnablePortForwarding()
    {
        if (!PortForwardingToggle.IsToggled())
        {
            PortForwardingToggle.Toggle();
        }
        return this;
    }

    public SettingRobot DisablePortForwarding()
    {
        if (PortForwardingToggle.IsToggled())
        {
            PortForwardingToggle.Toggle();
        }
        return this;
    }

    public SettingRobot ClickCopyPortNumber()
    {
        CopyPortNumberButton.Invoke();
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
        Thread.Sleep(TestConstants.UserInputSimulationDelay);
        if (killSwitchMode == KillSwitchMode.Standard)
        {
            KillSwitchStandardRadioButton.Click();
        }
        else if (killSwitchMode == KillSwitchMode.Advanced)
        {
            KillSwitchAdvancedRadioButton.Click();
        }

        return this;
    }

    public SettingRobot ApplySettings()
    {
        Thread.Sleep(TestConstants.UserInputSimulationDelay);
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

    public SettingRobot CloseSettingsUsingEscButton()
    {
        Keyboard.Type(VirtualKeyShort.ESC);
        return this;
    }

    public SettingRobot ClickSupportCenterSettingsCard()
    {
        SupportCenterSettingsCard.ScrollIntoView().Click();
        return this;
    }

    public SettingRobot ClickDebugLogsSettingsCard()
    {
        DebugLogsSettingsCard.ScrollIntoView().Click();
        return this;
    }

    public SettingRobot ClickApplicationLogsSettingsCard()
    {
        ApplicationLogsSettingsCard.Click();
        return this;
    }

    public SettingRobot ClickServiceLogsSettingsCard()
    {
        ServiceLogsSettingsCard.Click();
        return this;
    }

    public SettingRobot PressLearnMore()
    {
        LearnMoreButton.Click();
        return this;
    }

    public SettingRobot SelectLastConnectionOption()
    {
        return SelectDefaultConnectionOption(VpnConnectionOption.Last);
    }

    public SettingRobot SelectFastestConnectionOption()
    {
        return SelectDefaultConnectionOption(VpnConnectionOption.Fastest);
    }

    public SettingRobot SelectDefaultConnectionOption(VpnConnectionOption option)
    {
        Element settingsDefaultConnectionComboBox = SettingsPage.FindDescendant(DefaultConnectionDropdown);
        settingsDefaultConnectionComboBox.Click();
        Thread.Sleep(TestConstants.AnimationDelay);

        string optionName = option switch
        {
            VpnConnectionOption.Fastest => LanguageHelper.GetTranslatedString("Settings_Connection_Default_Fastest"),
            VpnConnectionOption.Random => LanguageHelper.GetTranslatedString("Settings_Connection_Default_Random"),
            VpnConnectionOption.Last => LanguageHelper.GetTranslatedString("Settings_Connection_Default_Last"),
            _ => throw new System.NotImplementedException($"VpnConnectionOption '{option}' is not supported in Settings."),
        };

        Element.ByName(optionName).Click();
        Thread.Sleep(TestConstants.AnimationDelay);

        settingsDefaultConnectionComboBox.ComboBoxSelectedEquals(optionName);

        return this;
    }

    public SettingRobot SelectProfileDefaultConnectionOption(string profileName)
    {
        Element.ByName(profileName).Click();
        return this;
    }

    public SettingRobot SelectLanguage(Language language)
    {
        LanguageComboBox
            .ScrollIntoView().Click()
            .SelectDropdownItem(language.GetFullName());
        return this;
    }

    public class Verifications : SettingRobot
    {
        public Verifications IsReconnectBtnDisplayed()
        {
            ReconnectButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsApplyBtnDisplayed()
        {
            ApplyButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsApplyButtonMissing()
        {
            ApplyButton.DoesNotExist();
            return this;
        }

        public Verifications IsCorrectAccountInfoDisplayed(string accountName, string accountPlan)
        {
            Thread.Sleep(TestConstants.OneSecondTimeout);
            List<string> allChildren = AccountButton.GetAllChildrenNames();
            Assert.That(allChildren, Does.Contain(accountName));
            Assert.That(allChildren, Does.Contain(accountPlan));
            return this;
        }

        public Verifications IsVpnAcceleratorEnabled()
        {
            Assert.That(VpnAcceleratorToggle.IsToggled(), Is.True);
            return this;
        }

        public Verifications IsProTunEnabled()
        {
            Assert.That(ProtonProtocolsToggle.IsToggled(), Is.True);
            return this;
        }

        public Verifications IsProTunDisabled()
        {
            Assert.That(ProtonProtocolsToggle.IsToggled(), Is.False);
            return this;
        }

        public Verifications ProtonProtocolsAreHidden()
        {
            ProTunUdpProtocolRadioButton.DoesNotExist();
            ProTunTcpProtocolRadioButton.DoesNotExist();
            ProTunTlsProtocolRadioButton.DoesNotExist();
            return this;
        }

        public Verifications IsNetshieldBlocking(NetShieldMode netShieldMode)
        {
            DnsHelper.FlushDns();
            CommonAssertions.AssertDnsIsResolved(NETSHIELD_NO_BLOCK);

            switch (netShieldMode)
            {
                case NetShieldMode.BlockMalwareOnly:
                    CommonAssertions.AssertDnsIsNotResolved(NETSHIELD_MALWARE_ENDPOINT);
                    CommonAssertions.AssertDnsIsResolved(NETSHIELD_ADS_ENDPOINT);
                    CommonAssertions.AssertAtLeastOneDomainResolved(_netShieldAdultContentDomains);
                    break;
                case NetShieldMode.BlockAdsMalwareTrackers:
                    CommonAssertions.AssertDnsIsNotResolved(NETSHIELD_MALWARE_ENDPOINT);
                    CommonAssertions.AssertDnsIsNotResolved(NETSHIELD_ADS_ENDPOINT);
                    CommonAssertions.AssertAtLeastOneDomainResolved(_netShieldAdultContentDomains);
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

        public Verifications IsProfileTaglineDisplayed(string profileName)
        {
            string settingsOverriddenByProfileTagline = LanguageHelper.GetTranslatedString("Settings_OverriddenByProfile_Tagline").Replace("{0}", profileName);
            Element.ByName(settingsOverriddenByProfileTagline).WaitUntilDisplayed();
            return this;
        }

        public Verifications IsCorrectProtocolChecked(Protocol protocol)
        {
            Assert.That(GetProtocolRadioButton(protocol).IsChecked(), Is.True);
            return this;
        }

        public Verifications IsNetshieldDisabledStateDisplayed()
        {
            NetShieldSettingsCard.FindChild(Element.ByName(_offStateTranslated)).WaitUntilDisplayed();
            return this;
        }

        public Verifications IsNetshieldEnabledStateDisplayed()
        {
            NetShieldSettingsCard.FindChild(Element.ByName(_onStateTranslated)).WaitUntilDisplayed();
            return this;
        }

        public Verifications IsSplitTunnelingDisabledStateDisplayed()
        {
            SplitTunnelingSettingsCard.FindChild(Element.ByName(_offStateTranslated)).WaitUntilDisplayed();
            return this;
        }

        public Verifications IsSplitTunnelingEnabledStateDisplayed(SplitTunnelingMode splitTunnelingMode)
        {
            SplitTunnelingSettingsCard.FindChild(Element.ByName(splitTunnelingMode.GetEnumValue())).WaitUntilDisplayed();
            return this;
        }

        public Verifications IsPortForwardingDisabledStateDisplayed()
        {
            PortForwardingSettingsCard.FindChild(Element.ByName(_offStateTranslated)).WaitUntilDisplayed();
            return this;
        }

        public Verifications IsPortForwardingEnabledStateDisplayed()
        {
            PortForwardingSettingsCard.FindChild(Element.ByName(_onStateTranslated)).WaitUntilDisplayed();
            return this;
        }

        public Verifications IsKillSwitchEnabledStateDisplayed(KillSwitchMode killSwitchMode)
        {
            KillSwitchSettingsCard.FindChild(Element.ByName(killSwitchMode.GetEnumValue())).WaitUntilDisplayed();
            return this;
        }

        public Verifications IsKillSwitchDisabledStateDisplayed()
        {
            KillSwitchSettingsCard.FindChild(Element.ByName(_offStateTranslated)).WaitUntilDisplayed();
            return this;
        }

        public Verifications AreNotificationsEnabled()
        {
            NotificationsToggle.ScrollIntoView();
            Assert.That(NotificationsToggle.IsToggled(), Is.True);
            return this;
        }

        public Verifications AreNotificationsDisabled()
        {
            NotificationsToggle.ScrollIntoView();
            Assert.That(NotificationsToggle.IsToggled(), Is.False);
            return this;
        }

        public Verifications IsNetshieldNotBlocking()
        {
            AssertCommonNetShieldDisabledState();
            CommonAssertions.AssertDnsIsResolved(NETSHIELD_MALWARE_ENDPOINT);
            return this;
        }

        public Verifications IsFreeUserNetShieldState()
        {
            AssertCommonNetShieldDisabledState();
            CommonAssertions.AssertDnsIsNotResolved(NETSHIELD_MALWARE_ENDPOINT);
            return this;
        }

        private void AssertCommonNetShieldDisabledState()
        {
            DnsHelper.FlushDns();
            CommonAssertions.AssertDnsIsResolved(NETSHIELD_NO_BLOCK);
            CommonAssertions.AssertDnsIsResolved(NETSHIELD_ADS_ENDPOINT);
            CommonAssertions.AssertAtLeastOneDomainResolved(_netShieldAdultContentDomains);
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
            Assert.That(AutoConnectToggle.IsToggled(), Is.True);
            return this;
        }

        public Verifications IsExcludedLocationDisplayed(Country countryName)
        {
            SettingsPage.FindDescendant(Element.ByName(countryName.GetName())).WaitUntilExists();
            return this;
        }

        public Verifications IsExcludedLocationNotDisplayed(Country countryName)
        {
            SettingsPage.FindDescendant(Element.ByName(countryName.GetName())).DoesNotExist();
            return this;
        }

        public Verifications IsRemoveExcludedLocationButtonDisplayed()
        {
            RemoveExcludedLocationButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsNoLocationsAvailableDisplayed()
        {
            NoLocationsAvailable.WaitUntilDisplayed();
            return this;
        }
    }

    private Element GetProtocolRadioButton(Protocol protocol)
    {
        switch (protocol)
        {
            case Protocol.Smart:
                return SmartProtocolRadioButton;
            case Protocol.WireGuardUdp:
                return WireGuardUdpProtocolRadioButton;
            case Protocol.ProTunUdp:
                return ProTunUdpProtocolRadioButton;
            case Protocol.OpenVpnUdp:
                return OpenVpnUdpProtocolRadioButton;
            case Protocol.WireGuardTcp:
                return WireGuardTcpProtocolRadioButton;
            case Protocol.WireGuardTls:
                return WireGuardTlsProtocolRadioButton;
            case Protocol.ProTunTcp:
                return ProTunTcpProtocolRadioButton.ScrollIntoView();
            case Protocol.ProTunTls:
                return ProTunTlsProtocolRadioButton.ScrollIntoView();
            case Protocol.OpenVpnTcp:
                return OpenVpnTcpProtocolRadioButton.ScrollIntoView();
            default:
                throw new ArgumentException($"Unknown protocol: {protocol}");
        }
    }

    public Verifications Verify => new();
}