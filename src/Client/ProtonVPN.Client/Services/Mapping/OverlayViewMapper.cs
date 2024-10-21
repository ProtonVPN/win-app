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
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.Contracts.Services.Mapping;
using ProtonVPN.Client.Services.Mapping.Bases;
using ProtonVPN.Client.UI.Login.Overlays;
using ProtonVPN.Client.UI.Overlays.HumanVerification;
using ProtonVPN.Client.UI.Overlays.Information;
using ProtonVPN.Client.UI.Overlays.Information.Notification;
using ProtonVPN.Client.UI.Overlays.Welcome;
using ProtonVPN.Client.UI.Overlays.WhatsNew;

namespace ProtonVPN.Client.Services.Mapping;

public class OverlayViewMapper : ViewMapperBase<OverlayViewModelBase, ContentDialog>, IOverlayViewMapper
{
    protected override void ConfigureMappings()
    {
        ConfigureMapping<HumanVerificationOverlayViewModel, HumanVerificationOverlayView>();

        ConfigureMapping<P2POverlayViewModel, P2POverlayView>();
        ConfigureMapping<SecureCoreOverlayViewModel, SecureCoreOverlayView>();
        ConfigureMapping<TorOverlayViewModel, TorOverlayView>();
        ConfigureMapping<SmartRoutingOverlayViewModel, SmartRoutingOverlayView>();
        ConfigureMapping<ServerLoadOverlayViewModel, ServerLoadOverlayView>();
        ConfigureMapping<TroubleshootingOverlayViewModel, TroubleshootingOverlayView>();
        ConfigureMapping<SsoLoginOverlayViewModel, SsoLoginOverlayView>();
        ConfigureMapping<OutdatedClientOverlayViewModel, OutdatedClientOverlayView>();

        ConfigureMapping<WelcomeOverlayViewModel, WelcomeOverlayView>();
        ConfigureMapping<WelcomeToVpnPlusOverlayViewModel, WelcomeToVpnPlusOverlayView>();
        ConfigureMapping<WelcomeToVpnUnlimitedOverlayViewModel, WelcomeToVpnUnlimitedOverlayView>();
        ConfigureMapping<WelcomeToVpnB2BOverlayViewModel, WelcomeToVpnB2BOverlayView>();

        ConfigureMapping<WhatsNewB2BOverlayViewModel, WhatsNewB2BOverlayView>();
        ConfigureMapping<WhatsNewFreeOverlayViewModel, WhatsNewFreeOverlayView>();
        ConfigureMapping<WhatsNewPaidOverlayViewModel, WhatsNewPaidOverlayView>();
    }
}