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

namespace ProtonVPN.UI.Tests.Robots.Upsell.Carousel;

public partial class UpsellCarouselRobot : UIActions
{
    protected AutomationElement UpsellCarouselWindowTitle => ElementByAutomationId("WindowTitleLabel").AsLabel();
    protected Button MoveToPreviousUpsellFeatureButton => ElementByAutomationId("MoveToPreviousUpsellFeatureButton").AsButton();
    protected Button MoveToNextUpsellFeatureButton => ElementByAutomationId("MoveToNextUpsellFeatureButton").AsButton();
    protected Button UpgradeButton => ElementByAutomationId("UpsellUpgradeButton").AsButton();
}