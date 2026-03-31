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
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using FlaUI.Core.Definitions;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Input;
using FlaUI.Core.Patterns;
using FlaUI.Core.Tools;
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.UiTools;

public static class UiActions
{
    public static T Click<T>(this T desiredElement, TimeSpan? retryIntervalOverload = null) where T : Element
    {
        AutomationElement? elementToClick = WaitUntilExists(desiredElement, TestConstants.EighteenSecondsTimeout, retryIntervalOverload);
        elementToClick?.WaitUntilClickable(TestConstants.EighteenSecondsTimeout);
        elementToClick?.Click();
        return desiredElement;
    }

    public static T DoubleClick<T>(this T desiredElement) where T : Element
    {
        AutomationElement? elementToClick = WaitUntilExists(desiredElement);
        elementToClick?.WaitUntilClickable(TestConstants.TenSecondsTimeout);
        elementToClick?.DoubleClick();
        return desiredElement;
    }

    public static T RightClick<T>(this T desiredElement) where T : Element
    {
        AutomationElement? elementToClick = WaitUntilExists(desiredElement);
        elementToClick?.WaitUntilClickable(TestConstants.TenSecondsTimeout);
        elementToClick?.RightClick();
        return desiredElement;
    }

    public static T Toggle<T>(this T desiredElement) where T : Element
    {
        AutomationElement? elementToClick = WaitUntilExists(desiredElement);
        elementToClick?.WaitUntilClickable(TestConstants.TenSecondsTimeout);
        elementToClick?.AsToggleButton().Toggle();
        return desiredElement;
    }

    public static bool IsToggled<T>(this T toggleElement) where T : Element
    {
        AutomationElement? elementToCheck = WaitUntilExists(toggleElement);
        elementToCheck.WaitUntilClickable(TestConstants.TenSecondsTimeout);

        ToggleButton? toggleButton = elementToCheck?.AsToggleButton();
        return toggleButton?.ToggleState == ToggleState.On;
    }

    public static T ExpandItem<T>(this T desiredElement) where T : Element
    {
        AutomationElement? element = WaitUntilExists(desiredElement);
        element?.Patterns.ExpandCollapse.Pattern.Expand();
        return desiredElement;
    }

    public static T Invoke<T>(this T desiredElement, TimeSpan? clickableTimeout = null) where T : Element
    {
        clickableTimeout ??= TestConstants.DefaultElementWaitingTime;
        AutomationElement? elementToClick = WaitUntilExists(desiredElement);
        elementToClick?.WaitUntilClickable(clickableTimeout);
        elementToClick?.AsButton().Invoke();
        return desiredElement;
    }

    public static T Hover<T>(this T desiredElement) where T : Element
    {
        AutomationElement? elementToHover = WaitUntilExists(desiredElement, TestConstants.EighteenSecondsTimeout);
        elementToHover?.WaitUntilClickable(TestConstants.EighteenSecondsTimeout);
        elementToHover?.HoverSmart();
        return desiredElement;
    }

    public static T TextBoxEquals<T>(this T desiredElement, string text) where T : Element
    {
        AutomationElement? element = WaitUntilExists(desiredElement);
        string? elementText = element?.AsTextBox().Text;
        Assert.That(elementText?.Equals(text), Is.True, $"Expected string: {text} But was: {elementText}");
        return desiredElement;
    }

    public static string? GetAutomationElementName<T>(this T desiredElement) where T : Element
    {
        AutomationElement? element = WaitUntilExists(desiredElement);
        return element?.Name;
    }

    public static T SetText<T>(this T desiredElement, string input) where T : Element
    {
        AutomationElement? elementToClick = WaitUntilExists(desiredElement);
        if (elementToClick != null)
        {
            elementToClick.AsTextBox().Text = input;
        }

        return desiredElement;
    }

    public static T SelectCheckboxByText<T>(this T desiredElement, string itemToSelect) where T : Element
    {
        foreach (AutomationElement checkbox in desiredElement.GetDescendantsByControlType(ControlType.CheckBox))
        {
            AutomationElement? textBlock = checkbox.FindFirstChild(cf => cf.ByControlType(ControlType.Text));
            if (textBlock?.Name == itemToSelect)
            {
                Thread.Sleep(500);
                checkbox.Click();
                break;
            }
        }
        return desiredElement;
    }

    public static List<string> GetAllCheckboxNames<T>(this T desiredElement) where T : Element
    {
        return desiredElement.GetDescendantsByControlType(ControlType.CheckBox)
           .Select(element => element.FindFirstChild(cf => cf.ByControlType(ControlType.Text))!.Name)
           .Where(name => !string.IsNullOrEmpty(name))
           .ToList() ?? [];
    }

