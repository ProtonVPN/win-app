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

using System.Threading;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
[Category("ARM")]
public class LoginTests : FreshSessionSetUp
{
    private const string INCORRECT_2FA_CODE = "123456";

    private const string LINE_TO_LOOK_FOR_IN_CLIENT = "vpn/v2/logicals?";
    private const string WORD_TO_LOOK_FOR_IN_SERVER = "protonvpn.net";
    private const string LINE_TO_LOOK_FOR_IN_SERVER = "node-";

    private static readonly string _incorrectUserError = ApiTranslationHelper.GetTranslatedString("Login_IncorrectUserError");
    private static readonly string _incorrectPasswordError = ApiTranslationHelper.GetTranslatedString("Login_IncorrectPasswordError");
    private static readonly string _incorrectCredentialsError = ApiTranslationHelper.GetTranslatedString("Login_IncorrectCredentialsError");
    private static readonly string _incorrectUsernameError = ApiTranslationHelper.GetTranslatedString("Login_InvalidUsername");
    private static readonly string _noServersError = ApiTranslationHelper.GetTranslatedString("Login_NoServersError");
    private static readonly string _emptyUsernameError = LanguageHelper.GetTranslatedString("SignIn_Form_UsernameError");
    private static readonly string _emptyPasswordError = LanguageHelper.GetTranslatedString("SignIn_Form_PasswordError");
    private static readonly string _incorrectTwoFaCodeError = LanguageHelper.GetTranslatedString("Login_Error_IncorrectTwoFactorCode");

    private static string ClientLogsPath => TestConstants.ClientLogsPath;
    private static string? ServerStoragePath => TestConstants.ServerStoragePath;

    private static readonly (string Plan, TestUserData User)[] _validCredentialUsersToCheck =
        [
            (Plan: "VPN Plus", User: TestUserData.PlusUser),
            (Plan: "Visionary", User: TestUserData.VisionaryUser),
            (Plan: "Proton Unlimited", User: TestUserData.UnlimitedUser),
            (Plan: "Proton VPN Free", User: TestUserData.FreeUser)
        ];

    private static readonly (string Plan, TestUserData User)[] _serverListUsersToCheck =
        [
            (Plan: "VPN Plus", User: TestUserData.PlusUser),
            (Plan: "Proton VPN Free", User: TestUserData.FreeUser)
        ];

    [Test]
    [Property("TestCaseId", "602337")]
    public void LoginWithSpecialCharsUser()
    {
        LoginWithUser(TestUserData.SpecialCharsUser);
    }

    [Test]
    [Property("TestCaseId", "602325")]
    public void LoginWithTwoPassUser()
    {
        LoginWithUser(TestUserData.TwoPassUser);
    }

    [Test]
    [Property("TestCaseId", "602321")]
    public void LoginWithIncorrectCredentials()
    {
        NavigationRobot
            .Verify.IsOnLoginPage();

        LoginRobot
            .Login(TestUserData.IncorrectUserAndPass)
            .Verify.IsErrorMessageDisplayed(_incorrectUserError);
    }

    [Test]
    [Property("TestCaseId", "602322")]
    [Retry(3)]
    public void LoginWithTwoFactor()
    {
        NavigationRobot
            .Verify.IsOnLoginPage();

        LoginRobot
            .Login(TestUserData.TwoFactorUser)
            .EnterTwoFactorCode(TestUserData.GetTwoFactorCode());

        NavigationRobot
            .Verify.IsOnMainPage();

        try
        {
            HomeRobot
                .Verify.IsUpsellModalDisplayed()
                .DismissUpsellModal();
        }
        catch { }
    }

    [Test]
    [Property("TestCaseId", "602324")]
    public void LoginWithIncorrectTwoFactorCode()
    {
        LoginRobot
            .Login(TestUserData.TwoFactorUser)
            .EnterTwoFactorCode(INCORRECT_2FA_CODE)
            .Verify.IsErrorMessageDisplayed(_incorrectTwoFaCodeError);
    }

    [Test]
    [Property("TestCaseId", "791683")]
    public void LoginWithWhitespaceUsername()
    {
        NavigationRobot
            .Verify.IsOnLoginPage();

        LoginRobot
            .Login(TestUserData.IncorrectUserWithWhitespace)
            .Verify.IsErrorMessageDisplayed(_incorrectUsernameError);

        LoginRobot
            .Login(TestUserData.CorrectUserWithWhitespace);
        NavigationRobot
            .Verify.IsOnMainPage();
    }

    [Test]
    [Property("TestCaseId", "760480")]
    public void CancelLogin()
    {
        NavigationRobot
            .Verify.IsOnLoginPage();

        LoginRobot
            .Login(TestUserData.PlusUser);

        Thread.Sleep(TestConstants.OneSecondTimeout);

        LoginRobot
            .CancelLogin()
            .Verify.IsLoginWindowDisplayed();
    }

