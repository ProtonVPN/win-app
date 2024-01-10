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

namespace ProtonVPN.UI.Tests.Robots.Login;

public partial class LoginRobot : UIActions
{
    protected TextBox UsernameTextBox => ElementByAutomationId("UsernameTextBox").AsTextBox();
    protected TextBox PasswordBox => ElementByAutomationId("PasswordBox").AsTextBox();
    protected Button SignInButton => ElementByAutomationId("SignInButton").AsButton();
    protected TextBox ErrorMessageTextBox => ElementByAutomationId("Message", TestConstants.MediumTimeout).AsTextBox();
    protected TextBox AuthenticateButton => ElementByAutomationId("AuthenticateButton").AsTextBox();
    protected TextBox TwoFactorInputField(string position) => ElementByName($"{position} digit").AsTextBox();

    protected Button HelpButton => ElementByAutomationId("HelpButton").AsButton();
    protected Menu HelpFlyoutMenu => ElementByAutomationId("HelpFlyoutMenu").AsMenu();
    protected MenuItem ReportIssueMenuItem => HelpFlyoutMenu.FindFirstDescendant("ReportIssueMenuItem").AsMenuItem();
}