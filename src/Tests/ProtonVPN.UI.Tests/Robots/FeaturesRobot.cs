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
using ProtonVPN.UI.Tests.UiTools;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Robots;

public class FeaturesRobot
{
    private static readonly string _connectedKillSwitchEnabledFlyoutTextOne = LanguageHelper.GetTranslatedString("Flyouts_KillSwitch_Advanced_Success");
    private static readonly string _connectedKillSwitchEnabledFlyoutTextTwo = LanguageHelper.GetTranslatedString("Flyouts_KillSwitch_Warning_Connected");

    private static readonly string _disconnectedKillSwitchEnabledFlyoutTextOne = LanguageHelper.GetTranslatedString("Flyouts_KillSwitch_Info_Disconnected");
    private static readonly string _disconnectedKillSwitchEnabledFlyoutTextTwo = LanguageHelper.GetTranslatedString("Flyouts_KillSwitch_Warning_Disconnected");

    private static readonly string _netShieldEnabledFlyoutTextOne = LanguageHelper.GetTranslatedString("Home_NetShield_AdsBlocked_Other");
    private static readonly string _netShieldEnabledFlyoutTextTwo = LanguageHelper.GetTranslatedString("Home_NetShield_TrackersStopped_Other");
    private static readonly string _netShieldEnabledFlyoutTextThree = LanguageHelper.GetTranslatedString("Home_NetShield_DataSaved");

    private static readonly string _splitTunnelingNoAppSelectedFlyoutText = LanguageHelper.GetTranslatedString("Settings_Connection_SplitTunneling_Apps_Select");

    private static readonly string _portUnavailableFlyoutTextOne = LanguageHelper.GetTranslatedString("Flyouts_PortForwarding_Warning");
    private static readonly string _portUnavailableFlyoutTextTwo = LanguageHelper.GetTranslatedString("Settings_Connection_PortForwarding_Unavailable");

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
                netshieldModeString = LanguageHelper.GetTranslatedString("Settings_Connection_NetShield_BlockMalwareOnly");
                break;
            case NetShieldMode.BlockAdsMalwareTrackers:
                netshieldModeString = LanguageHelper.GetTranslatedString("Settings_Connection_NetShield_BlockAdsMalwareTrackers");
                break;
            case NetShieldMode.BlockAdsMalwareTrackersAdultContent:
                netshieldModeString = LanguageHelper.GetTranslatedString("Settings_Connection_NetShield_BlockAdsMalwareTrackersAdultContent");
                break;
            default:
                throw new ArgumentException($"Unknown mode: {netShieldMode}");
        }

        ToggleFeature(netshieldModeString);
        return this;
    }

    public FeaturesRobot EnableKillSwitch(KillSwitchMode killSwitchMode)
    {
        ToggleFeature(killSwitchMode.GetEnumValue());
        return this;
    }

    public FeaturesRobot EnableSplitTunneling(SplitTunnelingMode splitTunnelingMode)
    {
        ToggleFeature(splitTunnelingMode.GetEnumValue());
        return this;
    }

    public FeaturesRobot EnableFeature()
    {
        ToggleFeature(SimpleToggle.On.GetEnumValue());
        return this;
    }

    public FeaturesRobot DisableFeature()
    {
        ToggleFeature(SimpleToggle.Off.GetEnumValue());
        return this;
    }

    public class Verifications : FeaturesRobot
    {
        public Verifications NetShieldFeatureNameEquals(string netShieldText)
        {
            NetShieldWidgetButton.WaitUntilDisplayed();
            List<string> allChildren = NetShieldWidgetButton.GetAllChildrenNames();
            Assert.That(allChildren, Does.Contain(netShieldText));
            return this;
        }

        public Verifications KillSwitchFeatureNameEquals(string killSwitchText)
        {
            KillSwitchWidgetButton.WaitUntilDisplayed();
            List<string> allChildren = KillSwitchWidgetButton.GetAllChildrenNames();
            Assert.That(allChildren, Does.Contain(killSwitchText));
            return this;
        }

        public Verifications PortForwardingFeatureNameEquals(string portForwardingText)
        {
            PortForwardingWidgetButton.WaitUntilDisplayed();
            List<string> allChildren = PortForwardingWidgetButton.GetAllChildrenNames();
            Assert.That(allChildren, Does.Contain(portForwardingText));
            return this;
        }

        public Verifications SplitTunnelingFeatureNameEquals(string splitTunnelingText)
        {
            SplitTunnelingWidgetButton.WaitUntilDisplayed();
            List<string> allChildren = SplitTunnelingWidgetButton.GetAllChildrenNames();
            Assert.That(allChildren, Does.Contain(splitTunnelingText));
            return this;
        }

        public Verifications IsNetShieldTextInFlyoutMenu()
        {
            List<string> allChildren = GetFlyoutChildren();
            Assert.That(allChildren, Does.Contain(_netShieldEnabledFlyoutTextOne));
            Assert.That(allChildren, Does.Contain(_netShieldEnabledFlyoutTextTwo));
            Assert.That(allChildren, Does.Contain(_netShieldEnabledFlyoutTextThree));
            return this;
        }

        public Verifications IsAdvancedKillSwitchTextInFlyoutMenu(bool isConnected)
        {
            List<string> allChildren = GetFlyoutChildren();
            Assert.That(allChildren, Does.Contain(KillSwitchMode.Advanced.GetEnumValue()));
            Assert.That(allChildren, Does.Contain(isConnected ? _connectedKillSwitchEnabledFlyoutTextOne : _disconnectedKillSwitchEnabledFlyoutTextOne));
            Assert.That(allChildren, Does.Contain(isConnected ? _connectedKillSwitchEnabledFlyoutTextTwo : _disconnectedKillSwitchEnabledFlyoutTextTwo));
            return this;
        }

        public Verifications IsSplitTunnelingAppAvailableInFlyoutMenu(string splitTunnelingMode)
        {
            List<string> allChildren = GetFlyoutChildren();
            Assert.That(allChildren, Does.Contain(splitTunnelingMode));
            Assert.That(allChildren, Does.Not.Contain(_splitTunnelingNoAppSelectedFlyoutText));
            return this;
        }

        public Verifications IsSplitTunnelingAppUnavailableInFlyoutMenu(string splitTunnelingMode)
        {
            List<string> allChildren = GetFlyoutChildren();
            Assert.That(allChildren, Does.Contain(splitTunnelingMode.Replace("1", "0")));
            Assert.That(allChildren, Does.Contain(_splitTunnelingNoAppSelectedFlyoutText));
            return this;
        }

        public Verifications IsPortForwardingEnabled()
        {
            CopyPortNumberFromActivePortSection.WaitUntilDisplayed(TestConstants.ThirtySecondsTimeout);
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
            Assert.That(allChildren, Does.Contain(_portUnavailableFlyoutTextOne));
            Assert.That(allChildren, Does.Contain(_portUnavailableFlyoutTextTwo));
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
        Thread.Sleep(TestConstants.TwoSecondsTimeout);
        WidgetFlyout.WaitUntilDisplayed();
        return WidgetFlyout.GetAllChildrenNames();
    }
    public Verifications Verify => new();
}