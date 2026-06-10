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
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Dialogs.Upsell;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.StatisticalEvents.Contracts;

namespace ProtonVPN.Client.Services.Activation;

public class UpsellCarouselWindowActivator : DialogActivatorBase<UpsellCarouselWindow>, IUpsellCarouselWindowActivator,
    IEventMessageReceiver<LoggedOutMessage>
{
    private readonly IUpsellDisplayReporter _upsellDisplayReporter;
    private readonly IUpsellCarouselViewNavigator _upsellCarouselViewNavigator;

    public override string WindowTitle => Localizer.Get("Upsell_Carousel_Title");

    public UpsellModalContext ModalContext { get; private set; } = UpsellModalContext.Undefined;

    public UpsellCarouselWindowActivator(
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        IApplicationThemeSelector themeSelector,
        ISettings settings,
        ILocalizationService localizationService,
        ILocalizationProvider localizer,
        IApplicationIconSelector iconSelector,
        IMainWindowActivator mainWindowActivator,
        IUpsellDisplayReporter upsellDisplayReporter,
        IUpsellCarouselViewNavigator upsellCarouselViewNavigator)
        : base(logger,
               uiThreadDispatcher,
               themeSelector,
               settings,
               localizationService,
               localizer,
               iconSelector,
               mainWindowActivator)
    {
        _upsellDisplayReporter = upsellDisplayReporter;
        _upsellCarouselViewNavigator = upsellCarouselViewNavigator;
    }

    public Task<bool> ActivateAsync(UpsellModalContext context)
    {
        Activate();

        ModalContext = context;

        _upsellDisplayReporter.Report(context);

        UpsellFeatureType? upsellFeatureType = GetUpsellFeatureType(context.Source);

        return _upsellCarouselViewNavigator.NavigateToFeatureViewAsync(upsellFeatureType);
    }

    private UpsellFeatureType? GetUpsellFeatureType(ModalSource? modalSource)
    {
        return modalSource switch
        {
            ModalSource.Countries or
            ModalSource.ChangeServer or
            ModalSource.DefaultConnection or
            ModalSource.ExcludeLocations or
            ModalSource.CarouselCountries => UpsellFeatureType.WorldwideCoverage,

            ModalSource.VpnAccelerator or
            ModalSource.Speed or
            ModalSource.CarouselSpeed => UpsellFeatureType.Speed,

            ModalSource.Streaming or
            ModalSource.StreamingActivity or
            ModalSource.CarouselStreaming => UpsellFeatureType.Streaming,

            ModalSource.NetShield or
            ModalSource.CarouselNetShield => UpsellFeatureType.NetShield,

            ModalSource.SecureCore or
            ModalSource.CarouselSecureCore => UpsellFeatureType.SecureCore,

            ModalSource.P2P or
            ModalSource.P2PActivity or
            ModalSource.PortForwarding or
            ModalSource.CarouselP2P => UpsellFeatureType.P2P,

            ModalSource.MaxConnections or
            ModalSource.CarouselMultipleDevices or
            ModalSource.Account or
            ModalSource.Devices => UpsellFeatureType.MultipleDevices,

            ModalSource.Tor or
            ModalSource.CarouselTor => UpsellFeatureType.Tor,

            ModalSource.SplitTunneling or
            ModalSource.CarouselSplitTunneling => UpsellFeatureType.SplitTunneling,

            ModalSource.Profiles or
            ModalSource.CarouselProfiles => UpsellFeatureType.Profiles,

            ModalSource.CarouselCustomization or
            ModalSource.AdvancedCustomization => UpsellFeatureType.AdvancedSettings,

            ModalSource.CustomDns => UpsellFeatureType.CustomDns,

            ModalSource.AllowLanConnections => UpsellFeatureType.AllowLanConnections,

            ModalSource.ModerateNat => UpsellFeatureType.ModerateNat,

            _ => null
        };
    }

    public void Receive(LoggedOutMessage message)
    {
        Hide();
    }
}