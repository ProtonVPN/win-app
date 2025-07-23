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
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;

namespace ProtonVPN.UI.Tests.TestsHelper;
public class CommonUiFlows : BaseTest
{
    public static HomeRobot FullLogin(TestUserData testUser)
    {
        LoginRobot
            .Login(testUser);

        NavigationRobot
            .Verify.IsOnMainPage()
                   .IsOnHomePage();

        HomeRobot
            .DismissWelcomeModal();

        // Remove when VPNWIN-2599 is implemented. 
        Thread.Sleep(TestConstants.AnimationDelay);

        return new HomeRobot();
    }
}
