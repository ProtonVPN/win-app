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
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Activation.Bases;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Dialogs.Upsell;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Services.Activation;

public class UpsellCarouselWindowActivator : DialogActivatorBase<UpsellCarouselWindow>, IUpsellCarouselWindowActivator
{
    private readonly IUpsellCarouselViewNavigator _upsellCarouselViewNavigator;

    public override string WindowTitle => Localizer.Get("Upsell_Carousel_Title");

    public ModalSources ModalSources { get; private set; } = ModalSources.Undefined;

    public UpsellCarouselWindowActivator(
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        IApplicationThemeSelector themeSelector,
        ISettings settings,
        ILocalizationProvider localizer,
        IApplicationIconSelector iconSelector,
        IMainWindowActivator mainWindowActivator,
        IUpsellCarouselViewNavigator upsellCarouselViewNavigator)
        : base(logger,
               uiThreadDispatcher,
               themeSelector,
               settings,
               localizer,
               iconSelector,
               mainWindowActivator)
    {
        _upsellCarouselViewNavigator = upsellCarouselViewNavigator;
    }

    public Task<bool> ActivateAsync(UpsellFeatureType? upsellFeatureType)
    {
        Activate();

        SetCorrespondingModalSources(upsellFeatureType);

        return _upsellCarouselViewNavigator.NavigateToFeatureViewAsync(upsellFeatureType);
    }

    private void SetCorrespondingModalSources(UpsellFeatureType? upsellFeatureType)
    {
        ModalSources = upsellFeatureType switch
        {
            UpsellFeatureType.WorldwideCoverage => ModalSources.Countries,
            UpsellFeatureType.Speed => ModalSources.VpnAccelerator,
            UpsellFeatureType.Streaming => ModalSources.Streaming,
            UpsellFeatureType.NetShield => ModalSources.NetShield,
            UpsellFeatureType.SecureCore => ModalSources.SecureCore,
            UpsellFeatureType.P2P => ModalSources.P2P,
            UpsellFeatureType.MultipleDevices => ModalSources.MaxConnections,
            UpsellFeatureType.Tor => ModalSources.Countries,
            UpsellFeatureType.SplitTunneling => ModalSources.SplitTunneling,
            UpsellFeatureType.Profiles => ModalSources.Profiles,
            _ => ModalSources.Undefined
        };
    }
}