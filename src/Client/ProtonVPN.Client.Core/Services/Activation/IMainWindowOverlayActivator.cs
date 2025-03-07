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
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Core.Services.Activation.Bases;

namespace ProtonVPN.Client.Core.Services.Activation;

public interface IMainWindowOverlayActivator : IOverlayActivator
{
    Task<ContentDialogResult> ShowHumanVerificationOverlayAsync();

    Task<ContentDialogResult> ShowSecureCoreInfoOverlayAsync();

    Task<ContentDialogResult> ShowP2PInfoOverlayAsync();

    Task<ContentDialogResult> ShowTorInfoOverlayAsync();

    Task<ContentDialogResult> ShowSmartRoutingInfoOverlayAsync();

    Task<ContentDialogResult> ShowProfileInfoOverlayAsync();

    Task<ContentDialogResult> ShowServerLoadInfoOverlayAsync();

    Task<ContentDialogResult> ShowDiscardConfirmationOverlayAsync();

    Task<ContentDialogResult> ShowSettingsOverriddenByProfileOverlayAsync(string localizedSettingsName);

    Task<ContentDialogResult> ShowOutdatedClientOverlayAsync();

    Task<ContentDialogResult> ShowWelcomeOverlayAsync();

    Task<ContentDialogResult> ShowWelcomeToVpnB2BOverlayAsync();

    Task<ContentDialogResult> ShowWelcomeToVpnPlusOverlayAsync();

    Task<ContentDialogResult> ShowWelcomeToVpnUnlimitedOverlayAsync();

    Task<ContentDialogResult> ShowFreeConnectionsOverlayAsync();

    Task<ContentDialogResult> ShowWhatsNewOverlayAsync();
}