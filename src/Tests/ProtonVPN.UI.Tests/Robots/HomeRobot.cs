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
using FlaUI.Core.Input;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.UiTools;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.Enums.Locations;

namespace ProtonVPN.UI.Tests.Robots;

public class HomeRobot
{
    private static readonly string _yourIpAddressTranslated = LanguageHelper.GetTranslatedString("Home_ConnectionDetails_YourIpAddress");
    private static readonly string _countryTranslated = LanguageHelper.GetTranslatedString("Home_ConnectionDetails_Country");
    private static readonly string _providerTranslated = LanguageHelper.GetTranslatedString("Home_ConnectionDetails_Isp");
    private static readonly string _protectedLabelTranslated = LanguageHelper.GetTranslatedString("Home_ConnectionDetails_Protected");
    private static readonly string _unprotectedLabelTranslated = LanguageHelper.GetTranslatedString("Home_ConnectionDetails_Unprotected");
    private static readonly string _connectingLabelTranslated = LanguageHelper.GetTranslatedString("Home_ConnectionDetails_Connecting");
    private static readonly string _protectedLabelAdvancedKillSwitchTranslated = LanguageHelper.GetTranslatedString("Home_ConnectionDetails_AdvancedKillSwitchActivated");
    private static readonly string _getStartedButtonTranslated = LanguageHelper.GetTranslatedString("Dialogs_Common_GetStarted");
    private static readonly string _notTheCountryWantedLabelTranslated = LanguageHelper.GetTranslatedString("ExcludedLocations_SmartDiscovery_Prompt_Title");
    private static readonly string _upgradeYourServerLabelTranslated = LanguageHelper.GetTranslatedString("UpsellBanner_WrongCountry_Description");
    private static readonly string _serverChangesUpsellLabelTranslated = LanguageHelper.GetTranslatedString("Dialogs_ChangeServer_GetUnlimitedServerChanges");
    private static readonly string _upgradeButtonTranslated = LanguageHelper.GetTranslatedString("Common_Actions_Upgrade");
    private static readonly string _fastestCountryOptionTranslated = LanguageHelper.GetTranslatedString("Country_Fastest");
    private static readonly string _randomCountryOptionTranslated = LanguageHelper.GetTranslatedString("Country_Random");
    private static readonly string _lastConnectionOptionTranslated = LanguageHelper.GetTranslatedString("Settings_Connection_Default_Last");
    private static readonly string _wireGuardConnectionErrorPanelTitleTranslated = LanguageHelper.GetTranslatedString("Connection_Error_ConflictingAdapter_Title");
    private static readonly string _wireGuardConnectionErrorPanelDescriptionTranslated = LanguageHelper.GetTranslatedString("Connection_Error_ConflictingAdapter_Description") + "\r\n- WireGuard Tunnel (wg0)";

    protected Element LocationDetailsIpAddress = Element.ByName(_yourIpAddressTranslated);
    protected Element LocationDetailsCountry = Element.ByName(_countryTranslated);
    protected Element LocationDetailsProvider = Element.ByName(_providerTranslated);

    protected Element ProtectedLabel = Element.ByName(_protectedLabelTranslated);
    protected Element UnprotectedLabel = Element.ByName(_unprotectedLabelTranslated);
    protected Element ConnectingLabel = Element.ByName(_connectingLabelTranslated);
    protected Element ProtectedLabelAdvancedKillSwitch = Element.ByName(_protectedLabelAdvancedKillSwitchTranslated);

    protected Element GetStartedButton = Element.ByName(_getStartedButtonTranslated);
    protected Element GetDealButton = Element.ByAutomationId("UpsellUpgradeButton");

    protected Element NotTheCountryWantedLabel = Element.ByName(_notTheCountryWantedLabelTranslated);
    protected Element UpgradeYourServerLabel = Element.ByName(_upgradeYourServerLabelTranslated);
    protected Element ServerChangesUpsellLabel = Element.ByName(_serverChangesUpsellLabelTranslated);
    protected Element UpgradeButton = Element.ByName(_upgradeButtonTranslated);

    protected Element FastestCountryOption = Element.ByName(_fastestCountryOptionTranslated);
    protected Element RandomCountryOption = Element.ByName(_randomCountryOptionTranslated);
    protected Element LastConnectionOption = Element.ByName(_lastConnectionOptionTranslated);

