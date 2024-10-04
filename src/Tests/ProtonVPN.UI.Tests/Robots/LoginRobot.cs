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

using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.UiTools;

namespace ProtonVPN.UI.Tests.Robots;

public class LoginRobot
{
    protected Element UsernameTextBox => Element.ByAutomationId("UsernameTextBox");
    protected Element PasswordTextBox => Element.ByAutomationId("PasswordBox");
    protected Element SignInButton => Element.ByAutomationId("SignInButton");

    public LoginRobot Login(TestUserData user)
    {
        UsernameTextBox.SetText(user.Username);
        PasswordTextBox.SetText(user.Password);
        SignInButton.Click();

        return this;
    }

    public class Verifications : LoginRobot
    {
        
    }

    public Verifications Verify => new Verifications();
}
