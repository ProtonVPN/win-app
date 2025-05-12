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
    private readonly IUpsellDisplayStatisticalEventSender _upsellDisplayStatisticalEventSender;
    private readonly IUpsellCarouselViewNavigator _upsellCarouselViewNavigator;

    public override string WindowTitle => Localizer.Get("Upsell_Carousel_Title");

    public ModalSource ModalSource { get; private set; } = ModalSource.Undefined;

    public UpsellCarouselWindowActivator(
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        IApplicationThemeSelector themeSelector,
        ISettings settings,
        ILocalizationProvider localizer,
        IApplicationIconSelector iconSelector,
        IMainWindowActivator mainWindowActivator,
        IUpsellDisplayStatisticalEventSender upsellDisplayStatisticalEventSender,
        IUpsellCarouselViewNavigator upsellCarouselViewNavigator)
        : base(logger,
               uiThreadDispatcher,
               themeSelector,
               settings,
               localizer,
               iconSelector,
               mainWindowActivator)
    {
        _upsellDisplayStatisticalEventSender = upsellDisplayStatisticalEventSender;
        _upsellCarouselViewNavigator = upsellCarouselViewNavigator;
    }

    public Task<bool> ActivateAsync(UpsellFeatureType? upsellFeatureType)
    {
        Activate();

        SetCorrespondingModalSources(upsellFeatureType);

        _upsellDisplayStatisticalEventSender.Send(ModalSource);

        return _upsellCarouselViewNavigator.NavigateToFeatureViewAsync(upsellFeatureType);
    }

    private void SetCorrespondingModalSources(UpsellFeatureType? upsellFeatureType)
    {
        ModalSource = upsellFeatureType switch
        {
            UpsellFeatureType.WorldwideCoverage => ModalSource.Countries,
            UpsellFeatureType.Speed => ModalSource.VpnAccelerator,
            UpsellFeatureType.Streaming => ModalSource.Streaming,
            UpsellFeatureType.NetShield => ModalSource.NetShield,
            UpsellFeatureType.SecureCore => ModalSource.SecureCore,
            UpsellFeatureType.P2P => ModalSource.P2P,
            UpsellFeatureType.MultipleDevices => ModalSource.CarouselMultipleDevices,
            UpsellFeatureType.Tor => ModalSource.Tor,
            UpsellFeatureType.SplitTunneling => ModalSource.SplitTunneling,
            UpsellFeatureType.Profiles => ModalSource.Profiles,
            UpsellFeatureType.AdvancedSettings => ModalSource.CarouselCustomization,
            UpsellFeatureType.ModerateNat => ModalSource.ModerateNat,
            UpsellFeatureType.CustomDns => ModalSource.CustomDns,
            UpsellFeatureType.AllowLanConnections => ModalSource.AllowLanConnections,
            _ => ModalSource.Undefined
        };
    }

    public void Receive(LoggedOutMessage message)
    {
        Hide();
    }
}