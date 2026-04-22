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

using System.Collections.Generic;
using NUnit.Framework;
using ProtonVPN.UI.Tests.UiTools;

namespace ProtonVPN.UI.Tests.Robots;

public class TeachingTipRobot
{
    protected Element TeachingTip => Element.ByAutomationId("TeachingTip");
    protected Element PrimaryActionButton => Element.ByAutomationId("ActionButton");
    protected Element CloseActionButton => Element.ByAutomationId("CloseButton");

    public TeachingTipRobot PrimaryAction()
    {
        PrimaryActionButton.Click();
        return this;
    }

    public TeachingTipRobot CloseAction()
    {
        CloseActionButton.Click();
        return this;
    }

    public class Verifications : TeachingTipRobot
    {
        public Verifications IsTeachingTipDisplayed()
        {
            TeachingTip.WaitUntilDisplayed();
            return this;
        }

        public Verifications TeachingTipTextContains(string text)
        {
            List<string> allChildren = TeachingTip.GetAllChildrenNames();
            Assert.That(allChildren, Does.Contain(text));
            return this;
        }

        public Verifications TeachingTipButtonEquals(string? primary = null, string? close = null)
        {
            if (!string.IsNullOrEmpty(primary))
            {
                PrimaryActionButton.WaitUntilDisplayed();
                PrimaryActionButton.TextEquals(primary);
            }

            if (!string.IsNullOrEmpty(close))
            {
                CloseActionButton.WaitUntilDisplayed();
                CloseActionButton.TextEquals(close);
            }

            return this;
        }
    }

    public Verifications Verify => new();
}
