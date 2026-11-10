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

using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.Enums.Locations;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
[Category("ARM")]
[Category("SMOKE_3")]
public class RecentsTests : FreshSessionSetUp
{
    private const Country COUNTRY_NAME = Country.Austria;
    private static readonly string _fastestCountry = LanguageHelper.GetTranslatedString("Country_Fastest");
    private static readonly string _profileName = DefaultProfile.Gaming.GetEnumValue();

    [SetUp]
    public void SetUp()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test, Order(0)]
    [Property("TestCaseId", "602418")]
    [Retry(3)]
    public void RecentIsAddedToList()
    {
        SidebarRobot
            .NavigateToRecents()
            .Verify.IsNoRecentsLabelDisplayed();

        RecentsFlow.PopulateRecentsListWithFastestConnection();

        SidebarRobot
            .Verify.HasNoRecentsLabel()
                   .IsConnectionOptionDisplayed(_fastestCountry)
                   .IsRecentsCountDisplayed(1);

        RecentsFlow.PopulateRecentsListWithCountry(COUNTRY_NAME);

        SidebarRobot
            .NavigateToRecents()
            .Verify.IsConnectionOptionDisplayed(COUNTRY_NAME.GetName())
                   .IsRecentsCountDisplayed(2);
    }

    [Test, Order(1)]
    [Property("TestCaseId", "602425")]
    [Retry(3)]
    public void ProfilesAreAddedToRecentList()
    {
        RecentsFlow.PopulateRecentsListWithProfile(_profileName);

        SidebarRobot
            .NavigateToRecents()
            .Verify.IsConnectionOptionDisplayed(_profileName)
            .IsRecentsCountDisplayed(1);
    }

    [Test, Order(2)]
    [Property("TestCaseId", "602419")]
    public void RemoveRecentFromList()
    {
        RecentsFlow.PopulateRecentsListWithFastestConnection();
        RecentsFlow.PopulateRecentsListWithCountry(COUNTRY_NAME);

        SidebarRobot
            .NavigateToRecents()
            .ExpandSecondaryActionsForRecents(_fastestCountry)
            .RemoveRecent()
            .Verify.IsConnectionOptionMissing(_fastestCountry)
                   .IsRecentsCountDisplayed(1);
    }

    [Test, Order(3)]
    [Property("TestCaseId", "602420")]
    public void PinRecentFromList()
    {
        RecentsFlow.PopulateRecentsListWithCountry(COUNTRY_NAME);
        RecentsFlow.PopulateRecentsListWithProfile(_profileName);

        SidebarRobot
            .NavigateToRecents()
            .Verify.IsConnectionOptionDisplayed(_profileName)
                   .IsRecentsCountDisplayed(2)
                   .IsPinnedCountMissing()
            .ExpandSecondaryActionsForRecents(_profileName)
            .PinRecent()
            .Verify.IsPinnedCountDisplayed(1)
                   .IsRecentsCountDisplayed(1);
    }

    [Test, Order(4)]
    [Property("TestCaseId", "800922")]
    public void UnpinRecentFromList()
    {
        RecentsFlow.PopulateRecentsListWithCountry(COUNTRY_NAME);
        RecentsFlow.PopulateRecentsListWithProfile(_profileName);

        SidebarRobot
            .NavigateToRecents()
            .Verify.IsConnectionOptionDisplayed(_profileName)
            .ExpandSecondaryActionsForRecents(_profileName)
            .PinRecent()
            .Verify.IsRecentsCountDisplayed(1)
                   .IsPinnedCountDisplayed(1)
            .ExpandSecondaryActionsForRecents(_profileName)
            .UnpinRecent()
            .Verify.IsPinnedCountMissing()
                   .IsRecentsCountDisplayed(2);
    }
}
