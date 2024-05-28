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
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Robots.Home;

public partial class HomeRobot : UIActions
{
    protected Label VpnStatusTitleLabel => ElementByAutomationId("VpnStatusTitleLabel", TestConstants.ThirtySecondsTimeout).AsLabel();
    protected Label VpnStatusSubtitleLabel => ElementByAutomationId("VpnStatusSubtitleLabel", TestConstants.OneMinuteTimeout).AsLabel();

    protected Label ConnectionCardHeaderLabel => ElementByAutomationId("ConnectionCardHeaderLabel").AsLabel();
    protected Label ConnectionCardTitleLabel => ElementByAutomationId("ConnectionCardTitleLabel").AsLabel();
    protected Label ConnectionCardSubtitleLabel => ElementByAutomationId("ConnectionCardSubtitleLabel").AsLabel();
    protected Label ConnectionCardFeatureTorLabel => ElementByAutomationId("ConnectionCardFeatureTorLabel").AsLabel();
    protected Label ConnectionCardFeatureP2PLabel => ElementByAutomationId("ConnectionCardFeatureP2PLabel").AsLabel();
    protected Button ConnectionCardConnectButton => ElementByAutomationId("ConnectionCardConnectButton").AsButton();
    protected Button ConnectionCardCancelButton => ElementByAutomationId("ConnectionCardCancelButton").AsButton();
    protected Button ConnectionCardDisconnectButton => ElementByAutomationId("ConnectionCardDisconnectButton").AsButton();
    protected Button ConnectionCardChangeServerButton => ElementByAutomationId("ConnectionCardChangeServerButton").AsButton();
    protected Button ConnectionCardShowConnectionDetailsButton => ElementByAutomationId("ShowConnectionDetailsButton").AsButton();
    
    protected Button ConnectionDetailsCloseButton => ElementByAutomationId("CloseConnectionDetailsButton").AsButton();
    protected Label ConnectionDetailsTitleLabel => ElementByAutomationId("ConnectionDetailsTitle").AsLabel();

    protected Button ConnectionDetailServerLoadButton => ElementByAutomationId("ShowServerLoadOverlayButton").AsButton();
    protected Button ConnectionDetailLatencyButton => ElementByAutomationId("ShowLatencyOverlayButton").AsButton();
    protected Button ConnectionDetailProtocolButton => ElementByAutomationId("ShowProtocolOverlayButton").AsButton();

    protected Button HelpButton => ElementByAutomationId("HelpButton").AsButton();
    protected MenuItem IssueCategoryMenuItem => ElementByAutomationId("IssueCategoryMenuItem").AsMenuItem();

    protected AutomationElement RecentsComponent => ElementByAutomationId("RecentsComponent");
    protected AutomationElement RecentsTab => ElementByName("Recents");
    protected AutomationElement PrimaryActionButton => ElementByAutomationId("PrimaryButton");
    protected AutomationElement SecondaryActionButton => ElementByAutomationId("SecondaryButton");
    protected AutomationElement RemoveButton => ElementByName("Remove");

    protected Button DiscoverVpnPlusButton => ElementByAutomationId("DiscoverVpnPlusButton").AsButton();

    protected AutomationElement GetRecentRow(string countryName = null)
    {
        return RecentsComponent.FindFirstDescendant(cf => cf.ByName(countryName ?? "Fastest country"));
    }

    protected Button GetRecentRowPrimaryActionButton(string countryName = null)
    {
        return GetRecentRow(countryName).FindFirstDescendant(cf => cf.ByAutomationId("PrimaryButton")).AsButton();
    }

    protected Button GetRecentRowSecondaryActionButton(string countryName = null)
    {
        return GetRecentRow(countryName).FindFirstDescendant(cf => cf.ByAutomationId("SecondaryButton")).AsButton();
    }
}