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
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using NUnit.Framework;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests
{
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

        protected dynamic WaitUntilDisplayedByAutomationId(string automationId, TimeSpan time)
        {
            WaitForElement(() =>
            {
                RefreshWindow();
                if (Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId)) == null)
                {
                    return false;
                }
                return !Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId)).IsOffscreen;
            }, time, automationId);
            
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

        protected dynamic WaitUntilElementExistsByClassName(string className, TimeSpan time)
        {
            WaitForElement(() =>
            {
                RefreshWindow();
                return Window.FindFirstDescendant(cf => cf.ByClassName(className)) != null;
            }, time, className);

            return this;
        }

        protected dynamic WaitUntilTextMatchesByAutomationId(string automationId, TimeSpan time, string text, string timeoutMessage)
        {
            string elementText = "ELEMENT_NOT_FOUND";
            WaitForElement(() =>
            {
                RefreshWindow();
                AutomationElement element = Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
                if (element != null)
                {
                    elementText = element.AsLabel().Text;
                    return element.AsLabel().Text == text;
                }
                return false;
            }, time, automationId, $"Expected text: '{text}' does not match {automationId} element text {elementText}");

            return this;
        }

        protected AutomationElement WaitUntilElementExistsByAutomationIdAndReturnTheElement(string automationId, TimeSpan time)
        {
            AutomationElement element = null;
            WaitForElement(() =>
            {
                RefreshWindow();
                element = Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
                return element != null;
            }, time, automationId);

            return element;
        }

        protected dynamic CheckIfDisplayedByClassName(string className)
        {
            RefreshWindow();
            Assert.That(Window.FindFirstDescendant(cf => cf.ByClassName(className)).IsOffscreen, Is.False);
            return this;
        }

        protected dynamic CheckIfDisplayedByName(string name)
        {
            RefreshWindow();
            Assert.That(Window.FindFirstDescendant(cf => cf.ByName(name)).IsOffscreen, Is.False);
            return this;
        }

        protected dynamic CheckIfNotDisplayedByName(string name)
        {
            RefreshWindow();
            Assert.That(Window.FindFirstDescendant(cf => cf.ByName(name)).IsOffscreen, Is.True);
            return this;
        }

        protected dynamic CheckIfNotDisplayedByAutomationId(string automationId)
        {
            RefreshWindow();
            Assert.That(Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId)).IsOffscreen, Is.True);
            return this;
        }

        protected dynamic CheckIfDisplayedByAutomationId(string automationId)
        {
            RefreshWindow();
            Assert.That(Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId)).IsOffscreen, Is.False);
            return this;
        }

        protected dynamic CheckIfDoesNotExistByAutomationId(string automationId)
        {
            Assert.That(Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId)), Is.Null);
            return this;
        }

        protected dynamic CheckIfDoesNotExistByName(string name)
        {
            Assert.That(Window.FindFirstDescendant(cf => cf.ByName(name)), Is.Null);
            return this;
        }

        protected dynamic CheckIfDoesNotExistByClassName(string className)
        {
            Assert.That(Window.FindFirstDescendant(cf => cf.ByClassName(className)), Is.Null);
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
            WaitUntilElementExistsByAutomationId(automationId, timeout ?? TestData.VeryShortTimeout);
            return Window.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
        }

        protected AutomationElement ElementByClassName(string className)
        {
            WaitUntilElementExistsByClassName(className, TestData.VeryShortTimeout);
            return Window.FindFirstDescendant(cf => cf.ByClassName(className));
        }

        protected AutomationElement ElementByName(string name)
        {
            WaitUntilElementExistsByName(name, TestData.VeryShortTimeout);
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
            }
            return element;
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
                time, TestData.RetryInterval);

            if (!retry.Success)
            {
                if(customMessage == null)
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
}
