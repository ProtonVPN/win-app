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

using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Robots.Settings;

public partial class SettingsRobot
{
    public SettingsRobot DoNavigateToProtocolSettingsPage()
    {
        ProtocolSettingsCard.Click();

        // Navigation triggers an animation that may occasionnally fails the tests. Wait for animation to complete before moving on.
        this.Wait(TestConstants.DefaultAnimationDelay);

        return this;
    }

    public SettingsRobot DoSelectWireGuardProtocol()
    {
        WireGuardProtocolRadioButton.Click();
        return this;
    }

    public SettingsRobot DoRestoreSettings()
    {
        RestoreDefaultSettingsButton.Invoke();
        return this;
    }
}