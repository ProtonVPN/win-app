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

using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Tools;
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestsHelper;
using System;
using System.Runtime.InteropServices;

namespace ProtonVPN.UI.Tests.UiTools;

public static class UiActions
{
    public static T Click<T>(this T desiredElement) where T : Element
    {
        AutomationElement elementToClick = WaitUntilExists(desiredElement);
        elementToClick.Click();
        return desiredElement;
    }

    public static T Invoke<T>(this T desiredElement) where T : Element
    {
        AutomationElement elementToClick = WaitUntilExists(desiredElement);
        elementToClick.AsButton().Invoke();
        return desiredElement;
    }

    public static T SetText<T>(this T desiredElement, string input) where T : Element
    {
        AutomationElement elementToClick = WaitUntilExists(desiredElement);
        elementToClick.AsTextBox().Text = input;
        return desiredElement;
    }

    public static T ClickItem<T>(this T desiredElement, int index) where T : Element
    {
        AutomationElement element = WaitUntilExists(desiredElement);
        element.FindChildAt(index).Click();
        return desiredElement;
    }

    public static AutomationElement WaitUntilExists<T>(this T desiredElement, TimeSpan? time = null) where T : Element
    {
        if (time == null)
        { 
            time = TestConstants.DefaultElementWaitingTime; 
        }

        AutomationElement elementToWaitFor = null;
        WaitForElement(() =>
        {
            BaseTest.RefreshWindow();
            elementToWaitFor = FindFirstDescendantUsingChildren(desiredElement.Condition);
            return elementToWaitFor != null;
        }, time, desiredElement.SelectorValue);

        return elementToWaitFor;
    }

    public static AutomationElement WaitUntilDisplayed<T>(this T desiredElement, TimeSpan? time = null) where T : Element
    {
        if (time == null)
        { 
            time = TestConstants.DefaultElementWaitingTime; 
        }

        AutomationElement elementToWaitFor = null;
        WaitForElement(() =>
        {
            BaseTest.RefreshWindow();
            elementToWaitFor = FindFirstDescendantUsingChildren(desiredElement.Condition);
            if (elementToWaitFor == null)
            {
                return false;
            }

            return elementToWaitFor.IsOffscreen == false;
        }, time, desiredElement.SelectorValue);

        return elementToWaitFor;
    }

    private static void WaitForElement(Func<bool> function, TimeSpan? time, string selector, string customMessage = null)
    {
        RetryResult<bool> retry = Retry.WhileFalse(
            () =>
            {
                try
                {
                    BaseTest.App.WaitWhileBusy();
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
                Assert.Fail($"Failed to get {selector} element within {time?.TotalSeconds} seconds.");
            }
            else
            {
                Assert.Fail(customMessage);
            }
        }
    }

    private static AutomationElement FindFirstDescendantUsingChildren(Func<ConditionFactory, ConditionBase> conditionFunc)
    {
        AutomationElement child = BaseTest.Window.FindFirstChild(conditionFunc);
        if (child != null)
        {
            return child;
        }

        AutomationElement[] children = BaseTest.Window.FindAllChildren();
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
}