    protected Element ConnectionErrorPanel = Element.ByAutomationId("ConnectionErrorPanel");
    protected Element WireGuardConnectionErrorPanelTitle => ConnectionErrorPanel.FindChild(Element.ByName(_wireGuardConnectionErrorPanelTitleTranslated));
    protected Element WireGuardConnectionErrorPanelDescription => ConnectionErrorPanel.FindChild(Element.ByName(_wireGuardConnectionErrorPanelDescriptionTranslated));
    protected Element ConnectionErrorPanelCloseButton => ConnectionErrorPanel.FindChild(Element.ByAutomationId("CloseButton"));

    protected Element ProtectionTitle = Element.ByAutomationId("ProtectionTitle");
    protected Element LocationDetails = Element.ByAutomationId("LocationDetailsPage");
    protected Element ConnectionDetailsProtocol = Element.ByAutomationId("ShowProtocolFlyoutButton");
    protected Element ChangeProtocolButton = Element.ByAutomationId("ChangeProtocolFlyoutButton");
    protected Element KebabMenuButton = Element.ByAutomationId("TitleBarMenuButton");
    protected Element HelpButton = Element.ByAutomationId("HelpMenu");
    protected Element KebabMenuSettingsItem = Element.ByAutomationId("KebabMenuSettingsItem");
    protected Element KebabMenuExitItem = Element.ByAutomationId("KebabMenuExitItem");

    protected Element ExitButton = Element.ByAutomationId("PrimaryButton");
    protected Element MaximizeClientSizeButton = Element.ByAutomationId("Maximize");
    protected Element RestoreClientSizeButton = Element.ByAutomationId("Restore");
    protected Element CloseClientButton = Element.ByAutomationId("Close");
    protected Element MinimizeClientButton = Element.ByAutomationId("Minimize");

    protected Element ConnectionCardTitle = Element.ByAutomationId("ConnectionCardTitle");
    protected Element ConnectionCardDescription = Element.ByAutomationId("ConnectionCardDescription");
    protected Element ConnectionCardP2PTag = Element.ByAutomationId("ConnectionCardP2PTag");
    protected Element ConnectionCardTorTag = Element.ByAutomationId("ConnectionCardTorTag");
    protected Element ConnectionCardFreeConnectionsTagline = Element.ByAutomationId("ConnectionCardFreeConnectionsTagline");
    protected Element ConnectionCardConnectButton = Element.ByAutomationId("ConnectionCardConnectButton");
    protected Element ConnectionCardCancelButton = Element.ByAutomationId("ConnectionCardCancelButton");
    protected Element ConnectionCardDisconnectButton = Element.ByAutomationId("ConnectionCardDisconnectButton");
    protected Element ConnectionCardChangeServerButton = Element.ByAutomationId("ConnectionCardChangeServerButton");
    protected Element ConnectionCardChangeServerTimeoutButton = Element.ByAutomationId("ConnectionCardChangeServerTimeoutButton");
    protected Element ConnectionCardUpsellBanner = Element.ByAutomationId("ConnectionCardUpsellBanner");

    protected Element ConnectionPreferencesPage => Element.ByAutomationId("ConnectionPreferencesPage");
    protected Element DefaultConnectionSelectorButton => Element.ByAutomationId("DefaultConnectionSelectorButton");
    protected Element DefaultConnectionDropdown => Element.ByAutomationId("DefaultConnectionDropdown");

    protected Element ShowIpFlyoutButton => Element.ByAutomationId("ShowIpFlyoutButton");

    public HomeRobot DismissWelcomeModal()
    {
        Thread.Sleep(TestConstants.AnimationDelay);
        GetStartedButton.ClickUntilElementDisappears();
        Thread.Sleep(TestConstants.AnimationDelay);
        return this;
    }

    public HomeRobot DismissUpsellModal()
    {
        Thread.Sleep(TestConstants.AnimationDelay);
        CloseClientButton.Click();
        Thread.Sleep(TestConstants.AnimationDelay);
        return this;
    }

    public HomeRobot ClickOnConnectionCardTitle()
    {
        ConnectionCardTitle.Click();
        return this;
    }
    public string? GetVpnServerIp()
    {
        return ShowIpFlyoutButton.GetAutomationElementName();
    }

    public HomeRobot ConnectViaConnectionCard()
    {
        ConnectionCardConnectButton.Click();
        return this;
    }

