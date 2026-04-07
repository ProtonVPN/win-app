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

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
[Category("ARM")]
public class LoginTests : FreshSessionSetUp
{
    private const string INCORRECT_USER_ERROR = "This username does not exist. Please try again with a different username.";
    private const string INCORRECT_PASS_ERROR = "The password is not correct. Please try again with a different password.";
    private const string INCORRECT_CREDENTIALS_ERROR = "Incorrect login credentials. Please try again.";
    private const string EMPTY_USERNAME_ERROR = "Enter your email or username";
    private const string EMPTY_PASSWORD_ERROR = "Enter your password";
    private const string INCORRECT_2FA_CODE_ERROR = "Incorrect code. Please try again.";
    private const string INCORRECT_2FA_CODE = "123456";
    private const string INCORRECT_USERNAME_ERROR = "Invalid username";
    private const string NO_SERVERS_ERROR = "To start your journey in Proton VPN please contact your organization administrator to assign VPN connections to your account.";

    private const string DISABLE_INTERNET_SCRIPT = @"Disable-NetAdapter -Name ""Ethernet"" -Confirm:$false"; //Wi-Fi - local, Ethernet - ci
    private const string ENABLE_INTERNET_SCRIPT = @"Enable-NetAdapter -Name ""Ethernet"" -Confirm:$false";

    private const string LINE_TO_LOOK_FOR_IN_CLIENT = "vpn/v2/logicals?";
    private const string WORD_TO_LOOK_FOR_IN_SERVER = "protonvpn.net";
    private const string LINE_TO_LOOK_FOR_IN_SERVER = "node-";

    private static string ClientLogsPath => TestConstants.ClientLogsPath;
    private static string? ServerStoragePath => TestConstants.ServerStoragePath;

    [Test]
    public void LoginWithSpecialCharsUser()
    {
        LoginWithUser(TestUserData.SpecialCharsUser);
    }

    [Test]
    public void LoginWithTwoPassUser()
    {
        LoginWithUser(TestUserData.TwoPassUser);
    }

    [Test]
    public void LoginWithIncorrectCredentials()
    {
        NavigationRobot
            .Verify.IsOnLoginPage();

        LoginRobot
            .Login(TestUserData.IncorrectUserAndPass)
            .Verify.IsErrorMessageDisplayed(INCORRECT_USER_ERROR);
    }

