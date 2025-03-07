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
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Activation.Bases;
using ProtonVPN.Client.Core.Services.Mapping;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Overlays.HumanVerification;
using ProtonVPN.Client.UI.Overlays.Information;
using ProtonVPN.Client.UI.Overlays.Information.Notification;
using ProtonVPN.Client.UI.Overlays.WhatsNew;
using ProtonVPN.Client.UI.Overlays.Upsell;
using ProtonVPN.Client.UI.Overlays.Welcome;
using ProtonVPN.Logging.Contracts;

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

    public Task<ContentDialogResult> ShowDiscardConfirmationOverlayAsync()
    {
        return ShowMessageAsync(
            new MessageDialogParameters
            {
                Title = _localizer.Get("Settings_DiscardChanges_Confirmation_Title"),
                PrimaryButtonText = _localizer.Get("Settings_DiscardChanges_Confirmation_Action"),
                CloseButtonText = _localizer.Get("Settings_DiscardChanges_Confirmation_Cancel"),
            });
    }

    public Task<ContentDialogResult> ShowSettingsOverriddenByProfileOverlayAsync(string localizedSettingsName)
    {
        return ShowMessageAsync(
            new MessageDialogParameters
            {
                Title = _localizer.GetFormat("Settings_OverriddenByProfile_Title", localizedSettingsName),
                Message = $"{_localizer.Get("Settings_OverriddenByProfile_Description1")}{Environment.NewLine}{Environment.NewLine}{_localizer.Get("Settings_OverriddenByProfile_Description2")}" ,
                PrimaryButtonText = _localizer.Get("Connection_Error_EditProfile"),
                CloseButtonText = _localizer.Get("Common_Actions_Close"),
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

    public Task<ContentDialogResult> ShowSmartRoutingInfoOverlayAsync()
    {
        return ShowOverlayAsync<SmartRoutingOverlayViewModel>();
    }

    public Task<ContentDialogResult> ShowProfileInfoOverlayAsync()
    {
        return ShowOverlayAsync<ProfileOverlayViewModel>();
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

    public Task<ContentDialogResult> ShowFreeConnectionsOverlayAsync()
    {
        return ShowOverlayAsync<FreeConnectionsOverlayViewModel>();
    }

    public Task<ContentDialogResult> ShowWhatsNewOverlayAsync()
    {
        return ShowOverlayAsync<WhatsNewOverlayViewModel>();
    }
}