    public static List<string> GetAllChildrenNames<T>(this T desiredElement) where T : Element
    {
        return desiredElement.GetDescendantsByControlType(ControlType.Text)
            .Select(element => element.Name)
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList() ?? [];
    }

    public static T SelectDropdownItem<T>(this T desiredElement, string itemToSelect) where T : Element
    {
        AutomationElement? element = WaitUntilExists(desiredElement);
        ComboBoxItem[]? items = element.AsComboBox()?.Items;

        if (items == null || items.Length == 0)
        {
            throw new Exception($"Dropdown '{desiredElement}' has no items.");
        }

        foreach (ComboBoxItem item in items)
        {
            AutomationElement? textBlock = item.FindFirstChild(cf => cf.ByControlType(ControlType.Text));
            if (textBlock?.Name == itemToSelect)
            {
                item.Click();
                return desiredElement;
            }
        }

        throw new Exception($"Item '{itemToSelect}' was not found in dropdown '{desiredElement}'.");
    }

    public static T ClickItem<T>(this T desiredElement, int index) where T : Element
    {
        AutomationElement? element = WaitUntilExists(desiredElement);
        element?.FindChildAt(index)?.Click();
        return desiredElement;
    }

    public static T FindChild<T>(this T desiredElement, Element childSelector) where T : Element
    {
        desiredElement.ChildElement = childSelector;
        return desiredElement;
    }
    public static T And<T>(this T desiredElement, Element otherSelector) where T : Element
    {
        Func<ConditionFactory, ConditionBase> originalCondition = desiredElement.Condition;
        desiredElement.Condition = cf => originalCondition(cf).And(otherSelector.Condition(cf));
        desiredElement.SelectorName += $" AND {otherSelector.SelectorName}";
        return desiredElement;
    }

    public static T FindDescendant<T>(this T desiredElement, Element descendantSelector) where T : Element
    {
        desiredElement.ChildElement = descendantSelector;
        desiredElement.ChildElement.UseDescendantSearch = true;
        return desiredElement;
    }

    public static T ClearInput<T>(this T desiredElement) where T : Element
    {
        AutomationElement? element = WaitUntilExists(desiredElement);
        if (element != null)
        {
            element.AsTextBox().Text = "";
        }

        return desiredElement;
    }

    public static T ClearSearch<T>(this T desiredElement) where T : Element
    {
        AutomationElement? element = WaitUntilExists(desiredElement);

        Rectangle rect = element!.Parent.BoundingRectangle;
        int x = rect.Right - (rect.Height / 2);
        int y = rect.Top + rect.Height / 2;

        Mouse.Click(new Point(x, y));

        return desiredElement;
    }

    public static AutomationElement[] FindAllElements<T>(this T desiredElement) where T : Element
    {
        WaitUntilExists(desiredElement);
        return BaseTest.Window?.FindAllDescendants(desiredElement.Condition) ?? [];
    }

    public static Element ScrollIntoView<T>(this T desiredElement) where T : Element
    {
        AutomationElement? element = WaitUntilExists(desiredElement);
        element?.Patterns.ScrollItem.Pattern.ScrollIntoView();
        return desiredElement;
    }

    public static Element TextEquals<T>(this T desiredElement, string text) where T : Element
    {
        AutomationElement? element = WaitUntilExists(desiredElement);
        string? elementText = element?.AsLabel().Text;
        Assert.That(elementText?.Equals(text), Is.True, $"Expected string: {text} But was: {elementText}");
        return desiredElement;
    }

    public static Element ValueEquals<T>(this T desiredElement, string value) where T : Element
    {
        AutomationElement? element = WaitUntilExists(desiredElement);
        string? elementValue = element?.Patterns.Value.Pattern.Value;
        Assert.That(elementValue?.Equals(value), Is.True, $"Expected value: {value} But was: {elementValue}");
        return desiredElement;
    }

