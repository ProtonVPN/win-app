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

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Core.Services.Activation.Bases;

public abstract class FrameActivatorBase : ActivatorBase<Frame>
{
    protected FrameActivatorBase(
        ILogger logger)
        : base(logger)
    { }

    public void Load()
    {
        if (Host != null)
        {
            OnLoaded();
        }
    }

    public void Unload()
    {
        if (Host != null)
        {
            OnUnloaded();
        }
    }

    protected virtual void OnLoaded()
    { }

    protected virtual void OnUnloaded()
    { }

    protected override void RegisterToHostEvents()
    {
        if (Host != null)
        {
            Host.Navigating += OnNavigating;
            Host.Navigated += OnNavigated;
            Host.NavigationFailed += OnNavigationFailed;
            Host.NavigationStopped += OnNavigationStopped;
        }
    }

    protected override void UnregisterFromHostEvents()
    {
        if (Host != null)
        {
            Host.Navigating -= OnNavigating;
            Host.Navigated -= OnNavigated;
            Host.NavigationFailed -= OnNavigationFailed;
            Host.NavigationStopped -= OnNavigationStopped;
        }
    }

    protected virtual void OnFrameNavigating(NavigatingCancelEventArgs e)
    { }

    protected virtual void OnFrameNavigated(NavigationEventArgs e)
    { }

    private void OnNavigating(object sender, NavigatingCancelEventArgs e)
    {
        OnFrameNavigating(e);
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        OnFrameNavigated(e);
    }

    private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        Logger.Error<AppLog>($"Navigation to the page '{e.SourcePageType}' has failed.", e.Exception);
    }

    private void OnNavigationStopped(object sender, NavigationEventArgs e)
    {
        Logger.Info<AppLog>($"Navigation to the page '{e.SourcePageType}' was stopped.");
    }
}
