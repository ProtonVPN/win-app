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
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.Enums.Locations;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
public class SmartRoutingTests : FreshSessionSetUp
{
    private const Country SMART_ROUTING_COUNTRY = Country.Afghanistan;

    private static readonly string _smartRoutingTooltipText = LanguageHelper.GetTranslatedString("Countries_SmartRouting_RoutedThrough").Replace("{0}", "");

    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    [Property("TestCaseId", "890310")]
    public void SmartRoutingInfoInServerList()
    {
        SidebarRobot
            .NavigateToAllCountriesTab()
            .ExpandCities(SMART_ROUTING_COUNTRY)
            .ExpandSpecificServerList()
            .Verify.IsSmartRoutingButtonInfoShown()
            .HoverOverSmartRoutingGlobe();
        DesktopRobot
            .Verify.HoverTextContains(_smartRoutingTooltipText);
    }

    [Test]
    [Property("TestCaseId", "890311")]
    public void SmartRoutingLabelInConnectionDetails()
    {
        SidebarRobot
            .NavigateToAllCountriesTab()
            .ConnectToCountry(SMART_ROUTING_COUNTRY);
        HomeRobot
            .Verify.IsConnected()
                   .IsSmartRoutingConnection()
            .HoverOverSmartRoutingTag();
        DesktopRobot
            .Verify.HoverTextContains(_smartRoutingTooltipText);
        HomeRobot
            .ClickSmartRoutingTag();
        SidebarRobot
            .Verify.IsSmartRoutingModalInfoShown()
            .CloseTabInfoModal();
    }

    [Test]
    [Property("TestCaseId", "890312")]
    public void SmartRoutingInfoModalInServerList()
    {
        SidebarRobot
            .NavigateToAllCountriesTab()
            .ExpandCities(SMART_ROUTING_COUNTRY)
            .ExpandSpecificServerList()
            .Verify.IsSmartRoutingButtonInfoShown()
            .ClickSmartRoutingButton()
            .Verify.IsSmartRoutingModalInfoShown()
            .CloseTabInfoModal();
    }
}