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
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.UiTools;

namespace ProtonVPN.UI.Tests.Robots;

public class FeaturesRobot
{
    private const string NET_SHIELD_ENABLED_FLYOUT_TEXT_1 = "Ads blocked";
    private const string NET_SHIELD_ENABLED_FLYOUT_TEXT_2 = "Trackers stopped";
    private const string NET_SHIELD_ENABLED_FLYOUT_TEXT_3 = "Data saved";

    private const string DISCONNECTED_KILL_SWITCH_ENABLED_FLYOUT_TEXT_1 = "Advanced kill switch disables your internet to protect your IP address while you're not connected to Proton VPN.";
    private const string DISCONNECTED_KILL_SWITCH_ENABLED_FLYOUT_TEXT_2 = "To get back online, connect to VPN or disable advanced kill switch";

    private const string CONNECTED_KILL_SWITCH_ENABLED_FLYOUT_TEXT_1 = "Blocks all internet access when you're not connected to Proton VPN";
    private const string CONNECTED_KILL_SWITCH_ENABLED_FLYOUT_TEXT_2 = "Advanced kill switch applies even if you intentionally disconnect from VPN";

    private const string SPLIT_TUNNELING_NO_APP_SELECTED_FLYOUT_TEXT = "Select apps";

    private const string PORT_UNAVAILABLE_FLYOUT_TEXT_1 = "Connect to a P2P server to improve torrenting speeds";
    private const string PORT_UNAVAILABLE_FLYOUT_TEXT_2 = "Unavailable";

    protected Element NetShieldWidgetButton = Element.ByAutomationId("NetShieldWidgetButton");
    protected Element KillSwitchWidgetButton = Element.ByAutomationId("KillSwitchWidgetButton");
    protected Element PortForwardingWidgetButton = Element.ByAutomationId("PortForwardingWidgetButton");
    protected Element SplitTunnelingWidgetButton = Element.ByAutomationId("SplitTunnelingWidgetButton");

    protected Element CopyPortNumberFromActivePortSection = Element.ByAutomationId("CopyPortNumberCondensedButton");

    protected Element WidgetFlyout = Element.ByAutomationId("WidgetFlyout");
    protected Element MenuFlyout = Element.ByClassName("MenuFlyout");
    protected Element WidgetFlyoutToggle => WidgetFlyout.FindDescendant(Element.ByClassName("Button"));

    protected Element CopyPortNumberFromFlyoutMenu = Element.ByAutomationId("CopyPortNumberCompactButton");

    public FeaturesRobot HoverOverNetShieldWidget()
    {
        NetShieldWidgetButton.Hover();
        Thread.Sleep(TestConstants.AnimationDelay);
        return this;
    }

    public FeaturesRobot HoverOverKillSwitchWidget()
    {
        KillSwitchWidgetButton.Hover();
        Thread.Sleep(TestConstants.AnimationDelay);
        return this;
    }

    public FeaturesRobot HoverOverPortForwardingWidget()
    {
        PortForwardingWidgetButton.Hover();
        Thread.Sleep(TestConstants.AnimationDelay);
        return this;
    }

    public FeaturesRobot HoverOverSplitTunnelingWidget()
    {
        SplitTunnelingWidgetButton.Hover();
        Thread.Sleep(TestConstants.AnimationDelay);
        return this;
    }

    public FeaturesRobot ClickNetShieldWidget()
    {
        NetShieldWidgetButton.Click();
        return this;
    }

    public FeaturesRobot ClickKillSwitchWidget()
    {
        KillSwitchWidgetButton.Click();
        return this;
    }

    public FeaturesRobot ClickPortForwardingWidget()
    {
        PortForwardingWidgetButton.Click();
        return this;
    }

    public FeaturesRobot ClickSplitTunnelingWidget()
    {
        SplitTunnelingWidgetButton.Click();
        return this;
    }

    public FeaturesRobot ClickCopyPortNumberFromActivePortSection()
    {
        CopyPortNumberFromActivePortSection.Click();
        return this;
    }

    public FeaturesRobot ClickCopyPortNumberFromFlyoutMenu()
    {
        CopyPortNumberFromFlyoutMenu.Click();
        return this;
    }

    public FeaturesRobot EnableNetShield(NetShieldMode netShieldMode)
    {
        string netshieldModeString;

        switch (netShieldMode)
        {
            case NetShieldMode.BlockMalwareOnly:
                netshieldModeString = "Block malware only";
                break;
            case NetShieldMode.BlockAdsMalwareTrackers:
                netshieldModeString = "Block ads, trackers, and malware";
                break;
            case NetShieldMode.BlockAdsMalwareTrackersAdultContent:
                netshieldModeString = "Block ads, trackers, malware, and adult content";
                break;
            default:
                throw new ArgumentException($"Unknown mode: {netShieldMode}");
        }

        ToggleFeature(netshieldModeString);
        return this;
    }

