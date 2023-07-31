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
        RetryResult<AutomationElement> retry = Retry.WhileNull(
            () => {
                App.WaitWhileBusy();
                RefreshWindow();
                return Window.FindFirstDescendant(cf => cf.ByName(name));
            },
            time, TestConstants.RetryInterval);

        if (!retry.Success)
        {
            Assert.Fail("Failed to get " + name + " element within " + time.Seconds + " seconds.");
        }
        return this;
    }

    public dynamic WaitUntilElementExistsByAutomationId(string automationId, TimeSpan time)
    {
        RetryResult<AutomationElement> retry = Retry.WhileNull(
            () => {
                RefreshWindow();
                return Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            },
            time, TestConstants.RetryInterval);

        if (!retry.Success)
        {
            Assert.Fail("Failed to get " + automationId + "element within " + time.Seconds + " seconds.");
        }
        return this;
    }

    protected dynamic WaitUntilTextMatches(Func<Label> getLabelMethod, TimeSpan time, string text)
    {
        RetryResult<bool> retry = Retry.WhileFalse(() => {
            return getLabelMethod()?.Text.Equals(text) ?? false;
        }, time, TestConstants.RetryInterval);


        if (!retry.Success)
        {
            Assert.Fail($"Expected text: '{text}' does not match: '{getLabelMethod().Text}'.");
        }
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

    protected AutomationElement ElementByName(string name, TimeSpan? timeout = null)
    {
        WaitUntilElementExistsByName(name, timeout ?? TestConstants.VeryShortTimeout);
        return Window.FindFirstDescendant(cf => cf.ByName(name));
    }
}