    [Test]
    [Property("TestCaseId", "760481")]
    public void CancelTwoFactorLogin()
    {
        NavigationRobot
            .Verify.IsOnLoginPage();

        LoginRobot
            .Login(TestUserData.TwoFactorUser)
            .EnterTwoFactorCode(TestUserData.GetTwoFactorCode());

        Thread.Sleep(TestConstants.OneSecondTimeout);

        LoginRobot
            .CancelLogin()
            .Verify.IsLoginWindowDisplayed();
    }

    [Test]
    [Property("TestCaseId", "602323")]
    public void LoginWithEmptyCredentials()
    {
        NavigationRobot
            .Verify.IsOnLoginPage();

        LoginRobot
            .ClickSignInButton()
            .Verify.IsErrorMessageDisplayed(_emptyUsernameError)
                   .IsErrorMessageDisplayed(_emptyPasswordError);

        LoginRobot
            .ClickSignInWithSso()
            .ClickSignInButton()
            .Verify.IsErrorMessageDisplayed(_emptyUsernameError);
    }

    [Test]
    [Property("TestCaseId", "602326")]
    public void LoginWithZeroConnectionsAccount()
    {
        NavigationRobot
            .Verify.IsOnLoginPage();

        LoginRobot
            .Login(TestUserData.ZeroAssignedConnectionsUser);

        NavigationRobot
            .Verify.IsOnNoServersPage()
                   .IsMessageDisplayed(_noServersError)
            .ClickRefreshButtonOnNoServersPage()
            .Verify.IsOnNoServersPage()
            .ClickSignOutButtonOnNoServersPage()
            .Verify.IsOnLoginPage();
    }

    [Test]
    [Property("TestCaseId", "602328")]
    public void LoginWithInvalidCredentialsFiveTimes()
    {
        for (int i = 0; i < 5; i++)
        {
            NavigationRobot
                .Verify.IsOnLoginPage();

            LoginRobot
                .Login(TestUserData.IncorrectPass)
                .Verify.IsErrorMessageDisplayed(_incorrectPasswordError);
        }
    }

    [Test]
    [Property("TestCaseId", "602320")]
    [Category("SMOKE_1")]
    [Retry(3)]
    [TestCaseSource(nameof(_validCredentialUsersToCheck))]
    public void LoginWithValidCredentials((string Plan, TestUserData User) userToCheck)
    {
        CommonUiFlows.FullLogin(userToCheck.User);

        try
        {
            DesktopRobot.CloseSurvey();
        }
        catch { }

        try
        {
            HomeRobot
                .Verify.IsUpsellModalDisplayed()
                .DismissUpsellModal();
        }
        catch { }

        SettingRobot
            .OpenSettings()
            .Verify.IsCorrectAccountInfoDisplayed(userToCheck.User.Username, userToCheck.Plan)
            .CloseSettings();

        CommonUiFlows.Logout();
    }

    [Test]
    [Property("TestCaseId", "602329")]
    [TestCaseSource(nameof(_serverListUsersToCheck))]
    public void ServerListFullyLoadedAfterLogin((string Plan, TestUserData User) userToCheck)
    {
        CommonUiFlows.FullLogin(userToCheck.User);

        SidebarRobot
            .Verify.AreAllServersDisplayed();

        //give it time to populate the service-logs after connecting
        Thread.Sleep(TestConstants.OneSecondTimeout);

        WindowsUtils.AssertLogFile(ClientLogsPath, LINE_TO_LOOK_FOR_IN_CLIENT);

        WindowsUtils.AssertLogFile(ServerStoragePath!, LINE_TO_LOOK_FOR_IN_SERVER, WORD_TO_LOOK_FOR_IN_SERVER);

        CommonUiFlows.Logout();
    }

    [Test]
    [Property("TestCaseId", "602319")]
    [Retry(3)]
    public void LoginWithoutInternet()
    {
        try
        {
            NavigationRobot
                .Verify.IsOnLoginPage();

            ScriptHelper.DisableInternet();
            NetworkUtils.AssertInternetAvailability(false);

            LoginRobot
               .Login(TestUserData.PlusUser);

            Thread.Sleep(TestConstants.TenSecondsTimeout);
            SupportRobot
                .Verify.IsConnectionHelpDisplayed()
                .CloseSupportWindow();

            LoginRobot
                .Verify.IsLoginWindowDisplayed();
        }
        finally
        {
            ScriptHelper.EnableInternet();
            NetworkUtils.AssertInternetAvailability(true);
        }
    }

    private void LoginWithUser(TestUserData user)
    {
        NavigationRobot
            .Verify.IsOnLoginPage();

        LoginRobot
            .Login(user);

        NavigationRobot
            .Verify.IsOnMainPage();

        try
        {
            HomeRobot
                .Verify.IsUpsellModalDisplayed()
                .DismissUpsellModal();
        }
        catch { }
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        ScriptHelper.EnableInternet();
        NetworkUtils.AssertInternetAvailability(true);
    }
}
