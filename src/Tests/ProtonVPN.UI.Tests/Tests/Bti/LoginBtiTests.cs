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

using System.Threading.Tasks;
using NUnit.Framework;
using ProtonVPN.UI.Tests.ApiClient.TestEnv;
using ProtonVPN.UI.Tests.Robots.Home;
using ProtonVPN.UI.Tests.Robots.Login;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.Bti;

[TestFixture]
[Category("BTI")]
public class LoginBtiTests : TestSession
{
    private LoginRobot _loginRobot = new();
    private HomeRobot _homeRobot = new();
    private AtlasApiClient _atlasApiClient = new AtlasApiClient();

    [SetUp]
    public async Task TestInitializeAsync()
    {
        await _atlasApiClient.CleanupEnvVarsAsync();
        LaunchApp();
    }

    [Test]
    public async Task LoginUsingCaptcha()
    {
        await _atlasApiClient.ForceCaptchaOnLoginAsync();
        _loginRobot.DoLogin(TestUserData.PlusUserBti)
            .WaitUntilCaptchaIsDisplayed();
        _homeRobot.DoWaitForVpnStatusSubtitleLabel();
    }

    [TearDown]
    public async Task TestCleanupAsync()
    {
        await _atlasApiClient.CleanupEnvVarsAsync();
        Cleanup();
    }
}
