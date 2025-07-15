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

using Microsoft.UI.Xaml;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Activation.Bases;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Dialogs.Tray;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Core.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts;

namespace ProtonVPN.Client.Services.Activation;

public class TrayAppWindowActivator : DialogActivatorBase<TrayAppWindow>, ITrayAppWindowActivator
{
    private const double TRAY_APP_LOGIN_WIDTH = 340;
    private const double TRAY_APP_LOGIN_HEIGHT = 420;
    private const double TRAY_APP_MAIN_WIDTH = 425;
    private const double TRAY_APP_MAIN_HEIGHT = 620;
    private const double TRAY_APP_FREE_MAIN_HEIGHT = 520;
    private const double TRAY_APP_MARGIN = 10;

    private readonly IUserAuthenticator _userAuthenticator;

    public override string WindowTitle { get; } = $"{App.APPLICATION_NAME} (tray)";

    public TrayAppWindowActivator(
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        IApplicationThemeSelector themeSelector,
        ISettings settings,
        ILocalizationProvider localizer,
        IApplicationIconSelector iconSelector,
        IMainWindowActivator mainWindowActivator,
        IUserAuthenticator userAuthenticator)
        : base(logger,
               uiThreadDispatcher,
               themeSelector,
               settings,
               localizer,
               iconSelector,
               mainWindowActivator)
    {
        _userAuthenticator = userAuthenticator;
    }

    protected override void InvalidateWindowPosition()
    {
        double width = _userAuthenticator.IsLoggedIn
            ? TRAY_APP_MAIN_WIDTH
            : TRAY_APP_LOGIN_WIDTH;
        double height = _userAuthenticator.IsLoggedIn
            ? Settings.VpnPlan.IsPaid
                ? TRAY_APP_MAIN_HEIGHT
                : TRAY_APP_FREE_MAIN_HEIGHT
            : TRAY_APP_LOGIN_HEIGHT;

        Host?.MoveNearTrayOnPrimaryMonitor(width, height, TRAY_APP_MARGIN);
    }

    protected override void OnWindowFocused()
    {
        base.OnWindowFocused();

        InvalidateWindowPosition();
    }

    protected override void OnWindowUnfocused()
    {
        base.OnWindowUnfocused();

        Hide();
    }
}