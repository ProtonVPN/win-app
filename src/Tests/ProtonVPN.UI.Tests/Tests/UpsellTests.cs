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

using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.Robots.Upsell.Carousel;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests;

[TestFixture]
[Category("UI")]
public class UpsellTests : TestSession
{
    private const int UPSELL_FEATURES_COUNT = 10;

    private LoginRobot _loginRobot = new();
    private HomeRobot _homeRobot = new();
    private UpsellCarouselRobot _upsellCarouselRobot = new();

    [SetUp]
    public void TestInitialize()
    {
        LaunchApp();

        _loginRobot
            .Wait(TestConstants.StartupDelay)
            .DoLogin(TestUserData.FreeUser);

        _homeRobot
            .DoWaitForVpnStatusSubtitleLabel();
    }

    [Test]
    public void UpsellCarousel()
    {
        _homeRobot
            .DoOpenUpsellCarousel();

        _upsellCarouselRobot
            .VerifyUpsellCarouselWindowIsOpened();

        for (int i = 0; i < UPSELL_FEATURES_COUNT; i++)
        {
            _upsellCarouselRobot
                .DoMoveToNextFeature();
        }

        for (int i = 0; i < UPSELL_FEATURES_COUNT; i++)
        {
            _upsellCarouselRobot
                .DoMoveToPreviousFeature();
        }
    }

    [TearDown]
    public void TestCleanup()
    {
        Cleanup();
    }
}