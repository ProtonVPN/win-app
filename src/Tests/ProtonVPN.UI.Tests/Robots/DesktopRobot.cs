/*
 * Copyright (c) 2025 Proton AG
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
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.UiTools;

namespace ProtonVPN.UI.Tests.Robots;

public class DesktopRobot
{
    private readonly UIA3Automation _automation = new();

    public DesktopRobot HoverOverPortForwarding()
    {
        AutomationElement desktop = _automation.GetDesktop();
        AutomationElement? portForwardingButton = desktop.FindFirstDescendant(cf => cf.ByAutomationId("PortForwardingWidgetButton"));
        Assert.That(portForwardingButton, Is.Not.Null, "Port Forwarding button not found");

        portForwardingButton?.HoverSmart();

        return this;
    }

    public DesktopRobot ClickHoverCopyPort()
    {
        AutomationElement desktop = _automation.GetDesktop();
        AutomationElement? copyPortButton = desktop.FindFirstDescendant(cf => cf.ByAutomationId("CopyPortNumberCompactButton"));
        Assert.That(copyPortButton, Is.Not.Null, "Copy Port button not found");
        copyPortButton?.Click();
        return this;
    }

    public class Verifications : DesktopRobot
    {
        public Verifications IsDisplayed(TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromSeconds(8);
            bool visible = ToastCapture.WaitForToastVisible(_automation, timeout.Value);
            Assert.That(visible, Is.True, "Toast notification was not found.");
            return this;
        }

        public Verifications PortMatchesUI(int uiPort, TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromSeconds(6);
            int toastPort = ToastCapture.GetPortFromVisibleToast(_automation, timeout);
            Assert.That(toastPort, Is.EqualTo(uiPort),
                $"Port in toast ({toastPort}) does not match port in UI ({uiPort}).");
            return this;
        }

        public Verifications ClickCopyMatchesUI(int uiPort, TimeSpan? timeout = null)
        {
            int copied = ToastCapture.ClickToastCopyAndGetPort(_automation, timeout);
            Assert.That(copied, Is.EqualTo(uiPort),
                $"Copied port from toast ({copied}) does not match port in UI ({uiPort}).");
            return this;
        }
    }

    public Verifications Verify => new();
}