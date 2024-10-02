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

using ProtonVPN.Client.Legacy.UI.Upsell.Carousel.Features;
using ProtonVPN.Client.Legacy.UI.Upsell.Carousel.Features.Base;
using ProtonVPN.Client.Legacy.UI.Upsell.Carousel.Models;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Legacy.Models.Navigation;

public class UpsellCarouselViewNavigator : ViewNavigatorBase, IUpsellCarouselViewNavigator
{
    public UpsellCarouselViewNavigator( 
        ILogger logger,
        IViewMapper viewMapper) 
        : base(logger, viewMapper)
    { }

    public Task NavigateToFeatureAsync(UpsellFeature feature)
    {
        switch (feature)
        {
            case UpsellFeature.WorldwideCoverage:
                return NavigateToFeatureAsync<WorldwideCoverageUpsellFeatureViewModel>();
            case UpsellFeature.Speed:
                return NavigateToFeatureAsync<SpeedUpsellFeatureViewModel>();
            case UpsellFeature.Streaming:
                return NavigateToFeatureAsync<StreamingUpsellFeatureViewModel>();
            case UpsellFeature.NetShield:
                return NavigateToFeatureAsync<NetShieldUpsellFeatureViewModel>();
            case UpsellFeature.SecureCore:
                return NavigateToFeatureAsync<SecureCoreUpsellFeatureViewModel>();
            case UpsellFeature.P2P:
                return NavigateToFeatureAsync<P2PUpsellFeatureViewModel>();
            case UpsellFeature.MultipleDevices:
                return NavigateToFeatureAsync<MultipleDevicesUpsellFeatureViewModel>();
            case UpsellFeature.Tor:
                return NavigateToFeatureAsync<TorUpsellFeatureViewModel>();
            case UpsellFeature.SplitTunneling:
                return NavigateToFeatureAsync<SplitTunnelingUpsellFeatureViewModel>();
            case UpsellFeature.AdvancedSettings:
                return NavigateToFeatureAsync<AdvancedSettingsUpsellFeatureViewModel>();
            default:
                throw new InvalidOperationException($"Upsell feature '{feature}' is not recognized.");
        }
    }

    private Task NavigateToFeatureAsync<TViewModel>() 
        where TViewModel : UpsellFeatureViewModelBase
    {
        return NavigateToAsync<TViewModel>();
    }
}