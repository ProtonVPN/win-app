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
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using FlaUI.UIA3;
using FlaUI.Core.Input;
using FlaUI.Core.AutomationElements;
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.UiTools;

namespace ProtonVPN.UI.Tests.Robots;

public class TrayRobot
{
    private static UIA3Automation _automation = new();
    private static AutomationElement Desktop => _automation.GetDesktop();
    private static AutomationElement TaskBar => Desktop.FindFirstChild(cf => cf.ByName("Taskbar"))!;
    private static AutomationElement? VpnIcon => TaskBar!.FindFirstDescendant(cf => cf.ByAutomationId("NotifyItemIcon").And(cf.ByName("Proton VPN")));
    private static AutomationElement? TrayApp => Desktop.FindFirstChild(cf => cf.ByName("Proton VPN (tray)"));
    private static AutomationElement? AppHeader => Desktop.FindFirstDescendant(cf => cf.ByName("AppWindow Custom Title Bar"));

    private static AutomationElement SettingsWindow => Desktop.FindFirstChild(cf => cf.ByName("Settings"))!;
    private static AutomationElement OtherTrayIcons => SettingsWindow.FindFirstDescendant(cf => cf.ByName("Other system tray icons"))!;
    private static AutomationElement ShowMoreButton => OtherTrayIcons?.FindFirstChild(cf => cf.ByName("Show more settings").Or(cf.ByName("Show all settings")))!;
    private static AutomationElement VpnToggle => OtherTrayIcons.FindFirstDescendant(cf => cf.ByName("Proton VPN").And(cf.ByClassName("ToggleSwitch")))!;

    protected Element UsernameTextBox => Element.ByAutomationId("UsernameTextBox");
    protected Element ExitAppButton => Element.ByName("Exit").And(Element.ByClassName("Button"));
    protected Element OpenAppButton => Element.ByName("Open Proton VPN").And(Element.ByClassName("Button"));

    public class TrayAppWindow : IDisposable
    {
        public TrayAppWindow()
        {
            OpenTrayApp();
            Element.Root = TrayApp;
        }

        public void Dispose()
        {
            Element.Root = null;
        }
    }

    public static void OpenTrayApp()
    {
        Thread.Sleep(TestConstants.TwoSecondsTimeout);
        VpnIcon!.Click();
        Thread.Sleep(TestConstants.TwoSecondsTimeout);
    }

    public TrayRobot ClickTaskbar()
    {
        Rectangle rect = TaskBar.BoundingRectangle;
        int x = rect.Left + (rect.Width * 2 / 3);
        int y = rect.Top + rect.Height / 2;
        Mouse.Click(new Point(x, y));
        return this;
    }

    public TrayRobot DoubleClickTrayApp()
    {
        Thread.Sleep(TestConstants.TwoSecondsTimeout);
        VpnIcon!.DoubleClick();
        Thread.Sleep(TestConstants.TwoSecondsTimeout);
        return this;
    }

    public TrayRobot ClickExitAppButton()
    {
        ExitAppButton.Click();
        return this;
    }

    public TrayRobot ClickOpenAppButton()
    {
        OpenAppButton.Click();
        return this;
    }

    private void NavigateToWindowsTraySettingsAndTurnOnProtonVpn()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "ms-settings:taskbar",
            UseShellExecute = true
        });
        Thread.Sleep(TestConstants.TwoSecondsTimeout);
        ShowMoreButton.Patterns.ExpandCollapse.Pattern.Expand();
        Thread.Sleep(TestConstants.AnimationDelay);
        VpnToggle!.AsToggleButton().Toggle();

        SettingsWindow.AsWindow().Close();
    }

    public class Verifications : TrayRobot
    {
        public Verifications IsTrayIconDisplayed()
        {
            if (VpnIcon == null)
            {
                NavigateToWindowsTraySettingsAndTurnOnProtonVpn();
            }

            Assert.That(VpnIcon, Is.Not.Null);
            return this;
        }

        public Verifications IsLoginWindowFocused(bool expected)
        {
            UsernameTextBox.AssertIsFocused(expected);
            return this;
        }

        public Verifications IsHomeFocused(bool expected)
        {
            Assert.That(AppHeader, expected ? Is.Not.Null : Is.Null);
            return this;
        }

        public Verifications IsTrayFocused(bool expected)
        {
            Assert.That(TrayApp, expected ? Is.Not.Null : Is.Null);
            return this;
        }
    }

    public Verifications Verify => new Verifications();
}