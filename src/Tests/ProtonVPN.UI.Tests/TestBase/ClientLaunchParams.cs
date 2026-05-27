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

using System.Text;

namespace ProtonVPN.UI.Tests.TestBase;

public class ClientLaunchParams
{
    public static ClientLaunchParams FreshStartWithNoOnboarding => new();
    public static ClientLaunchParams FreshStartWithOnboarding => new(shouldSkipOnboarding: false);
    public static ClientLaunchParams StartWithNoOnboarding => new(isFreshStart: false);
    public static ClientLaunchParams StartWithNoOnboardingNoRefresh => new(isFreshStart: false, shouldRefreshWindow: false);

    public bool IsFreshStart { get; }
    public bool ShouldSkipOnboarding { get; }
    public bool ShouldRefreshWindow { get; }
    public bool ShouldExitAppOnClose { get; }
    public bool ShouldDisableAutoUpdate { get; }

    private ClientLaunchParams(
        bool isFreshStart = true,
        bool shouldRefreshWindow = true,
        bool shouldSkipOnboarding = true,
        bool shouldExitAppOnClose = false,
        bool shouldDisableAutoUpdate = true)
    {
        IsFreshStart = isFreshStart;
        ShouldRefreshWindow = shouldRefreshWindow;
        ShouldSkipOnboarding = shouldSkipOnboarding;
        ShouldExitAppOnClose = shouldExitAppOnClose;
        ShouldDisableAutoUpdate = shouldDisableAutoUpdate;
    }

    public string BuildArguments()
    {
        StringBuilder arguments = new();

        if (ShouldExitAppOnClose)
        {
            arguments.Append("-ExitAppOnClose ");
        }

        if (ShouldDisableAutoUpdate)
        {
            arguments.Append("-DisableAutoUpdate ");
        }

        if (ShouldSkipOnboarding)
        {
            arguments.Append("-SkipOnboarding");
        }

        return arguments.ToString().Trim();
    }
}