    public static Element ComboBoxSelectedEquals<T>(this T desiredElement, string expectedValue) where T : Element
    {
        AutomationElement? element = WaitUntilExists(desiredElement);

        ISelectionPattern? selectionPattern = element?.Patterns.Selection.PatternOrDefault;
        if (selectionPattern != null)
        {
            AutomationElement[]? selectedItems = selectionPattern.Selection.Value;
            if (selectedItems != null && selectedItems.Length > 0)
            {
                AutomationElement selectedItem = selectedItems[0];

                AutomationElement? textBlock = selectedItem.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text));

                if (textBlock != null)
                {
                    string? displayText = textBlock.Name;
                    Assert.That(displayText?.Equals(expectedValue), Is.True, 
                        $"Expected ComboBox selected value: '{expectedValue}' But was: '{displayText}'");
                    return desiredElement;
                }
            }
        }

        Assert.Fail($"Could not verify ComboBox selection. Expected value: '{expectedValue}'");
        return desiredElement;
    }

    public static AutomationElement? WaitUntilExists<T>(this T desiredElement, TimeSpan? time = null, TimeSpan? retryIntervalOverload = null) where T : Element
    {
        return WaitForElement(desiredElement, time, element => element != null, null, retryIntervalOverload);
    }

    public static AutomationElement? WaitUntilDisplayed<T>(this T desiredElement, TimeSpan? time = null, TimeSpan? retryIntervalOverload = null) where T : Element
    {
        return WaitForElement(desiredElement, time, element => element != null && !element.IsOffscreen, null, retryIntervalOverload);
    }

    public static void DoesNotExist<T>(this T desiredElement) where T : Element
    {
        AutomationElement? element = FindFirstDescendantUsingChildren(desiredElement.Condition);
        Assert.That(element, Is.Null, $"Element {desiredElement.SelectorName} was found. But it should not exist.");
    }

    public static void AssertIsFocused<T>(this T desiredElement) where T : Element
    {
        AutomationElement? element = WaitUntilExists(desiredElement);

        Assert.That(element?.Properties.HasKeyboardFocus.Value, Is.True);
    }

    public static void AssertIsToggled<T>(this T desiredElement, bool checkParent = false) where T : Element
    {
        WaitUntilExists(desiredElement);
        AutomationElement? element = FindFirstDescendantUsingChildren(desiredElement.Condition);

        if (checkParent)
        {
            element = element?.Parent;
        }

        Assert.That(element?.AsToggleButton().IsToggled, Is.True, $"Element {desiredElement.SelectorName} was not toggled.");
    }

    private static AutomationElement[] GetDescendantsByControlType<T>(this T desiredElement, ControlType controlType) where T : Element
    {
        AutomationElement? element = WaitUntilExists(desiredElement);
        return element?.FindAllDescendants(cf => cf.ByControlType(controlType)) ?? [];
    }

    private static AutomationElement? WaitForElement<T>(
        T desiredElement,
        TimeSpan? time,
        Func<AutomationElement?, bool> condition,
        string? customMessage = null,
        TimeSpan? retryIntervalOverload = null) where T : Element
    {
        time ??= TestConstants.DefaultElementWaitingTime;
        TimeSpan retryInterval = retryIntervalOverload ?? TestConstants.RetryInterval;

        AutomationElement? elementToWaitFor = null;

        RetryResult<bool> retry = Retry.WhileFalse(
            () =>
            {
                try
                {
                    BaseTest.RefreshWindow();
                    BaseTest.App?.WaitWhileBusy();

                    elementToWaitFor = FindFirstDescendantUsingChildren(desiredElement.Condition);

                    if (desiredElement.ChildElement != null && elementToWaitFor != null)
                    {
                        elementToWaitFor = desiredElement.ChildElement.UseDescendantSearch
                            ? elementToWaitFor.FindFirstDescendant(desiredElement.ChildElement.Condition)
                            : elementToWaitFor.FindFirstChild(desiredElement.ChildElement.Condition);
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
            time, retryInterval);

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

    private static AutomationElement? FindFirstDescendantUsingChildren(Func<ConditionFactory, ConditionBase> conditionFunc)
    {
        AutomationElement? child = BaseTest.Window?.FindFirstChild(conditionFunc);
        if (child != null)
        {
            return child;
        }

        AutomationElement[]? children = BaseTest.Window?.FindAllChildren() ?? [];
        foreach (AutomationElement windowChild in children)
        {
            AutomationElement? descendant = windowChild.FindFirstDescendant(conditionFunc);
            if (descendant != null)
            {
                return descendant;
            }
        }

        return null;
    }

    public static bool IsChecked<T>(this T desiredElement) where T : Element
    {
        AutomationElement? element = WaitUntilExists(desiredElement);
        if (element is null)
        {
            return false;
        }

        RadioButton radioButton = new RadioButton(element.FrameworkAutomationElement);
        return radioButton.IsChecked;
    }

    public static T ClickTabByName<T>(this T desiredElement, string partialName) where T : Element
    {
        AutomationElement? element = WaitUntilExists(desiredElement);

        AutomationElement? tabItem = element?
            .FindAllDescendants(cf => cf.ByControlType(ControlType.TabItem))
            .FirstOrDefault(t => t.Name.Contains(partialName, StringComparison.OrdinalIgnoreCase))
            ?? throw new Exception($"TabItem containing '{partialName}' not found.");

        tabItem.AsTabItem().Select();
        return desiredElement;
    }
}