    public FeaturesRobot EnableKillSwitch(KillSwitchMode killSwitchMode)
    {
        ToggleFeature(killSwitchMode.ToString());
        return this;
    }

    public FeaturesRobot EnableSplitTunneling(SplitTunnelingMode splitTunnelingMode)
    {
        ToggleFeature(splitTunnelingMode.ToString());
        return this;
    }

    public FeaturesRobot EnableFeature()
    {
        ToggleFeature(SimpleToggle.On.ToString());
        return this;
    }

    public FeaturesRobot DisableFeature()
    {
        ToggleFeature(SimpleToggle.Off.ToString());
        return this;
    }

    public class Verifications : FeaturesRobot
    {
        public Verifications IsNetShieldTextInFlyoutMenu()
        {
            List<string> allChildren = GetFlyoutChildren();
            Assert.That(allChildren, Does.Contain(NET_SHIELD_ENABLED_FLYOUT_TEXT_1));
            Assert.That(allChildren, Does.Contain(NET_SHIELD_ENABLED_FLYOUT_TEXT_2));
            Assert.That(allChildren, Does.Contain(NET_SHIELD_ENABLED_FLYOUT_TEXT_3));
            return this;
        }

        public Verifications IsAdvancedKillSwitchTextInFlyoutMenu(bool isConnected)
        {
            List<string> allChildren = GetFlyoutChildren();
            Assert.That(allChildren, Does.Contain(KillSwitchMode.Advanced.ToString()));
            Assert.That(allChildren, Does.Contain(isConnected ? CONNECTED_KILL_SWITCH_ENABLED_FLYOUT_TEXT_1 : DISCONNECTED_KILL_SWITCH_ENABLED_FLYOUT_TEXT_1));
            Assert.That(allChildren, Does.Contain(isConnected ? CONNECTED_KILL_SWITCH_ENABLED_FLYOUT_TEXT_2 : DISCONNECTED_KILL_SWITCH_ENABLED_FLYOUT_TEXT_2));
            return this;
        }

        public Verifications IsSplitTunnelingAppAvailableInFlyoutMenu(string splitTunnelingMode)
        {
            List<string> allChildren = GetFlyoutChildren();
            Assert.That(allChildren, Does.Contain(splitTunnelingMode));
            Assert.That(allChildren, Does.Not.Contain(SPLIT_TUNNELING_NO_APP_SELECTED_FLYOUT_TEXT));
            return this;
        }

        public Verifications IsSplitTunnelingAppUnavailableInFlyoutMenu(string splitTunnelingMode)
        {
            List<string> allChildren = GetFlyoutChildren();
            Assert.That(allChildren, Does.Contain(splitTunnelingMode.Replace("1", "0")));
            Assert.That(allChildren, Does.Contain(SPLIT_TUNNELING_NO_APP_SELECTED_FLYOUT_TEXT));
            return this;
        }

        public Verifications IsPortForwardingEnabled()
        {
            CopyPortNumberFromActivePortSection.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsLastChangedTimerDisplayed()
        {
            List<string> allChildren = GetFlyoutChildren();
            Assert.That(allChildren, Has.Some.Match("Last changed: \\d+ second[s]? ago"));
            return this;
        }

        public Verifications IsPortUnavailable()
        {
            List<string> allChildren = GetFlyoutChildren();
            Assert.That(allChildren, Does.Contain(PORT_UNAVAILABLE_FLYOUT_TEXT_1));
            Assert.That(allChildren, Does.Contain(PORT_UNAVAILABLE_FLYOUT_TEXT_2));
            return this;
        }
    }

    private void ToggleFeature(string optionToSelect)
    {
        List<string> allChildren = GetFlyoutChildren();
        if (!allChildren.Contains(optionToSelect))
        {
            WidgetFlyoutToggle.Click();
            Thread.Sleep(TestConstants.UserInputSimulationDelay);
            MenuFlyout.FindDescendant(Element.ByName(optionToSelect)).DoubleClick();
            Thread.Sleep(TestConstants.UserInputSimulationDelay);
        }
    }

    private List<string> GetFlyoutChildren()
    {
        WidgetFlyout.WaitUntilDisplayed();
        return WidgetFlyout.GetAllChildrenNames();
    }
    public Verifications Verify => new();
}