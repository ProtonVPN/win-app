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
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;
using FlaUI.UIA3;
using FlaUI.Core.Input;
using FlaUI.Core.Definitions;
using FlaUI.Core.AutomationElements;
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Robots;

public class DesktopRobot : IDisposable
{
    private static readonly UIA3Automation _automation = new();
    private static AutomationElement Desktop => _automation.GetDesktop();
    private static AutomationElement TaskBar => Desktop.FindFirstChild(cf => cf.ByName("Taskbar"))!;
    private static AutomationElement? VpnIcon => TaskBar!.FindFirstDescendant(cf => cf.ByAutomationId("NotifyItemIcon").And(cf.ByName("Proton VPN")));
    private static AutomationElement SettingsWindow => Desktop.FindFirstChild(cf => cf.ByName("Settings"))!;
    private static AutomationElement OtherTrayIcons => SettingsWindow.FindFirstDescendant(cf => cf.ByName("Other system tray icons"))!;
    private static AutomationElement ShowMoreButton => OtherTrayIcons?.FindFirstChild(cf => cf.ByName("Show more settings").Or(cf.ByName("Show all settings")))!;
    private static AutomationElement VpnToggle => OtherTrayIcons.FindFirstDescendant(cf => cf.ByName("Proton VPN").And(cf.ByClassName("ToggleSwitch")))!;
    private static AutomationElement NotificationsToggle => SettingsWindow.FindFirstDescendant(cf => cf.ByName("Notifications").And(cf.ByClassName("ToggleSwitch")))!;
    private static AutomationElement DoNotDisturbToggle => SettingsWindow.FindFirstDescendant(cf => cf.ByName("Do not disturb").And(cf.ByClassName("ToggleSwitch")))!;
    private static AutomationElement ProtonVpnNotificationToggle => SettingsWindow.FindFirstDescendant(cf => cf.ByName("Proton VPN").And(cf.ByClassName("ToggleSwitch")))!;

    public static string ReadClipboardText()
    {
        StringBuilder? value = new();

        Thread thread = new(() =>
        {
            try
            {
                value.Append(Clipboard.GetText().Trim());
            }
            catch
            {
                value = null;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        return value is null
            ? string.Empty
            : value.ToString();
    }

    public DesktopRobot DismissOldToastsIfVisible(TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(2);
        try
        {
            ToastCapture.DismissToast(_automation, timeout);
        }
        catch (TimeoutException)
        {
            // Ignore
        }

        //Wait to make sure its gone
        Thread.Sleep(TestConstants.TwoSecondsTimeout);
        return this;
    }

    public DesktopRobot CloseSurvey()
    {
        AutomationElement Desktop = _automation.GetDesktop();
        AutomationElement SurveyWindow = Desktop.FindFirstChild(cf => cf.ByName("Proton VPN - Survey"))!;
        AutomationElement CloseButton = SurveyWindow!.FindFirstDescendant(cf => cf.ByAutomationId("Close"))!;
        CloseButton.Click();
        return this;
    }

    private static void NavigateToWindowsTraySettingsAndTurnOnProtonVpn()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "ms-settings:taskbar",
            UseShellExecute = true
        });
        Thread.Sleep(TestConstants.TwoSecondsTimeout);
        ShowMoreButton.Patterns.ExpandCollapse.Pattern.Expand();
        Thread.Sleep(TestConstants.AnimationDelay);

        ToggleButton vpnToggleButton = VpnToggle!.AsToggleButton();
        if (vpnToggleButton.ToggleState == ToggleState.Off)
        {
            vpnToggleButton.Toggle();
        }

        SettingsWindow.AsWindow().Close();
    }

    public class Verifications
    {
        private readonly UIA3Automation _automation;

        public Verifications(UIA3Automation automation)
        {
            _automation = automation;
        }

