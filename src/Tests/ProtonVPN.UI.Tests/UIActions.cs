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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests
{
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

        protected dynamic WaitUntilDisplayedByAutomationId(string automationId, TimeSpan time)
        {
            RetryResult<bool> retry = Retry.WhileTrue(
                () => {
                    RefreshWindow();
                    if (Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId)) == null)
                    {
                        return true;
                    }
                    return Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId)).IsOffscreen;
                },
                time, TestConstants.RetryInterval);

            if (!retry.Success)
            {
                Assert.Fail("Failed to get " + automationId + "element within " + time.Seconds + " seconds.");
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

        protected AutomationElement WaitUntilElementExistsByAutomationIdAndReturnTheElement(string automationId, TimeSpan time)
        {
            AutomationElement element = null;
            RetryResult<AutomationElement> retry = Retry.WhileNull(
                () => {
                    RefreshWindow();
                    element = Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
                    return element;
                },
                time, TestConstants.RetryInterval);

            if (!retry.Success)
            {
                Assert.Fail("Failed to get " + automationId + "element within " + time.Seconds + " seconds.");
            }
            return element;
        }

        protected dynamic WaitUntilElementExistsByClassName(string className, TimeSpan time)
        {
            RetryResult<AutomationElement> retry = Retry.WhileNull(
                () => {
                    RefreshWindow();
                    return Window.FindFirstDescendant(cf => cf.ByClassName(className));
                },
                time, TestConstants.RetryInterval);

            if (!retry.Success)
            {
                Assert.Fail("Failed to get " + className + "element within " + time.Seconds + " seconds.");
            }
            return this;
        }

        protected dynamic WaitUntilTextMatchesByAutomationId(string automationId, TimeSpan time, string text, string timeoutMessage)
        {
            AutomationElement element = null;
            string elementText = null;
            RetryResult<bool> retry = Retry.WhileFalse(
                () => {
                    RefreshWindow();
                    element = Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
                    if(element != null)
                    {
                        elementText = element.AsLabel().Text;
                        return elementText == text;
                    }
                    return false;
                },
                time, TestConstants.RetryInterval);

            if (!retry.Success)
            {
                elementText = "ELEMENT_WAS_NOT_FOUND";
                Assert.Fail($"Expected text: '{text}' does not match {automationId} element text {elementText}");
            }
            return this;
        }

        protected dynamic CheckIfDisplayedByClassName(string className)
        {
            RefreshWindow();
            Assert.IsFalse(Window.FindFirstDescendant(cf => cf.ByClassName(className)).IsOffscreen);
            return this;
        }

        protected dynamic CheckIfDisplayedByName(string name)
        {
            RefreshWindow();
            Assert.IsFalse(Window.FindFirstDescendant(cf => cf.ByName(name)).IsOffscreen);
            return this;
        }

        protected dynamic CheckIfNotDisplayedByName(string name)
        {
            RefreshWindow();
            Assert.IsTrue(Window.FindFirstDescendant(cf => cf.ByName(name)).IsOffscreen);
            return this;
        }

        protected dynamic CheckIfNotDisplayedByAutomationId(string automationId)
        {
            RefreshWindow();
            Assert.IsTrue(Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId)).IsOffscreen);
            return this;
        }

        protected dynamic CheckIfDoesNotExistsByAutomationId(string automationId)
        {
            Assert.IsNull(Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId)));
            return this;
        }

        protected dynamic CheckIfDoesNotExistsByName(string name)
        {
            Assert.IsNull(Window.FindFirstDescendant(cf => cf.ByName(name)));
            return this;
        }

        protected dynamic MoveMouseToElement(AutomationElement element, int offsetX = 0, int offsetY = 0)
        {
            Mouse.MovePixelsPerMillisecond = 100;
            Mouse.MoveTo(element.GetClickablePoint().X + offsetX, element.GetClickablePoint().Y + offsetY);
            return this;
        }

        protected AutomationElement ElementByAutomationId(string automationId)
        {
            WaitUntilElementExistsByAutomationId(automationId, TestConstants.VeryShortTimeout);
            return Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
        }

        protected AutomationElement ElementByClassName(string className)
        {
            WaitUntilElementExistsByClassName(className, TestConstants.VeryShortTimeout);
            return Window.FindFirstDescendant(cf => cf.ByClassName(className));
        }

        protected AutomationElement ElementByName(string name)
        {
            WaitUntilElementExistsByName(name, TestConstants.VeryShortTimeout);
            return Window.FindFirstDescendant(cf => cf.ByName(name));
        }

        protected AutomationElement FirstVisibleElementByName(string name)
        {
            AutomationElement element = null;
            AutomationElement[] connectButtons = Window.FindAllDescendants(cf => cf.ByName(name));
            foreach (AutomationElement connectButton in connectButtons)
            {
                if (!connectButton.IsOffscreen)
                {
                    element = connectButton;
                    break;
                }
            };
            return element;
        }
    }
}
