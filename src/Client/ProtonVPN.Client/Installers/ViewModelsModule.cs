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

using Autofac;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.UI;
using ProtonVPN.Client.UI.Countries;
using ProtonVPN.Client.UI.Countries.Pages;
using ProtonVPN.Client.UI.Dialogs.Overlays;
using ProtonVPN.Client.UI.Gallery;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Client.UI.Home.ConnectionCard;
using ProtonVPN.Client.UI.Home.Details;
using ProtonVPN.Client.UI.Home.Help;
using ProtonVPN.Client.UI.Home.Map;
using ProtonVPN.Client.UI.Home.Recents;
using ProtonVPN.Client.UI.Home.Status;
using ProtonVPN.Client.UI.Login;
using ProtonVPN.Client.UI.Login.Forms;
using ProtonVPN.Client.UI.Settings;
using ProtonVPN.Client.UI.Settings.Pages;
using ProtonVPN.Client.UI.Settings.Pages.Advanced;

namespace ProtonVPN.Client.Installers;

public class ViewModelsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        RegisterViewModel<ProtocolOverlayViewModel>(builder);
        RegisterViewModel<LatencyOverlayViewModel>(builder);
        RegisterViewModel<ServerLoadOverlayViewModel>(builder);
        RegisterViewModel<VpnSpeedViewModel>(builder);
        RegisterViewModel<IpAddressViewModel>(builder);
        RegisterViewModel<ConnectionDetailsViewModel>(builder);
        RegisterViewModel<RecentsViewModel>(builder);
        RegisterViewModel<VpnStatusViewModel>(builder);
        RegisterViewModel<NetShieldStatsViewModel>(builder);
        RegisterViewModel<ConnectionCardViewModel>(builder);
        RegisterViewModel<MapViewModel>(builder);
        RegisterViewModel<HelpViewModel>(builder);
        RegisterViewModel<SettingsViewModel>(builder);
        RegisterViewModel<CountriesViewModel>(builder);
        RegisterViewModel<HomeViewModel>(builder);
        RegisterViewModel<ShellViewModel>(builder);
        RegisterViewModel<CensorshipViewModel>(builder);
        RegisterViewModel<AutoStartupViewModel>(builder);
        RegisterViewModel<CustomDnsServersViewModel>(builder);
        RegisterViewModel<DebugLogsViewModel>(builder);
        RegisterViewModel<AdvancedSettingsViewModel>(builder);
        RegisterViewModel<VpnAcceleratorViewModel>(builder);
        RegisterViewModel<ProtocolViewModel>(builder);
        RegisterViewModel<SplitTunnelingViewModel>(builder);
        RegisterViewModel<PortForwardingViewModel>(builder);
        RegisterViewModel<KillSwitchViewModel>(builder);
        RegisterViewModel<NetShieldViewModel>(builder);
        RegisterViewModel<CountryViewModel>(builder);
        RegisterViewModel<LoginViewModel>(builder);
        RegisterViewModel<LoginFormViewModel>(builder);
        RegisterViewModel<TwoFactorFormViewModel>(builder);

        RegisterViewModel<GalleryViewModel>(builder);
        RegisterViewModel<GalleryItemViewModel>(builder);
    }

    private void RegisterViewModel<TType>(ContainerBuilder builder)
        where TType : notnull
    {
        builder.RegisterType<TType>().AsSelf().As<IEventMessageReceiver>().SingleInstance();
    }
}