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
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using static ProtonVPN.UI.Tests.TestsHelper.TestConstants;

namespace ProtonVPN.UI.Tests.TestsHelper;

public class CommonUiFlows : BaseTest
{
    public static void FullLogin(TestUserData testUser)
    {
        LoginRobot
            .Login(testUser);

        NavigationRobot
            .Verify.IsOnMainPage()
                   .IsOnHomePage();

        // Remove when VPNWIN-2599 is implemented. 
        Thread.Sleep(TestConstants.AnimationDelay);
    }

    public static void Logout()
    {
        HomeRobot
            .ExpandKebabMenuButton();

        SettingRobot
            .SignOut()
            .ConfirmSignOut();

        LoginRobot
            .Verify.IsLoginWindowDisplayed();
    }

    public static void EnsureUserIsDisconnected(bool shouldVerifyKillSwitch = false)
    {
        Action verifyDisconnectState = shouldVerifyKillSwitch
            ? () => HomeRobot.Verify.IsAdvancedKillSwitchActivated()
            : () => HomeRobot.Verify.IsDisconnected();

        try
        {
            verifyDisconnectState();
        }
        catch (TimeoutException)
        {
            HomeRobot
                .Disconnect();
            verifyDisconnectState();
        }
    }

    public static void ChangeProtocol(Protocol protocol, bool shouldEnableProTun = false)
    {
        SettingRobot
            .OpenSettings()
            .OpenProtocolSettings();

        HandleProtun(shouldEnableProTun);

        SettingRobot
            .SelectProtocol(protocol)
            .ApplySettings()
            .CloseSettings();
    }

    private static void HandleProtun(bool shouldEnableProTun)
    {
        if (TestConstants.IsProtunVersion)
        {
            if (shouldEnableProTun)
            {
                SettingRobot.EnableProtunToggle();
            }
            else
            {
                SettingRobot.DisableProtunToggle();
            }
        }
    }
}