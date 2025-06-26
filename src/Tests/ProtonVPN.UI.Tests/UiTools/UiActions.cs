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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.UiTools;

public static class UiActions
{
    public static T Click<T>(this T desiredElement) where T : Element
    {
        AutomationElement elementToClick = WaitUntilExists(desiredElement);
        elementToClick.WaitUntilClickable(TestConstants.TenSecondsTimeout);
        elementToClick.Click();
        return desiredElement;
    }

    public static T DoubleClick<T>(this T desiredElement) where T : Element
    {
        AutomationElement elementToClick = WaitUntilExists(desiredElement);
        elementToClick.WaitUntilClickable(TestConstants.TenSecondsTimeout);
        elementToClick.DoubleClick();
        return desiredElement;
    }

    public static T RightClick<T>(this T desiredElement) where T : Element
    {
        AutomationElement elementToClick = WaitUntilExists(desiredElement);
        elementToClick.WaitUntilClickable(TestConstants.TenSecondsTimeout);
        elementToClick.RightClick();
        return desiredElement;
    }

    public static T Toggle<T>(this T desiredElement) where T : Element
    {
        AutomationElement elementToClick = WaitUntilExists(desiredElement);
        elementToClick.WaitUntilClickable(TestConstants.TenSecondsTimeout);
        elementToClick.AsToggleButton().Toggle();
        return desiredElement;
    }

    public static T MoveMouse<T>(this T desiredElement, int offsetX = 0, int offsetY = 0) where T : Element
    {
        AutomationElement element = WaitUntilExists(desiredElement);
        Mouse.MovePixelsPerMillisecond = 100;
        Point clickablePoint = element.WaitUntilClickable(TestConstants.TenSecondsTimeout).GetClickablePoint();
        Mouse.MoveTo(clickablePoint.X + offsetX, clickablePoint.Y + offsetY);
        return desiredElement;
    }

    public static T ExpandItem<T>(this T desiredElement) where T : Element
    {
        AutomationElement element = WaitUntilExists(desiredElement);
        element.Patterns.ExpandCollapse.Pattern.Expand();
        return desiredElement;
    }

    public static T Invoke<T>(this T desiredElement, TimeSpan? clickableTimeout = null) where T : Element
    {
        clickableTimeout ??= TestConstants.DefaultElementWaitingTime;
        AutomationElement elementToClick = WaitUntilExists(desiredElement);
        elementToClick.WaitUntilClickable(clickableTimeout);
        elementToClick.AsButton().Invoke();
        return desiredElement;
    }

