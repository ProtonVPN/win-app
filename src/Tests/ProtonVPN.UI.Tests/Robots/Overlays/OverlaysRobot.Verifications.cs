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
using NUnit.Framework;

namespace ProtonVPN.UI.Tests.Robots.Overlays;

public partial class OverlaysRobot
{
    public OverlaysRobot VerifyOverlayIsOpened(string expectedTitle, bool hasHyperlink)
    {
        Assert.IsNotNull(CloseOverlayButton);

        Window overlay = OverlayWindow;

        Assert.IsNotNull(overlay);

        if (!string.IsNullOrEmpty(expectedTitle))
        {
            Assert.AreEqual(expectedTitle, overlay.Name);
        }

        if (hasHyperlink)
        {
            Assert.IsNotNull(LearnMoreHyperlink);
        }

        return this;
    }

    public OverlaysRobot VerifyProtocolOverlaySettingsCard(string expectedProtocol)
    {
        Button settingsCard = ProtocolSettingsCard;

        Assert.IsNotNull(settingsCard);

        Assert.IsNotNull(settingsCard.FindFirstChild(cf => cf.ByText(expectedProtocol)));

        return this;
    }

    public OverlaysRobot VerifyChangeServerCountdownProgressRing()
    {
        Assert.IsNotNull(ChangeServerCountdownProgressRing);

        return this;
    }
}