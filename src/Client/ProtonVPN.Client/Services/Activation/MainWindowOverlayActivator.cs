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

using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Contracts.Services.Activation.Bases;
using ProtonVPN.Client.Contracts.Services.Mapping;
using ProtonVPN.Client.Contracts.Services.Selection;
using ProtonVPN.Client.UI.Overlays.HumanVerification;
using ProtonVPN.Client.UI.Overlays.Information;
using ProtonVPN.Client.UI.Overlays.Information.Notification;
using ProtonVPN.Client.UI.Overlays.Welcome;
using ProtonVPN.Client.UI.Overlays.WhatsNew;

namespace ProtonVPN.Client.Services.Activation;

public class MainWindowOverlayActivator : OverlayActivatorBase<MainWindow>, IMainWindowOverlayActivator
{
    private readonly ILocalizationProvider _localizer;

    public MainWindowOverlayActivator(
        ILocalizationProvider localizer,
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        IApplicationThemeSelector themeSelector,
        ISettings settings,
        IOverlayViewMapper overlayViewMapper)
        : base(logger, uiThreadDispatcher, themeSelector, settings, overlayViewMapper)
    {
        _localizer = localizer;
    }

    public Task<ContentDialogResult> ShowHumanVerificationOverlayAsync()
    {
        return ShowOverlayAsync<HumanVerificationOverlayViewModel>();
    }

    public Task<ContentDialogResult> ShowSecureCoreInfoOverlayAsync()
    {
        return ShowOverlayAsync<SecureCoreOverlayViewModel>();
    }

    public Task<ContentDialogResult> ShowTroubleshootingOverlayAsync()
    {
        return ShowOverlayAsync<TroubleshootingOverlayViewModel>();
    }

    public Task<ContentDialogResult> ShowSettingsDiscardOverlayAsync(bool isReconnectionRequired)
    {
        return ShowMessageAsync(
            new MessageDialogParameters
            {
                Title = _localizer.Get("Settings_Common_Discard_Title"),
                PrimaryButtonText = _localizer.Get("Settings_Common_Discard"),
                SecondaryButtonText = _localizer.Get(isReconnectionRequired
                    ? "Settings_Common_ApplyAndReconnect"
                    : "Settings_Common_Apply"),
                CloseButtonText = _localizer.Get("Common_Actions_Cancel"),
                UseVerticalLayoutForButtons = true,
            });
    }

    public Task<ContentDialogResult> ShowP2PInfoOverlayAsync()
    {
        return ShowOverlayAsync<P2POverlayViewModel>();
    }

    public Task<ContentDialogResult> ShowTorInfoOverlayAsync()
    {
        return ShowOverlayAsync<TorOverlayViewModel>();
    }

    public Task<ContentDialogResult> ShowProfilesInfoOverlayAsync()
    {
        return ShowMessageAsync(new()
        {
            Title = "Profiles Info overlay", Message = "TODO", CloseButtonText = "Close",
        });
    }

    public Task<ContentDialogResult> ShowSmartRoutingInfoOverlayAsync()
    {
        return ShowOverlayAsync<SmartRoutingOverlayViewModel>();
    }

    public Task<ContentDialogResult> ShowServerLoadInfoOverlayAsync()
    {
        return ShowOverlayAsync<ServerLoadOverlayViewModel>();
    }

    public Task<ContentDialogResult> ShowOutdatedClientOverlayAsync()
    {
        return ShowOverlayAsync<OutdatedClientOverlayViewModel>();
    }

    public Task<ContentDialogResult> ShowWelcomeOverlayAsync()
    {
        return ShowOverlayAsync<WelcomeOverlayViewModel>();
    }

    public Task<ContentDialogResult> ShowWelcomeToVpnB2BOverlayAsync()
    {
        return ShowOverlayAsync<WelcomeToVpnB2BOverlayViewModel>();
    }

    public Task<ContentDialogResult> ShowWelcomeToVpnPlusOverlayAsync()
    {
        return ShowOverlayAsync<WelcomeToVpnPlusOverlayViewModel>();
    }

    public Task<ContentDialogResult> ShowWelcomeToVpnUnlimitedOverlayAsync()
    {
        return ShowOverlayAsync<WelcomeToVpnUnlimitedOverlayViewModel>();
    }

    public Task<ContentDialogResult> ShowWhatsNewB2BOverlayAsync()
    {
        return ShowOverlayAsync<WhatsNewB2BOverlayViewModel>();
    }

    public Task<ContentDialogResult> ShowWhatsNewFreeOverlayAsync()
    {
        return ShowOverlayAsync<WhatsNewFreeOverlayViewModel>();
    }

    public Task<ContentDialogResult> ShowWhatsNewPaidOverlayAsync()
    {
        return ShowOverlayAsync<WhatsNewPaidOverlayViewModel>();
    }
}