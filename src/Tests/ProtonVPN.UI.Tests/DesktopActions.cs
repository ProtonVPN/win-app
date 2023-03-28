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
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests
{
    internal class DesktopActions
    {
        protected AutomationElement Desktop;

        protected dynamic WaitUntilExistsByName(string name, TimeSpan time)
        {
            RetryResult<AutomationElement> retry = Retry.WhileNull(
                () => {
                    RefreshDesktop();
                    return Desktop.FindFirstDescendant(cf => cf.ByName(name));
                },
                time, TestConstants.RetryInterval);

            if (!retry.Success)
            {
                Assert.Fail("Failed to get " + name + " element within " + time.Seconds + " seconds.");
            }
            return this;
        }

        protected dynamic WaitUntilExistsByAutomationId(string automationId, TimeSpan time)
        {
            RetryResult<AutomationElement> retry = Retry.WhileNull(
                () => {
                    RefreshDesktop();
                    return Desktop.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
                },
                time, TestConstants.RetryInterval);
            if (!retry.Success)
            {
                Assert.Fail("Failed to get " + automationId + " element within " + time.Seconds + " seconds.");
            }
            return this;
        }

        protected dynamic WaitUntilDisplayedByAutomationId(string automationId, TimeSpan time)
        {
            RetryResult<bool> retry = Retry.WhileTrue(
                () => {
                    RefreshDesktop();
                    if (Desktop.FindFirstDescendant(cf => cf.ByAutomationId(automationId)) == null)
                    {
                        return true;
                    }
                    return Desktop.FindFirstDescendant(cf => cf.ByAutomationId(automationId)).IsOffscreen;
                },
                time, TestConstants.RetryInterval);

            if (!retry.Success)
            {
                Assert.Fail("Failed to get " + automationId + "element within " + time.Seconds + " seconds.");
            }
            return this;
        }

        protected dynamic WaitUntilDisplayedByName(string name, TimeSpan time)
        {
            RetryResult<bool> retry = Retry.WhileTrue(
                () => {
                    RefreshDesktop();
                    if (Desktop.FindFirstDescendant(cf => cf.ByName(name)) == null)
                    {
                        return true;
                    }
                    return Desktop.FindFirstDescendant(cf => cf.ByName(name)).IsOffscreen;
                },
                time, TestConstants.RetryInterval);

            if (!retry.Success)
            {
                Assert.Fail("Failed to get " + name + "element within " + time.Seconds + " seconds.");
            }
            return this;
        }

        protected dynamic WaitUntilDisplayedByClass(string className, TimeSpan time)
        {
            RetryResult<AutomationElement> retry = Retry.WhileNull(
                () =>
                {
                    RefreshDesktop();
                    return Desktop.FindFirstDescendant(cf => cf.ByClassName(className));
                },
                time, TestConstants.RetryInterval);

            if (!retry.Success)
            {
                Assert.Fail("Failed to get " + className + "element within " + time.Seconds + " seconds.");
            }
            return this;
        }

        protected AutomationElement ElementByAutomationId(string automationId)
        {
            WaitUntilExistsByAutomationId(automationId, TestConstants.ShortTimeout);
            return Desktop.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
        }

        protected AutomationElement ElementByClassName(string className)
        {
            WaitUntilDisplayedByClass(className, TestConstants.ShortTimeout);
            return Desktop.FindFirstDescendant(cf => cf.ByClassName(className));
        }

        protected AutomationElement ElementByName(string name)
        {
            WaitUntilExistsByName(name, TestConstants.ShortTimeout);
            return Desktop.FindFirstDescendant(cf => cf.ByName(name));
        }

        protected void RefreshDesktop()
        {   
            Desktop = new UIA3Automation().GetDesktop();
        }
    }
}