    public HomeRobot CancelConnection(TimeSpan? retryIntervalOverload = null)
    {
        ConnectionCardCancelButton.ClickUntilElementDisappears(retryIntervalOverload);
        return this;
    }

    public HomeRobot Disconnect()
    {
        ConnectionCardDisconnectButton.Click();
        return this;
    }

    public HomeRobot ClickOnProtocolConnectionDetails()
    {
        ConnectionDetailsProtocol.Click();
        return this;
    }

    public HomeRobot ClickChangeProtocolButton()
    {
        ChangeProtocolButton.Click();
        // Remove when VPNWIN-2599 is implemented.
        Thread.Sleep(TestConstants.AnimationDelay);
        return this;
    }

    public HomeRobot ExpandKebabMenuButton()
    {
        KebabMenuButton.ClickUntilAnotherElementAppears(KebabMenuSettingsItem);
        return this;
    }

    public HomeRobot ClickOnHelpButton()
    {
        HelpButton.DoubleClick();
        return this;
    }

    public HomeRobot NavigateToSettingsViaKebabMenu()
    {
        KebabMenuSettingsItem.DoubleClick();
        return this;
    }

    public HomeRobot ExitViaKebabMenu()
    {
        Thread.Sleep(TestConstants.AnimationDelay);
        KebabMenuExitItem.DoubleClick();
        return this;
    }

    public HomeRobot ExitViaKebabMenuWithConfirmation()
    {
        ExitViaKebabMenu();
        ExitButton.ClickUntilElementExits();
        return this;
    }

    public HomeRobot ChangeServer()
    {
        ConnectionCardChangeServerButton.Click();
        return this;
    }

    public HomeRobot ClickLockedChangedServer()
    {
        ConnectionCardChangeServerTimeoutButton.Click();
        return this;
    }

    public HomeRobot CloseClientViaCloseButton()
    {
        CloseClientButton.Click();
        return this;
    }

    public HomeRobot MinimizeClientViaMinimizeButton()
    {
        MinimizeClientButton.Click();
        return this;
    }

    public HomeRobot MaximizeClientSizeViaMaximizeButton()
    {
        MaximizeClientSizeButton.Click();
        return this;
    }

    public HomeRobot RestoreClientSizeViaRestoreButton()
    {
        RestoreClientSizeButton.Click();
        return this;
    }

    public HomeRobot SelectDefaultConnectionOption(VpnConnectionOption option)
    {
        DefaultConnectionSelectorButton.Click();
        Thread.Sleep(TestConstants.AnimationDelay);

        string optionName = option switch
        {
            VpnConnectionOption.Fastest => LanguageHelper.GetTranslatedString("Settings_Connection_Default_Fastest"),
            VpnConnectionOption.Random => LanguageHelper.GetTranslatedString("Settings_Connection_Default_Random"),
            VpnConnectionOption.Last => LanguageHelper.GetTranslatedString("Settings_Connection_Default_Last"),
            _ => throw new NotImplementedException($"VpnConnectionOption '{option}' is not supported on the home page ComboBox."),
        };

        Element.ByName(optionName).Click();
        Thread.Sleep(TestConstants.AnimationDelay);

        return this;
    }

    public HomeRobot SelectDefaultConnectionCountry(Country countryName, string? specificInfo = null)
    {
        DefaultConnectionSelectorButton.Click();
        Thread.Sleep(TestConstants.AnimationDelay);
        DefaultConnectionDropdown.SelectDropdownItem(countryName.GetName(), specificInfo);
        return this;
    }

    public HomeRobot CloseConnectionError()
    {
        Thread.Sleep(TestConstants.TwoSecondsTimeout);
        ConnectionErrorPanelCloseButton.Click();
        return this;
    }

    public class Verifications : HomeRobot
    {
        public Verifications IsLocationDetailsPanelEmpty()
        {
            LocationDetailsIpAddress.SiblingTextEquals("-");
            LocationDetailsCountry.SiblingTextEquals("-");
            LocationDetailsProvider.SiblingTextEquals("-");
            return this;
        }

        public Verifications IsWelcomeModalDisplayed()
        {
            GetStartedButton.WaitUntilDisplayed(TestConstants.TwoMinutesTimeout);
            return this;
        }

        public Verifications IsUpsellModalDisplayed()
        {
            GetDealButton.WaitUntilDisplayed(TestConstants.FiveSecondsTimeout);
            return this;
        }

