/*
 * Copyright (c) 2026 Proton AG
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

using System;
using System.Threading;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using ProtonVPN.UI.Tests.UiTools;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Robots;

public class LoginRobot
{
    private static readonly string _killSwitchDisabledLabelTranslated = LanguageHelper.GetTranslatedString("SignIn_KillSwitch_Disabled");

    protected Element KillSwitchDisabledLabel = Element.ByName(_killSwitchDisabledLabelTranslated);
    protected Element DisableKillSwitchButton = Element.ByAutomationId("DisableKillSwitchButton");
    protected Element DisableKillSwitchLabel = Element.ByAutomationId("AdvancedKillSwitchDescriptionText");
    protected Element UsernameTextBox = Element.ByAutomationId("UsernameTextBox");
    protected Element PasswordTextBox = Element.ByAutomationId("PasswordBox");
    protected Element TwoFactorFirstDigit = Element.ByAutomationId("FirstDigit");
    protected Element TwoFactorSecondDigit = Element.ByAutomationId("SecondDigit");
    protected Element TwoFactorThirdDigit = Element.ByAutomationId("ThirdDigit");
    protected Element TwoFactorFourthDigit = Element.ByAutomationId("FourthDigit");
    protected Element TwoFactorFifthDigit = Element.ByAutomationId("FifthDigit");
    protected Element TwoFactorLastDigit = Element.ByAutomationId("LastDigit");

    protected Element SignInButton = Element.ByAutomationId("SignInButton");
    protected Element CreateAccountButton = Element.ByAutomationId("CreateAccountButton");
    protected Element SsoWindow = Element.ByAutomationId("ContentScrollViewer");
    protected Element SignInWithSsoButton = Element.ByAutomationId("SwitchSignInButton");
    protected Element CancelSignInButton = Element.ByAutomationId("CancelSignInButton");

    protected Element CaptchaWindow = Element.ByAutomationId("WebView2");
    protected Element CloseCaptchaButton = Element.ByAutomationId("CloseContentDialogButton");

    protected Element HelpButton = Element.ByAutomationId("HelpButton");
    protected Element ReportIssueMenuItem = Element.ByAutomationId("ReportIssueMenuItem");
    protected Element ForgotUsernameMenuItem = Element.ByAutomationId("ForgotUsernameMenuItem");
    protected Element ForgotPasswordMenuItem = Element.ByAutomationId("ForgotPasswordMenuItem");

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

    public LoginRobot ClickCreateAccountButton()
    {
        CreateAccountButton.Invoke();
        return this;
    }

    public LoginRobot ClickSignInWithSso()
    {
        SignInWithSsoButton.Click();
        return this;
    }

    public LoginRobot EnterTwoFactorCode(string twoFactorCode)
    {
        TwoFactorFirstDigit.WaitUntilDisplayed(TestConstants.OneMinuteTimeout);
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
        SsoWindow.WaitUntilDisplayed(TestConstants.OneMinuteTimeout);
        Thread.Sleep(TestConstants.ThirtySecondsTimeout);
        SsoWindow.Click();

        Keyboard.Type(VirtualKeyShort.TAB);
        Keyboard.Type(password);
        Keyboard.Type(VirtualKeyShort.TAB);
        Keyboard.Type(VirtualKeyShort.ENTER);
        return this;
    }

    public void OpenHelpMenu()
    {
        HelpButton.Click();
        // Remove when VPNWIN-2599 is implemented.
        Thread.Sleep(TestConstants.AnimationDelay);
    }

    public void NavigateToForgotUsername()
    {
        OpenHelpMenu();
        ForgotUsernameMenuItem.DoubleClick();
    }

    public void NavigateToForgotPassword()
    {
        OpenHelpMenu();
        ForgotPasswordMenuItem.DoubleClick();
    }

    public void NavigateToBugReport()
    {
        OpenHelpMenu();
        ReportIssueMenuItem.DoubleClick();
    }

    public LoginRobot CancelLogin()
    {
        CancelSignInButton.Click();
        return this;
    }

    public LoginRobot DisableKillSwitch()
    {
        DisableKillSwitchButton.Click();
        KillSwitchDisabledLabel.WaitUntilDisplayed();
        return this;
    }

    public LoginRobot ClickCloseCaptchaButton()
    {
        CloseCaptchaButton.Click();
        return this;
    }

    public class Verifications : LoginRobot
    {
        public Verifications IsCaptchaDisplayed()
        {
            CaptchaWindow.WaitUntilDisplayed();
            Thread.Sleep(TestConstants.OneSecondTimeout);
            return this;
        }

        public Verifications IsErrorMessageDisplayed(string errorMessage)
        {
            Element.ByName(errorMessage).WaitUntilDisplayed();
            return this;
        }

        public Verifications IsLoginWindowDisplayed(TimeSpan? timeout = null)
        {
            timeout ??= TestConstants.OneMinuteTimeout;
            UsernameTextBox.WaitUntilDisplayed(timeout);
            PasswordTextBox.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsAdvancedKillSwitchDisplayed()
        {
            DisableKillSwitchLabel.WaitUntilDisplayed(TestConstants.FiveSecondsTimeout);
            return this;
        }
    }

    public Verifications Verify => new();
}