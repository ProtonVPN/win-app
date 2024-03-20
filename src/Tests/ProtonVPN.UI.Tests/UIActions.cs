/*
 * Copyright (c) 2023 Proton AG
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
using System.Runtime.InteropServices;
using System.Xml.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests;

public class UIActions : TestSession
{
    protected dynamic WaitUntilElementExistsByName(string name, TimeSpan time)
    {
        WaitForElement(() =>
        {
            RefreshWindow();
            return Window.FindFirstDescendant(cf => cf.ByName(name)) != null;
        }, time, name);

        return this;
    }

    public dynamic WaitUntilElementExistsByAutomationId(string automationId, TimeSpan time)
    {
        WaitForElement(() =>
        {
            RefreshWindow();
            return Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId)) != null;
        }, time, automationId);

        return this;
    }

    public dynamic WaitUntilElementExistsByXpath(string xpath, TimeSpan time)
    {
        WaitForElement(() =>
        {
            RefreshWindow();
            return Window.FindFirstByXPath(xpath) != null;
        }, time, xpath);

        return this;
    }

    protected dynamic WaitUntilTextMatches(Func<Label> getLabelMethod, TimeSpan time, string text)
    {
        WaitForElement(() =>
        {
            RefreshWindow();
            return getLabelMethod()?.Text.Equals(text) ?? false;
        }, time, text);
        return this;
    }

    protected dynamic MoveMouseToElement(AutomationElement element, int offsetX = 0, int offsetY = 0)
    {
        Mouse.MovePixelsPerMillisecond = 100;
        Mouse.MoveTo(element.GetClickablePoint().X + offsetX, element.GetClickablePoint().Y + offsetY);
        return this;
    }

    protected AutomationElement ElementByAutomationId(string automationId, TimeSpan? timeout = null)
    {
        WaitUntilElementExistsByAutomationId(automationId, timeout ?? TestConstants.VeryShortTimeout);
        return Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
    }

    protected AutomationElement ElementByXpath(string xpath, TimeSpan? timeout = null)
    {
        WaitUntilElementExistsByXpath(xpath, timeout ?? TestConstants.VeryShortTimeout);
        return Window.FindFirstByXPath(xpath);
    }

    protected AutomationElement ElementByName(string name, TimeSpan? timeout = null)
    {
        WaitUntilElementExistsByName(name, timeout ?? TestConstants.VeryShortTimeout);
        return Window.FindFirstDescendant(cf => cf.ByName(name));
    }

    private void WaitForElement(Func<bool> function, TimeSpan time, string selector, string customMessage = null)
    {
        RetryResult<bool> retry = Retry.WhileFalse(
            () => {
                try
                {
                    App.WaitWhileBusy();
                    return function();
                }
                catch (COMException)
                {
                    return false;
                }
            },
            time, TestConstants.RetryInterval);

        if (!retry.Success)
        {
            if (customMessage == null)
            {
                Assert.Fail($"Failed to get {selector} element within {time.TotalSeconds} seconds.");
            }
            else
            {
                Assert.Fail(customMessage);
            }
        }
    }
}