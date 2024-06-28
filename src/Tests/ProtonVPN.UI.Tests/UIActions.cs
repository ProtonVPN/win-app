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
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Tools;
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests;

public class UIActions : TestSession
{
    public AutomationElement WaitUntilElementExistsByAutomationId(string automationId, TimeSpan time)
    {
        AutomationElement element = null;
        WaitForElement(() =>
        {
            RefreshWindow();
            element = FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            return element != null;
        }, time, automationId);

        return element;
    }

    public AutomationElement WaitUntilElementExistsByXpath(string xpath, TimeSpan time)
    {
        AutomationElement element = null;
        WaitForElement(() =>
        {
            RefreshWindow();
            element = Window.FindFirstByXPath(xpath);
            return element != null;
        }, time, xpath);

        return element;
    }

    protected AutomationElement WaitUntilElementExistsByName(string name, TimeSpan time)
    {
        AutomationElement element = null;
        WaitForElement(() =>
        {
            RefreshWindow();
            element = FindFirstDescendant(cf => cf.ByName(name));
            return element != null;
        }, time, name);

        return element;
    }

    protected void WaitUntilTextMatches(Func<Label> getLabelFunction, TimeSpan time, string text)
    {
        WaitForElement(() =>
        {
            RefreshWindow();
            return getLabelFunction()?.Text.Equals(text) ?? false;
        }, time, text);
    }

    protected AutomationElement ElementByAutomationId(string automationId, TimeSpan? timeout = null)
    {
        return WaitUntilElementExistsByAutomationId(automationId, timeout ?? TestConstants.FiveSecondsTimeout);;
    }

    protected AutomationElement ElementByXpath(string xpath, TimeSpan? timeout = null)
    {
        return WaitUntilElementExistsByXpath(xpath, timeout ?? TestConstants.FiveSecondsTimeout);
    }

    protected AutomationElement ElementByName(string name, TimeSpan? timeout = null)
    {
        return WaitUntilElementExistsByName(name, timeout ?? TestConstants.FiveSecondsTimeout);
    }

    public AutomationElement FindFirstDescendant(Func<ConditionFactory, ConditionBase> conditionFunc)
    {
        AutomationElement child = Window.FindFirstChild(conditionFunc);
        if (child != null)
        {
            return child;
        }

        AutomationElement[] children = Window.FindAllChildren();
        foreach (AutomationElement windowChild in children)
        {
            AutomationElement descendant = windowChild.FindFirstDescendant(conditionFunc);
            if (descendant != null)
            {
                return descendant;
            }
        }

        return null;
    }

    private void WaitForElement(Func<bool> function, TimeSpan time, string selector, string customMessage = null)
    {
        RetryResult<bool> retry = Retry.WhileFalse(
            () =>
            {
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