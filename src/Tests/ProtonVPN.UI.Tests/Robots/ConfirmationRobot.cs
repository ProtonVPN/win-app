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

public class ConfirmationRobot
{
    protected Element PrimaryActionButton = Element.ByAutomationId("PrimaryButton");
    protected Element SecondaryActionButton = Element.ByAutomationId("SecondaryButton");
    protected Element CancelActionButton = Element.ByAutomationId("CloseButton");


    public ConfirmationRobot PrimaryAction()
    {
        PrimaryActionButton.Click();
        return this;
    }

    public ConfirmationRobot SecondaryAction()
    {
        SecondaryActionButton.Click();
        return this;
    }

    public ConfirmationRobot CancelAction()
    {
        CancelActionButton.Click();
        return this;
    }
}