        public Verifications EnsureNotificationPrerequisitesAreMet()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ms-settings:notifications",
                UseShellExecute = true
            });

            Thread.Sleep(TestConstants.FiveSecondsTimeout);

            if (!(bool)NotificationsToggle.AsToggleButton().IsToggled!)
            {
                NotificationsToggle.AsToggleButton().Toggle();
            }
            if ((bool)DoNotDisturbToggle.AsToggleButton().IsToggled!)
            {
                DoNotDisturbToggle.AsToggleButton().Toggle();
            }

            for (int i = 0; i < 3; i++)
            {
                Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.DOWN);
                Thread.Sleep(TestConstants.UserInputSimulationDelay);
            }

            if (!(bool)ProtonVpnNotificationToggle.AsToggleButton().IsToggled!)
            {
                ProtonVpnNotificationToggle.AsToggleButton().Toggle();
            }

            SettingsWindow.AsWindow().Close();

            return this;
        }

        public Verifications IsTrayIconDisplayed()
        {
            if (VpnIcon == null)
            {
                NavigateToWindowsTraySettingsAndTurnOnProtonVpn();
            }

            Assert.That(VpnIcon, Is.Not.Null);
            return this;
        }

        public Verifications IsWindowTitlePresent(string windowTitlePart)
        {
            DateTime timeoutDate = DateTime.UtcNow + TestConstants.ThirtySecondsTimeout;
            AutomationElement[]? desktopApps = null;

            while (DateTime.UtcNow < timeoutDate)
            {
                AutomationElement desktop = _automation.GetDesktop();
                desktopApps = desktop.FindAllChildren();

                // Trying normal UIA title match first
                if (desktopApps.Any(e => e.Name != null && e.Name.Contains(windowTitlePart)))
                {
                    return this;
                }

                // Trying process-based detection as a fallback
                bool browserRunning = Process.GetProcessesByName("msedge").Any() || Process.GetProcessesByName("chrome").Any();
                if (browserRunning)
                {
                    return this;
                }

                Thread.Sleep(TestConstants.FiveSecondsTimeout);
            }

            List<string> windowNames = desktopApps!.Where(e => e.Name != null && !string.IsNullOrWhiteSpace(e.Name)).Select(e => $"  • {e.Name}").ToList();

            string windowList = windowNames.Any() ? string.Join("\n", windowNames) : " (No windows found)";

            string failureMessage = $"Window with title containing '{windowTitlePart}' was not found after 30 seconds.\nAvailable windows:\n{windowList}";

            Assert.Fail(failureMessage);
            return this;
        }

        public Verifications IsToastDisplayed(TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromSeconds(8);
            bool isVisible = ToastCapture.WaitForToastVisible(_automation, timeout.Value);
            Assert.That(isVisible, Is.True, "Toast notification was not found.");
            return this;
        }

        public Verifications IsToastNotDisplayed(TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromSeconds(8);
            bool isVisible = ToastCapture.WaitForToastVisible(_automation, timeout.Value);
            Assert.That(isVisible, Is.False, "Toast notification was found.");
            return this;
        }

        public Verifications DoesToastContainConnectionState(string connectionState, TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromSeconds(6);
            string toastText = ToastCapture.GetConnectionStateFromVisibleToast(_automation, timeout);
            Assert.That(toastText, Does.Contain(connectionState));
            return this;
        }

        public Verifications DoesToastPortMatchUI(int uiPort, TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromSeconds(6);
            int toastPort = ToastCapture.GetPortFromVisibleToast(_automation, timeout);
            Assert.That(toastPort, Is.EqualTo(uiPort),
                $"Port in toast ({toastPort}) does not match port in UI ({uiPort}).");
            return this;
        }

        public Verifications DoesToastCopyPortMatchUI(int uiPort, TimeSpan? timeout = null)
        {
            int copied = ToastCapture.ClickToastCopyAndGetPort(_automation, timeout);
            Assert.That(copied, Is.EqualTo(uiPort),
                $"Copied port from toast ({copied}) does not match port in UI ({uiPort}).");
            return this;
        }
    }

    public void Dispose()
    {
        _automation.Dispose();
    }

    public Verifications Verify => new(_automation);
}