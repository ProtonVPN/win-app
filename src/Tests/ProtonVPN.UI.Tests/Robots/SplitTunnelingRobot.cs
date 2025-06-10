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

using System.Threading;
using FlaUI.Core.AutomationElements;
using ProtonVPN.UI.Tests.UiTools;

namespace ProtonVPN.UI.Tests.Robots;

public class SplitTunnelingRobot
{
    protected Element SplitTunnelingSwitch = Element.ByAutomationId("SplitTunnelingSwitch");
    protected Element IpAddressTextBox = Element.ByAutomationId("IpAddressTextBox");
    protected Element AddIpAddressButton = Element.ByAutomationId("AddIpAddressButton");
    protected Element ExcludeModeRadioButton = Element.ByName("Exclude mode");
    protected Element IncludeModeRadioButton = Element.ByName("Include mode");
    protected Element TrashIcon = Element.ByAutomationId("RemoveIpAddressButton");

    public SplitTunnelingRobot ToggleSplitTunnelingSwitch()
    {
        SplitTunnelingSwitch.Click();
        return this;
    }

    public SplitTunnelingRobot AddIpAddress(string ipAddress)
    {
        IpAddressTextBox.SetText(ipAddress);
        AddIpAddressButton.ScrollIntoView().Click();
        return this;
    }

    public SplitTunnelingRobot TickIpAddressCheckBox(string ipAddress)
    {
        Element.ByName(ipAddress).ScrollIntoView();
        Element.ByName(ipAddress).Click();
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

    public SplitTunnelingRobot ClearIpInput()
    {
        IpAddressTextBox.ClearInput();
        return this;
    }

    public SplitTunnelingRobot DeleteAllIps()
    {
        TrashIcon.WaitUntilExists();
        AutomationElement[] IpAllTrashIcons = TrashIcon.FindAllElements();
        foreach (AutomationElement ipTrashIcon in IpAllTrashIcons)
        {
            ipTrashIcon.Patterns.ScrollItem.Pattern.ScrollIntoView();
            ipTrashIcon.AsButton().Invoke();
        }
        return this;
    }

    public class Verifications : SplitTunnelingRobot
    {
        public Verifications WasIpNotAdded(string ipAddress)
        {
            Element.ByName(ipAddress).DoesNotExist();
            IpAddressTextBox.ValueEquals(ipAddress);
            return this;
        }

        public Verifications WasIpAdded(string ipAddress)
        {
            Element.ByName(ipAddress).WaitUntilDisplayed();
            IpAddressTextBox.ValueEquals("");
            return this;
        }
    }

    public Verifications Verify => new Verifications();
}
