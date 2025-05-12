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

using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Mapping;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Core.Services.Navigation.Bases;
using ProtonVPN.Client.UI.Dialogs.Upsell.Features;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Services.Navigation;

public class UpsellCarouselViewNavigator : ViewNavigatorBase, IUpsellCarouselViewNavigator
{
    public override FrameLoadedBehavior LoadBehavior { get; protected set; } = FrameLoadedBehavior.NavigateToDefaultViewIfEmpty;

    public UpsellCarouselViewNavigator(
        ILogger logger,
        IPageViewMapper pageViewMapper,
        IUIThreadDispatcher uiThreadDispatcher)
        : base(logger, pageViewMapper, uiThreadDispatcher)
    { }

    public Task<bool> NavigateToFeatureViewAsync(UpsellFeatureType? upsellFeatureType)
    {
        return upsellFeatureType switch
        {
            UpsellFeatureType.WorldwideCoverage => NavigateToWorldwideCoverageViewAsync(),
            UpsellFeatureType.Speed => NavigateToSpeedViewAsync(),
            UpsellFeatureType.Streaming => NavigateToStreamingViewAsync(),
            UpsellFeatureType.NetShield => NavigateToNetShieldViewAsync(),
            UpsellFeatureType.SecureCore => NavigateToSecureCoreViewAsync(),
            UpsellFeatureType.P2P => NavigateToP2PViewAsync(),
            UpsellFeatureType.MultipleDevices => NavigateToMultipleDevicesViewAsync(),
            UpsellFeatureType.Tor => NavigateToTorViewAsync(),
            UpsellFeatureType.SplitTunneling => NavigateToSplitTunnelingViewAsync(),
            UpsellFeatureType.Profiles => NavigateToProfilesViewAsync(),
            UpsellFeatureType.AdvancedSettings
                or UpsellFeatureType.CustomDns
                or UpsellFeatureType.ModerateNat
                or UpsellFeatureType.AllowLanConnections => NavigateToAdvancedSettingsViewAsync(),
            _ => NavigateToDefaultAsync(),
        };
    }

    public override Task<bool> NavigateToDefaultAsync()
    {
        return NavigateToFeatureViewAsync(UpsellFeatureType.WorldwideCoverage);
    }

    private Task<bool> NavigateToWorldwideCoverageViewAsync()
    {
        return NavigateToAsync<WorldwideCoverageUpsellFeaturePageViewModel>();
    }

    private Task<bool> NavigateToSpeedViewAsync()
    {
        return NavigateToAsync<SpeedUpsellFeaturePageViewModel>();
    }

    private Task<bool> NavigateToStreamingViewAsync()
    {
        return NavigateToAsync<StreamingUpsellFeaturePageViewModel>();
    }

    private Task<bool> NavigateToNetShieldViewAsync()
    {
        return NavigateToAsync<NetShieldUpsellFeaturePageViewModel>();
    }

    private Task<bool> NavigateToSecureCoreViewAsync()
    {
        return NavigateToAsync<SecureCoreUpsellFeaturePageViewModel>();
    }

    private Task<bool> NavigateToP2PViewAsync()
    {
        return NavigateToAsync<P2PUpsellFeaturePageViewModel>();
    }

    private Task<bool> NavigateToMultipleDevicesViewAsync()
    {
        return NavigateToAsync<MultipleDevicesUpsellFeaturePageViewModel>();
    }

    private Task<bool> NavigateToTorViewAsync()
    {
        return NavigateToAsync<TorUpsellFeaturePageViewModel>();
    }

    private Task<bool> NavigateToSplitTunnelingViewAsync()
    {
        return NavigateToAsync<SplitTunnelingUpsellFeaturePageViewModel>();
    }

    private Task<bool> NavigateToProfilesViewAsync()
    {
        return NavigateToAsync<ProfilesUpsellFeaturePageViewModel>();
    }

    private Task<bool> NavigateToAdvancedSettingsViewAsync()
    {
        return NavigateToAsync<AdvancedSettingsUpsellFeaturePageViewModel>();
    }
}