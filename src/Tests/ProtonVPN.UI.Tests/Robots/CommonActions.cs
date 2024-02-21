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
using System.Threading;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Robots;

public static class CommonActions
{
    public static void Wait(int delayInMilliseconds)
    {
        Thread.Sleep(delayInMilliseconds);
    }

    public static void Wait(TimeSpan delay)
    {
        Wait((int)delay.TotalMilliseconds);
    }

    public static T Wait<T>(this T robot, int delayInMilliseconds) where T : UIActions
    {
        Wait(delayInMilliseconds);
        return robot;
    }

    public static T Wait<T>(this T robot, TimeSpan delay) where T : UIActions
    {
        return robot.Wait((int)delay.TotalMilliseconds);
    }

    public static T WaitUntilDisplayed<T>(this T robot, TimeSpan time) where T : AutomationElement
    {
        RetryResult<bool> retry = Retry.WhileTrue(
            () =>
            {
                TestSession.RefreshWindow();
                return robot.IsOffscreen;
            },
            time, TestConstants.RetryInterval);

        return robot;
    }

    public static void ScrollIntoView(this AutomationElement element)
    {
        if (element == null)
        {
            return;
        }

        if (element.IsOffscreen && element.Patterns?.ScrollItem?.Pattern != null)
        {
            element.Patterns.ScrollItem.Pattern.ScrollIntoView();

            // Wait until scroll animation has completed.
            // (Focusing or clicking an element during scroll could fail)
            Wait(TestConstants.DefaultAnimationDelay);
        }
    }

    public static void FocusAndClick(this AutomationElement element)
    {
        if (element == null)
        {
            return;
        }

        element.ScrollIntoView();
        element.Focus();
        element.Click();
    }

    public static void FocusAndDoubleClick(this AutomationElement element)
    {
        if (element == null)
        {
            return;
        }

        element.FocusAndClick();
        element.Click();
    }
}