        public Verifications IsDisconnected(TimeSpan? timeout = null)
        {
            timeout ??= TestConstants.ThirtySecondsTimeout;
            UnprotectedLabel.WaitUntilDisplayed(timeout);
            ConnectionCardConnectButton.WaitUntilDisplayed(timeout);
            return this;
        }

        public Verifications IsWireGuardErrorDisplayed()
        {
            WireGuardConnectionErrorPanelTitle.WaitUntilDisplayed(TestConstants.OneMinuteTimeout);
            WireGuardConnectionErrorPanelDescription.WaitUntilDisplayed(TestConstants.ThirtySecondsTimeout);
            ConnectionErrorPanelCloseButton.WaitUntilDisplayed(TestConstants.ThirtySecondsTimeout);
            return this;
        }

        public Verifications IsAdvancedKillSwitchActivated(bool isExpectedToBeActive = true)
        {
            if (isExpectedToBeActive)
            {
                ProtectedLabelAdvancedKillSwitch.WaitUntilDisplayed();
            }
            else
            {
                ProtectedLabelAdvancedKillSwitch.DoesNotExist();
            }
            ConnectionCardConnectButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications AssertAllVpnConnectionOptions()
        {
            DefaultConnectionSelectorButton.Click();
            Thread.Sleep(TestConstants.AnimationDelay);

            FastestCountryOption.WaitUntilDisplayed();
            RandomCountryOption.WaitUntilDisplayed();
            LastConnectionOption.WaitUntilDisplayed();
            Mouse.Click();
            return this;
        }

        public Verifications IsConnecting()
        {
            ConnectingLabel.WaitUntilDisplayed(TestConstants.OneMinuteTimeout, TestConstants.MoreFrequentRetryInterval);
            return this;
        }

        public Verifications IsConnected()
        {
            ProtectedLabel.WaitUntilDisplayed(TestConstants.TwoMinutesTimeout);
            ConnectionCardDisconnectButton.WaitUntilDisplayed(TestConstants.ThirtySecondsTimeout);
            return this;
        }

        public Verifications IsChangeServerNotLocked()
        {
            ConnectionCardConnectButton.DoesNotExist();
            ConnectionCardChangeServerTimeoutButton.DoesNotExist();
            ConnectionCardChangeServerButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsChangeServerLocked()
        {
            ConnectionCardChangeServerTimeoutButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsNotTheCountryWantedBannerDisplayed()
        {
            NotTheCountryWantedLabel.WaitUntilDisplayed();
            UpgradeYourServerLabel.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsUnlimitedServersChangesUpsellDisplayed()
        {
            ServerChangesUpsellLabel.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsConnectionCardFreeConnectionsTaglineDisplayed()
        {
            ConnectionCardFreeConnectionsTagline.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsP2PConnection()
        {
            ConnectionCardP2PTag.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsTorConnection()
        {
            ConnectionCardTorTag.WaitUntilDisplayed();
            return this;
        }

        public Verifications ConnectionCardTitleEquals(string title)
        {
            ConnectionCardTitle.TextEquals(title);
            return this;
        }

        public Verifications ConnectionCardDescriptionContainsOneOf(Country[] countries)
        {
            ConnectionCardDescription.TextContainsOneOf(countries);
            return this;
        }

        public Verifications ConnectionCardDescriptionDoesNotContainOneOf(Country[] countries)
        {
            ConnectionCardDescription.TextDoesNotContainOneOf(countries);
            return this;
        }

        public Verifications ConnectionCardDescriptionContains(string description)
        {
            ConnectionCardDescription.TextContains(description);
            return this;
        }

        public Verifications ConnectionPreferencesDropdownContains(string[] options, bool isOnConnectionPreferencesPage = false)
        {
            VerifyDropdownOptions(options, shouldContain: true, isOnConnectionPreferencesPage);
            return this;
        }

        public Verifications ConnectionPreferencesDropdownDoesNotContain(string[] options, bool isOnConnectionPreferencesPage = false)
        {
            VerifyDropdownOptions(options, shouldContain: false, isOnConnectionPreferencesPage);
            return this;
        }

        public Verifications ConnectionCardConnectButtonEquals(string buttonName)
        {
            ConnectionCardConnectButton.TextEquals(buttonName);
            return this;
        }

        public Verifications LocationDetailsContains(string locationDetail)
        {
            LocationDetails.WaitUntilDisplayed();
            List<string> allChildren = LocationDetails.GetAllChildrenNames();
            Assert.That(allChildren, Does.Contain(locationDetail));
            return this;
        }

        public Verifications ProtectionStatusEquals(string protectionStatus)
        {
            ProtectionTitle.TextEquals(protectionStatus);
            return this;
        }

        public Verifications IsProtocolDisplayed(Protocol protocol)
        {
            switch (protocol)
            {
                case Protocol.WireGuardUdp:
                    ConnectionDetailsProtocol.TextEquals("WireGuard (UDP)");
                    break;
                case Protocol.ProTunUdp:
                    ConnectionDetailsProtocol.TextEquals("Proton WireGuard (UDP)");
                    break;
                case Protocol.OpenVpnUdp:
                    ConnectionDetailsProtocol.TextEquals("OpenVPN (UDP)");
                    break;
                case Protocol.ProTunTcp:
                    ConnectionDetailsProtocol.TextEquals("Proton WireGuard (TCP)");
                    break;
                case Protocol.ProTunTls:
                    ConnectionDetailsProtocol.TextEquals("Proton Stealth");
                    break;
                case Protocol.WireGuardTcp:
                    ConnectionDetailsProtocol.TextEquals("WireGuard (TCP)");
                    break;
                case Protocol.WireGuardTls:
                    ConnectionDetailsProtocol.TextEquals("Stealth");
                    break;
                case Protocol.OpenVpnTcp:
                    ConnectionDetailsProtocol.TextEquals("OpenVPN (TCP)");
                    break;
            }

            return this;
        }

        public Verifications AssertVPNIpAndExternalIpMatch(string vpnIpAddress, string externalIpAddress)
        {
            Assert.That(vpnIpAddress.Equals(externalIpAddress), Is.True);
            return this;
        }

        public Verifications AssertVpnConnectionEstablished(string ipAddressBefore, string ipAddressAfter)
        {
            Assert.That(ipAddressBefore.Equals(ipAddressAfter), Is.False,
                $"User was not connected to VPN server. IP Address not connected: {ipAddressBefore}. " +
                $"IP Address connected: {ipAddressAfter}");
            return this;
        }

        public Verifications AssertVpnConnectionAfterKill(string ipAddressBeforeKill, string ipAddressAfterKill)
        {
            Assert.That(ipAddressBeforeKill.Equals(ipAddressAfterKill), Is.True,
                $"VPN Connection was lost after app was killed. " +
                $"IP Address before client was killed: {ipAddressBeforeKill}. " +
                $"IP Address after client was killed: {ipAddressAfterKill}");
            return this;
        }

        public Verifications AssertVpnConnectionAfterRestored(string ipAddressBeforeKill, string ipAddressAfterRestore)
        {
            Assert.That(ipAddressBeforeKill.Equals(ipAddressAfterRestore), Is.True,
                $"VPN Connection was lost/reconnected after client was resumed. " +
                $"IP Address before client was killed: {ipAddressBeforeKill}. " +
                $"IP Address after client was restored: {ipAddressAfterRestore}");
            return this;
        }

        private List<string> OpenDropdownAndGetTexts(bool isOnConnectionPreferencesPage)
        {
            if (isOnConnectionPreferencesPage)
            {
                ConnectionPreferencesPage.FindDescendant(DefaultConnectionSelectorButton).BoundingRectangleMouseClick();
            }
            else
            {
                DefaultConnectionSelectorButton.Click();
            }

            Thread.Sleep(TestConstants.AnimationDelay);

            return DefaultConnectionDropdown.GetAllChildrenNames();
        }

        private void VerifyDropdownOptions(string[] options, bool shouldContain, bool isOnConnectionPreferencesPage)
        {
            List<string> dropdownTexts = OpenDropdownAndGetTexts(isOnConnectionPreferencesPage);

            foreach (string optionName in options)
            {
                bool actuallyContains = dropdownTexts.Any(actualText => actualText.Contains(optionName));
                Assert.That(actuallyContains, Is.EqualTo(shouldContain),
                    $"Expected dropdown {(shouldContain ? "to contain" : "to not contain")} text '{optionName}', " +
                    $"but available texts were: [{string.Join(", ", dropdownTexts)}]");
            }

            Thread.Sleep(TestConstants.AnimationDelay);
            ConnectionCardTitle.Click();
        }
    }

    public Verifications Verify => new();
}