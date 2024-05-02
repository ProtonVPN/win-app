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
    protected Label ApplicationTitleLabel => ElementByAutomationId("WindowTitleLabel").AsLabel();
    protected AutomationElement ApplicationIcon => ElementByAutomationId("ApplicationIcon");

    protected TitleBar ApplicationTitleBar => ElementByAutomationId("TitleBar").AsTitleBar();
    protected Button MinimizeButton => ApplicationTitleBar.FindFirstDescendant(c => c.ByName("Minimize")).AsButton();
    protected Button MaximizeButton => ApplicationTitleBar.FindFirstDescendant(c => c.ByName("Maximize")).AsButton();
    protected Button CloseButton => ApplicationTitleBar.FindFirstDescendant(c => c.ByName("Close")).AsButton();

    protected AutomationElement NavigationView => ElementByAutomationId("NavigationView");

    protected Button NavigationHamburgerButton => ElementByAutomationId("TogglePaneButton").AsButton();
    protected Grid NavigationSidebar => ElementByAutomationId("PaneRoot").AsGrid();

    protected Button HomeNavigationViewItem => NavigationView.FindFirstDescendant(c => c.ByAutomationId("Sidebar_Home")).AsButton();
    protected Button GatewaysNavigationViewItem => NavigationView.FindFirstDescendant(c => c.ByAutomationId("Sidebar_Gateways")).AsButton();
    protected Button CountriesNavigationViewItem => NavigationView.FindFirstDescendant(c => c.ByAutomationId("Sidebar_Countries")).AsButton();
    protected Button P2PCountriesNavigationViewItem => NavigationView.FindFirstDescendant(c => c.ByAutomationId("Sidebar_Countries_P2P")).AsButton();
    protected Button SecureCoreCountriesNavigationViewItem => NavigationView.FindFirstDescendant(c => c.ByAutomationId("Sidebar_Countries_SecureCore")).AsButton();
    protected Button TorCountriesNavigationViewItem => NavigationView.FindFirstDescendant(c => c.ByAutomationId("Sidebar_Countries_Tor")).AsButton();
    protected Button SettingsNavigationViewItem => NavigationView.FindFirstDescendant(c => c.ByAutomationId("Sidebar_Settings")).AsButton();
    protected Button KillSwitchFeatureNavigationViewItem => NavigationView.FindFirstDescendant(c => c.ByAutomationId("Sidebar_Features_KillSwitch")).AsButton();
    protected Button NetShieldFeatureNavigationViewItem => NavigationView.FindFirstDescendant(c => c.ByAutomationId("Sidebar_Features_NetShield")).AsButton();
    protected Button PortForwardingFeatureNavigationViewItem => NavigationView.FindFirstDescendant(c => c.ByAutomationId("Sidebar_Features_PortForwarding")).AsButton();
    protected Button SplitTunnelingFeatureNavigationViewItem => NavigationView.FindFirstDescendant(c => c.ByAutomationId("Sidebar_Features_SplitTunneling")).AsButton();

    protected Button AccountButton => NavigationView.FindFirstDescendant(c => c.ByAutomationId("Sidebar_Account")).AsButton();
    protected MenuItem GoToMyAccountButton => ElementByAutomationId("Sidebar_Account_Link").AsMenuItem();
    protected MenuItem SignOutButton => ElementByAutomationId("Sidebar_Account_SignOut").AsMenuItem();
    protected MenuItem ExitButton => ElementByAutomationId("Sidebar_Account_Exit").AsMenuItem();

    protected Label ActivePageTitleLabel => NavigationView.FindFirstChild(c => c.ByControlType(ControlType.Text)).AsLabel();

    protected Button GoBackButton => ElementByAutomationId("GoBackButton").AsButton();

    protected AutomationElement OverlayMessage => ElementByAutomationId("OverlayMessage");
    protected Label OverlayMessageTitle => OverlayMessage.FindFirstDescendant(c => c.ByControlType(ControlType.Text)).AsLabel();
    protected Button OverlayMessagePrimaryButton => OverlayMessage.FindFirstDescendant(c => c.ByAutomationId("PrimaryButton")).AsButton();
    protected Button OverlayMessageSecondaryButton => OverlayMessage.FindFirstDescendant(c => c.ByAutomationId("SecondaryButton")).AsButton();
    protected Button OverlayMessageCloseButton => OverlayMessage.FindFirstDescendant(c => c.ByAutomationId("CloseButton")).AsButton();
}