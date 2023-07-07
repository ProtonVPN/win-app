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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.UI.Tests.Robots.Shell;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Robots.Settings;

public partial class SettingsRobot
{
    public SettingsRobot VerifyProtocolSettingsCard(string expectedProtocol)
    {
        Button settingsCard = ProtocolSettingsCard;

        Assert.IsNotNull(settingsCard);

        Assert.IsNotNull(settingsCard.FindFirstDescendant(cf => cf.ByText(expectedProtocol)));

        return this;
    }

    public SettingsRobot VerifyProtocolSettingsPage()
    {
        Assert.IsNotNull(GoBackButton);
        return this;
    }

    public SettingsRobot VerifySmartProtocolIsChecked()
    {
        RadioButton smartProtocolRadioButton = SmartProtocolRadioButton;
        RadioButton wireGuardProtocolRadioButton = WireGuardProtocolRadioButton;

        Assert.IsNotNull(smartProtocolRadioButton);
        Assert.IsTrue(smartProtocolRadioButton.IsChecked);

        Assert.IsNotNull(wireGuardProtocolRadioButton);
        Assert.IsFalse(wireGuardProtocolRadioButton.IsChecked);

        return this;
    }

    public SettingsRobot VerifyWireGuardProtocolIsChecked()
    {
        RadioButton smartProtocolRadioButton = SmartProtocolRadioButton;
        RadioButton wireGuardProtocolRadioButton = WireGuardProtocolRadioButton;

        Assert.IsNotNull(smartProtocolRadioButton);
        Assert.IsFalse(smartProtocolRadioButton.IsChecked);

        Assert.IsNotNull(wireGuardProtocolRadioButton);
        Assert.IsTrue(wireGuardProtocolRadioButton.IsChecked);

        return this;
    }
}