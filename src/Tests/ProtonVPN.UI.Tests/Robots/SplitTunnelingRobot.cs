/*
 * Copyright (c) 2024 Proton AG
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

using ProtonVPN.UI.Tests.UiTools;

namespace ProtonVPN.UI.Tests.Robots;

public class SplitTunnelingRobot
{
    protected Element SplitTunnelingSwitch = Element.ByAutomationId("SplitTunnelingSwitch");
    protected Element AppsSelectorSettingsCard = Element.ByAutomationId("AppsSelectorSettingsCard");
    protected Element IpAddressesSelectorSettingsCard = Element.ByAutomationId("IpAddressesSelectorSettingsCard");

    protected Element ExcludeModeRadioButton = Element.ByName("Exclude mode");
    protected Element IncludeModeRadioButton = Element.ByName("Include mode");

    public SplitTunnelingRobot ToggleSplitTunnelingSwitch()
    {
        SplitTunnelingSwitch.Click();
        return this;
    }

    public SplitTunnelingRobot EditSplitTunnelingApps()
    {
        AppsSelectorSettingsCard.Click();
        return this;
    }

    public SplitTunnelingRobot EditSplitTunnelingIps()
    {
        IpAddressesSelectorSettingsCard.Click();
        return this;
    }

    public SplitTunnelingRobot SelectExcludeMode()
    {
        ExcludeModeRadioButton.Click();
        return this;
    }

    public SplitTunnelingRobot SelectIncludeMode()
    {
        IncludeModeRadioButton.Click();
        return this;
    }

    public class Verifications : SplitTunnelingRobot
    {
    }

    public Verifications Verify => new Verifications();
}