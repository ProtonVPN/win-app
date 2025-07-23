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
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.UiTools;
using static ProtonVPN.UI.Tests.TestsHelper.TestConstants;

namespace ProtonVPN.UI.Tests.Robots;

public class HomeRobot
{
    protected Element UnprotectedLabel = Element.ByName("Unprotected");
    protected Element ConnectingLabel = Element.ByName("Connecting");
    protected Element ProtectedLabel = Element.ByName("Protected");
    protected Element CancelConnectionButton = Element.ByName("Cancel");
    protected Element GetStartedButton = Element.ByName("Get started");
    protected Element ConnectionDetailsProtocol = Element.ByAutomationId("ShowProtocolFlyoutButton");
    protected Element ChangeProtocolButton = Element.ByAutomationId("ChangeProtocolFlyoutButton");
    protected Element KebabMenuButton = Element.ByAutomationId("TitleBarMenuButton");
    protected Element HelpButton = Element.ByAutomationId("HelpMenu");
    protected Element KebabMenuSettingsItem = Element.ByAutomationId("KebabMenuSettingsItem");
    protected Element KebabMenuExitItem = Element.ByAutomationId("KebabMenuExitItem");
    protected Element CloseButton = Element.ByAutomationId("Close");

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
    protected Element NotTheCountryWantedLabel = Element.ByName("Not the country you wanted?");
    protected Element UpgradeYourServerLabel = Element.ByName("Upgrade to choose any server.");
    protected Element ServerChangesUpsellLabel = Element.ByName("Get unlimited server changes with VPN Plus.");
    protected Element UpgradeButton = Element.ByName("Upgrade");
    protected Element DefaultConnectionSelectorButton = Element.ByAutomationId("DefaultConnectionSelectorButton");
    protected Element FastestCountryOption = Element.ByName("Fastest country");
    protected Element RandomCountryOption = Element.ByName("Random country");
    protected Element LastConnectionOption = Element.ByName("Last connection");
    protected Element CustomizeOption = Element.ByName("Customize");
    protected Element CustomizeCardConnectionTitleLabel = Element.ByName("Default connection");
    protected Element ProtectedLabelAdvancedKillSwitch = Element.ByName("Advanced kill switch activated");

    public HomeRobot DismissWelcomeModal()
    {
        GetStartedButton.Click();
        return this;
    }

    public HomeRobot ConnectViaConnectionCard()
    {
        ConnectionCardConnectButton.Click();
        return this;
    }

    public HomeRobot CancelConnection()
    {
        ConnectionCardCancelButton.Click();
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
        KebabMenuButton.Click();
        // Remove when VPNWIN-2599 is implemented.
        Thread.Sleep(TestConstants.AnimationDelay);
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
        KebabMenuExitItem.DoubleClick();
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
        CloseButton.Click();
        return this;
    }

    public class Verifications : HomeRobot
    {
        public Verifications IsWelcomeModalDisplayed()
        {
            GetStartedButton.WaitUntilDisplayed(TestConstants.TwoMinutesTimeout);
            return this;
        }

        public Verifications IsDisconnected()
        {
            UnprotectedLabel.WaitUntilDisplayed();
            ConnectionCardConnectButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsAdvancedKillSwitchActivated()
        {
            ProtectedLabelAdvancedKillSwitch.WaitUntilDisplayed();
            ConnectionCardConnectButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsConnecting()
        {
            ConnectingLabel.WaitUntilDisplayed(TestConstants.ThirtySecondsTimeout);
            return this;
        }

        public Verifications IsConnected()
        {
            ProtectedLabel.WaitUntilDisplayed(TestConstants.ThirtySecondsTimeout);
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

        public Verifications DoesConnectionCardTitleEqual(string title)
        {
            ConnectionCardTitle.TextEquals(title);
            return this;
        }

        public Verifications IsProtocolDisplayed(TestConstants.Protocol protocol)
        {
            switch (protocol)
            {
                case TestConstants.Protocol.OpenVpnUdp:
                    ConnectionDetailsProtocol.TextEquals("OpenVPN (UDP)");
                    break;
                case TestConstants.Protocol.OpenVpnTcp:
                    ConnectionDetailsProtocol.TextEquals("OpenVPN (TCP)");
                    break;
                case TestConstants.Protocol.WireGuardTcp:
                    ConnectionDetailsProtocol.TextEquals("WireGuard (TCP)");
                    break;
                case TestConstants.Protocol.WireGuardTls:
                    ConnectionDetailsProtocol.TextEquals("Stealth");
                    break;
                case TestConstants.Protocol.WireGuardUdp:
                    ConnectionDetailsProtocol.TextEquals("WireGuard (UDP)");
                    break;
            }

            return this;
        }

        public HomeRobot SelectVpnConnectionOption(VpnConnectionOptions option)
        {
            DefaultConnectionSelectorButton.Click();
            // This sleep added becuase of animation
            Thread.Sleep(TestConstants.AnimationDelay);

            switch (option)
            {
                case VpnConnectionOptions.Fast:
                    FastestCountryOption.DoubleClick();
                    break;

                case VpnConnectionOptions.Random:
                    RandomCountryOption.DoubleClick();
                    break;

                case VpnConnectionOptions.Last:
                    LastConnectionOption.DoubleClick();
                    break;

                case VpnConnectionOptions.Customized:
                    CustomizeOption.DoubleClick();
                    break;
            }
            return this;
        }

        public Verifications DoesCustomizedCardTitleEqual(string title)
        {
            CustomizeCardConnectionTitleLabel.TextEquals(title);
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
    }

    public Verifications Verify => new();
}