    public static T TextBoxEquals<T>(this T desiredElement, string text) where T : Element
    {
        AutomationElement element = WaitUntilExists(desiredElement);
        string elementText = element.AsTextBox().Text;
        Assert.That(elementText.Equals(text), Is.True, $"Expected string: {text} But was: {elementText}");
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

    public static T FindChild<T>(this T desiredElement, Element childSelector) where T : Element
    {
        desiredElement.ChildElement = childSelector;
        return desiredElement;
    }

    public static T ClearInput<T>(this T desiredElement) where T : Element
    {
        AutomationElement element = WaitUntilExists(desiredElement);
        element.AsTextBox().Text = "";
        return desiredElement;
    }

    public static AutomationElement[] FindAllElements<T>(this T desiredElement) where T : Element
    {
        WaitUntilExists(desiredElement);
        AutomationElement[] elements = BaseTest.Window.FindAllDescendants(desiredElement.Condition);
        return elements;
    }

    public static Element ScrollIntoView<T>(this T desiredElement) where T : Element
    { 
        AutomationElement element = WaitUntilExists(desiredElement);
        element.Patterns.ScrollItem.Pattern.ScrollIntoView();
        return desiredElement;
    }

    public static Element TextEquals<T>(this T desiredElement, string text) where T : Element
    {
        AutomationElement element = WaitUntilExists(desiredElement);
        string elementText = element.AsLabel().Text;
        Assert.That(elementText.Equals(text), Is.True, $"Expected string: {text} But was: {elementText}");
        return desiredElement;
    }

    public static Element ValueEquals<T>(this T desiredElement, string value) where T : Element
    {
        AutomationElement element = WaitUntilExists(desiredElement);
        string elementValue = element.Patterns.Value.Pattern.Value;
        Assert.That(elementValue.Equals(value), Is.True, $"Expected value: {value} But was: {elementValue}");
        return desiredElement;
    }

    public static AutomationElement WaitUntilExists<T>(this T desiredElement, TimeSpan? time = null) where T : Element
    {
        return WaitForElement(desiredElement, time, element => element != null);
    }

    public static AutomationElement WaitUntilDisplayed<T>(this T desiredElement, TimeSpan? time = null) where T : Element
    {
        return WaitForElement(desiredElement, time, element => element != null && !element.IsOffscreen);
    }

    public static void DoesNotExist<T>(this T desiredElement) where T : Element
    {
        AutomationElement element = FindFirstDescendantUsingChildren(desiredElement.Condition);
        Assert.That(element, Is.Null, $"Element {desiredElement.SelectorName} was found. But it should not exist.");
    }

    public static void IsToggled<T>(this T desiredElement) where T : Element
    {
        WaitUntilExists(desiredElement);
        AutomationElement element = FindFirstDescendantUsingChildren(desiredElement.Condition);
        Assert.That(element.AsToggleButton().IsToggled, Is.True, $"Element {desiredElement.SelectorName} was not toggled.");
    }

    private static AutomationElement WaitForElement<T>(
        T desiredElement,
        TimeSpan? time,
        Func<AutomationElement, bool> condition,
        string customMessage = null) where T : Element
    {
        time ??= TestConstants.DefaultElementWaitingTime;
        AutomationElement elementToWaitFor = null;

        RetryResult<bool> retry = Retry.WhileFalse(
            () =>
            {
                try
                {
                    BaseTest.RefreshWindow();
                    BaseTest.App.WaitWhileBusy();

                    elementToWaitFor = FindFirstDescendantUsingChildren(desiredElement.Condition);

                    if (desiredElement.ChildElement != null && elementToWaitFor != null)
                    {
                        elementToWaitFor = elementToWaitFor.FindFirstChild(desiredElement.ChildElement.Condition);
                    }

                    return condition(elementToWaitFor);
                }
                catch (COMException)
                {
                    // Sometimes framework throws these exceptions when searching for elements.
                    // Ignoring it, does not introduce side effects and increases stability.
                    return false;
                }
            },
            time, TestConstants.RetryInterval);

        if (!retry.Success)
        {
            string errorMessage = customMessage ??
                (desiredElement.ChildElement != null
                    ? $"Failed to get child element {desiredElement.ChildElement.SelectorName} inside {desiredElement.SelectorName} element within {time?.TotalSeconds} seconds."
                    : $"Failed to get {desiredElement.SelectorName} element within {time?.TotalSeconds} seconds.");

            Assert.Fail(errorMessage);
        }

        return elementToWaitFor;
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

    public static bool IsChecked<T>(this T desiredElement) where T : Element
    {
        AutomationElement element = WaitUntilExists(desiredElement);
        RadioButton radioButton = new RadioButton(element.FrameworkAutomationElement);
        return radioButton.IsChecked;
    }

    public static T ClickTabByName<T>(this T desiredElement, string partialName) where T : Element
    {
        AutomationElement element = WaitUntilExists(desiredElement);

        AutomationElement tabItem = element
            .FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.TabItem))
            .FirstOrDefault(t => t.Name.Contains(partialName, StringComparison.OrdinalIgnoreCase)) 
            ?? throw new Exception($"TabItem containing '{partialName}' not found.");

        tabItem.AsTabItem().Select();
        return desiredElement;
    }   
}