    [Test]
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
    }

    [Test]
    public void LoginWithIncorrectTwoFactorCode()
    {
        LoginRobot
            .Login(TestUserData.TwoFactorUser)
            .EnterTwoFactorCode(INCORRECT_2FA_CODE)
            .Verify.IsErrorMessageDisplayed(INCORRECT_2FA_CODE_ERROR);
    }

    [Test]
    public void LoginWithWhitespaceUsername()
    {
        NavigationRobot
            .Verify.IsOnLoginPage();

        LoginRobot
            .Login(TestUserData.IncorrectUserWithWhitespace)
            .Verify.IsErrorMessageDisplayed(INCORRECT_USERNAME_ERROR);

        LoginRobot
            .Login(TestUserData.CorrectUserWithWhitespace);
        NavigationRobot
            .Verify.IsOnMainPage();
    }

    [Test]
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
    [Ignore("JIRA - VPNWIN-3177")]
    public void LoginWithEmptyCredentials()
    {
        NavigationRobot
            .Verify.IsOnLoginPage();

        LoginRobot
            .ClickSignInButton()
            .Verify.IsErrorMessageDisplayed(EMPTY_USERNAME_ERROR)
                   .IsErrorMessageDisplayed(EMPTY_PASSWORD_ERROR);

        LoginRobot
            .ClickSignInWithSso()
            .ClickSignInButton()
            .Verify.IsErrorMessageDisplayed(EMPTY_USERNAME_ERROR);
    }

    [Test]
    public void LoginWithZeroConnectionsAccount()
    {
        NavigationRobot
            .Verify.IsOnLoginPage();

        LoginRobot
            .Login(TestUserData.ZeroAssignedConnectionsUser);

        NavigationRobot
            .Verify.IsOnNoServersPage()
                   .IsMessageDisplayed(NO_SERVERS_ERROR)
            .ClickRefreshButtonOnNoServersPage()
            .Verify.IsOnNoServersPage()
            .ClickSignOutButtonOnNoServersPage()
            .Verify.IsOnLoginPage();
    }

    [Test]
    public void LoginWithInvalidCredentialsFiveTimes()
    {
        for (int i = 0; i < 5; i++)
        {
            NavigationRobot
                .Verify.IsOnLoginPage();

            LoginRobot
                .Login(TestUserData.IncorrectPass)
                .Verify.IsErrorMessageDisplayed(INCORRECT_PASS_ERROR);
        }
    }

    [Test]
    [Retry(3)]
    public void LoginWithValidCredentials()
    {
        (string Plan, TestUserData User)[] usersToCheck =
        {
            (Plan: "VPN Plus", User: TestUserData.PlusUser),
            (Plan: "Visionary", User: TestUserData.VisionaryUser),
            (Plan: "Proton Unlimited", User: TestUserData.UnlimitedUser),
            (Plan: "Proton VPN Free", User: TestUserData.FreeUser)
        };

        foreach ((string Plan, TestUserData User) userToCheck in usersToCheck)
        {
            CommonUiFlows.FullLogin(userToCheck.User);

            SettingRobot
                .OpenSettings()
                .Verify.IsCorrectAccountInfoDisplayed(userToCheck.User.Username, userToCheck.Plan)
                .ExpandAccountDropdown()
                .SignOut()
                .ConfirmSignOut();

            LoginRobot
                .Verify.IsLoginWindowDisplayed();
        }
    }

    [Test]
    public void ServerListFullyLoadedAfterLogin()
    {
        TestUserData[] usersToCheck = { TestUserData.PlusUser, TestUserData.FreeUser };

        foreach (TestUserData userToCheck in usersToCheck)
        {
            CommonUiFlows.FullLogin(userToCheck);

            SidebarRobot
                .Verify.AreAllServersDisplayed();

            //give it time to populate the service-logs after connecting
            Thread.Sleep(TestConstants.OneSecondTimeout);

            WindowsUtils.AssertLogFile(ClientLogsPath, LINE_TO_LOOK_FOR_IN_CLIENT);

            WindowsUtils.AssertLogFile(ServerStoragePath!, LINE_TO_LOOK_FOR_IN_SERVER, WORD_TO_LOOK_FOR_IN_SERVER);

            HomeRobot.ExpandKebabMenuButton();
            SettingRobot
                .SignOut()
                .ConfirmSignOut();
            LoginRobot.Verify.IsLoginWindowDisplayed();
        }
    }

    [Test, Order(99)]
    public void LoginWithoutInternet()
    {
        NavigationRobot
            .Verify.IsOnLoginPage();

        WindowsUtils.RunPowerShellScript(DISABLE_INTERNET_SCRIPT);
        NetworkUtils.IsInternetAvailable(false);

        LoginRobot
           .Login(TestUserData.PlusUser);

        Thread.Sleep(TestConstants.FiveSecondsTimeout);
        SupportRobot
            .Verify.IsConnectionHelpDisplayed()
            .CloseSupportWindow();

        LoginRobot
            .Verify.IsLoginWindowDisplayed();

        WindowsUtils.RunPowerShellScript(ENABLE_INTERNET_SCRIPT);
        NetworkUtils.IsInternetAvailable(true);
    }

    private void LoginWithUser(TestUserData user)
    {
        NavigationRobot
            .Verify.IsOnLoginPage();

        LoginRobot
            .Login(user);

        NavigationRobot
            .Verify.IsOnMainPage();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        WindowsUtils.RunPowerShellScript(ENABLE_INTERNET_SCRIPT);
        NetworkUtils.IsInternetAvailable(true);
    }
}
