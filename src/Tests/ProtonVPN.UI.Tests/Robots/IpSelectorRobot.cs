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

using FlaUI.Core.AutomationElements;
using ProtonVPN.UI.Tests.UiTools;

namespace ProtonVPN.UI.Tests.Robots;

public class IpSelectorRobot
{
    protected Element IpSelectorOverlay = Element.ByAutomationId("IpSelectorOverlay");

    protected Element IpAddressTextBox = Element.ByAutomationId("IpAddressTextBox");
    protected Element IpAddressCheckBox = Element.ByAutomationId("AddressItemToggle");
    protected Element AddIpAddressButton = Element.ByAutomationId("AddButton");
    protected Element RemoveIpAddressButton = Element.ByAutomationId("TrashIcon");

    public Verifications Verify => new Verifications();

    public IpSelectorRobot AddIpAddress(string ipAddress)
    {
        IpAddressTextBox.SetText(ipAddress);
        AddIpAddressButton.ScrollIntoView().Click();
        return this;
    }

    public IpSelectorRobot TickIpAddressCheckBox(string ipAddress)
    {
        Element.ByName(ipAddress).ScrollIntoView();
        Element.ByName(ipAddress).Click();
        return this;
    }

    public IpSelectorRobot ClearIpInput()
    {
        IpAddressTextBox.ClearInput();
        return this;
    }

    public IpSelectorRobot DeleteAllIps()
    {
        RemoveIpAddressButton.WaitUntilExists();
        AutomationElement[] IpAllTrashIcons = RemoveIpAddressButton.FindAllElements();
        foreach (AutomationElement ipTrashIcon in IpAllTrashIcons)
        {
            ipTrashIcon.Patterns.ScrollItem.Pattern.ScrollIntoView();
            ipTrashIcon.AsButton().Invoke();
        }
        return this;
    }

    public class Verifications : IpSelectorRobot
    {
        public Verifications IsIpSelectorOpened()
        {
            IpSelectorOverlay.WaitUntilDisplayed();
            return this;
        }
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
}
