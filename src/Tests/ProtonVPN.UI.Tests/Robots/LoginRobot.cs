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

using FlaUI.Core.WindowsAPI;
using System.Threading;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.UiTools;
using FlaUI.Core.Input;

namespace ProtonVPN.UI.Tests.Robots;

public class LoginRobot
{
    protected Element UsernameTextBox = Element.ByAutomationId("UsernameTextBox");
    protected Element PasswordTextBox = Element.ByAutomationId("PasswordBox");
    protected Element TwoFactorFirstDigit = Element.ByAutomationId("FirstDigit");
    protected Element TwoFactorSecondDigit = Element.ByAutomationId("SecondDigit");
    protected Element TwoFactorThirdDigit = Element.ByAutomationId("ThirdDigit");
    protected Element TwoFactorFourthDigit = Element.ByAutomationId("FourthDigit");
    protected Element TwoFactorFifthDigit = Element.ByAutomationId("FifthDigit");
    protected Element TwoFactorLastDigit = Element.ByAutomationId("LastDigit");
    protected Element SignInButton = Element.ByAutomationId("SignInButton");
    protected Element SsoWindow = Element.ByAutomationId("ContentScrollViewer");
    protected Element SignInWithSsoButton = Element.ByName("Sign in with SSO");
    protected Element HelpButton = Element.ByAutomationId("HelpButton");
    protected Element ReportIssueMenuItem = Element.ByAutomationId("ReportIssueMenuItem");
    protected Element CancelSignInButton = Element.ByAutomationId("CancelSignInButton");

    public LoginRobot Login(TestUserData user)
    {
        UsernameTextBox.SetText(user.Username);
        PasswordTextBox.SetText(user.Password);
        SignInButton.Invoke();

        return this;
    }

    public LoginRobot EnterEmail(TestUserData user)
    {
        UsernameTextBox.SetText(user.Username);
        return this;
    }

    public LoginRobot ClickSignInButton()
    {
        SignInButton.Invoke();
        return this;
    }

    public LoginRobot ClickSignInWithSso()
    {
        SignInWithSsoButton.Click();
        return this;
    }

    public LoginRobot EnterTwoFactorCode(string twoFactorCode)
    {
        TwoFactorFirstDigit.SetText(twoFactorCode[0].ToString());
        TwoFactorSecondDigit.SetText(twoFactorCode[1].ToString());
        TwoFactorThirdDigit.SetText(twoFactorCode[2].ToString());
        TwoFactorFourthDigit.SetText(twoFactorCode[3].ToString());
        TwoFactorFifthDigit.SetText(twoFactorCode[4].ToString());
        TwoFactorLastDigit.SetText(twoFactorCode[5].ToString());

        return this;
    }

    public LoginRobot DoLoginSsoWebview(string password)
    {
        //We have a very limited ability to use WebView, that is why we are using static pauses and keyboard strokes.
        SsoWindow.WaitUntilDisplayed(TestConstants.ThirtySecondsTimeout);
        Thread.Sleep(15000);
        SsoWindow.Click();

        Keyboard.Type(VirtualKeyShort.TAB);
        Keyboard.Type(password);
        Keyboard.Type(VirtualKeyShort.TAB);
        Keyboard.Type(VirtualKeyShort.ENTER);
        return this;
    }

    public void NavigateToBugReport()
    {
        HelpButton.Click();
        // Remove when VPNWIN-2599 is implemented.
        Thread.Sleep(TestConstants.AnimationDelay);
        ReportIssueMenuItem.DoubleClick();
    }

    public LoginRobot CancelLogin()
    {
        CancelSignInButton.Click();

        return this;
    }

    public class Verifications : LoginRobot
    {
        public Verifications IsErrorMessageDisplayed(string errorMessage)
        {
            Element.ByName(errorMessage).WaitUntilDisplayed();
            return this;
        }

        public Verifications IsLoginWindowDisplayed()
        {
            UsernameTextBox.WaitUntilDisplayed(TestConstants.ThirtySecondsTimeout);
            PasswordTextBox.WaitUntilDisplayed();
            return this;
        }
    }

    public Verifications Verify => new();
}