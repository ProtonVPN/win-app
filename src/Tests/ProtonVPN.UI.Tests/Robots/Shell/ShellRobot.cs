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

using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace ProtonVPN.UI.Tests.Robots.Shell;

public partial class ShellRobot : UIActions
{
    protected Label ApplicationTitleLabel => ElementByAutomationId("ApplicationTitleLabel").AsLabel();
    protected AutomationElement ApplicationIcon => ElementByAutomationId("ApplicationIcon");

    protected Button MinimizeButton => ElementByAutomationId("_MinimizeButton").AsButton();
    protected Button MaximizeButton => ElementByAutomationId("_MaximizeButton").AsButton();
    protected Button CloseButton => ElementByAutomationId("_CloseButton").AsButton();

    protected AutomationElement NavigationView => ElementByAutomationId("NavigationView");

    protected Button NavigationHamburgerButton => ElementByAutomationId("TogglePaneButton").AsButton();
    protected Grid NavigationSideBar => ElementByAutomationId("PaneRoot").AsGrid();

    protected Button HomeNavigationViewItem => NavigationView.FindFirstDescendant(c => c.ByAutomationId("NavigationViewItem").And(c.ByName("Home"))).AsButton();
    protected Button CountriesNavigationViewItem => NavigationView.FindFirstDescendant(c => c.ByAutomationId("NavigationViewItem").And(c.ByName("Countries"))).AsButton();
    protected Button SettingsNavigationViewItem => NavigationView.FindFirstDescendant(c => c.ByAutomationId("NavigationViewItem").And(c.ByName("Settings"))).AsButton();

    protected Button AccountButton => ElementByAutomationId("AccountButton").AsButton();

    protected Label ActivePageTitleLabel => NavigationView.FindFirstChild(c => c.ByControlType(ControlType.Group)).FindFirstChild(c => c.ByControlType(ControlType.Text)).AsLabel();

    protected Button GoBackButton => ElementByAutomationId("GoBackButton").AsButton();
}