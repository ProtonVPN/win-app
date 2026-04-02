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

public class AppSelectorRobot
{
    protected Element AppCheckboxParent = Element.ByAutomationId("AppsListView");

    public AppSelectorRobot AddSuggestedApp(string appName)
    {
        AppCheckboxParent.SelectCheckboxByText(appName);
        return this;
    }

    public class Verifications : AppSelectorRobot
    {
        public Verifications IsAppChecked(string appName)
        {
            Element.ByName(appName).AssertIsToggled(true);
            return this;
        }

        public Verifications AssertAppAvailability(string appName, bool shouldBeAvailable)
        {
            List<string> allApps = AppCheckboxParent.GetAllCheckboxNames();
            if (shouldBeAvailable)
            {
                Assert.That(allApps, Does.Contain(appName));
            }
            else
            {
                Assert.That(allApps, Does.Not.Contain(appName));
            }
            return this;
        }
    }

    public Verifications Verify => new Verifications();
}