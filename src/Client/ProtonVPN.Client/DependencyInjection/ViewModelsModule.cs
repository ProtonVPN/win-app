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
using ProtonVPN.Client.UI;
using ProtonVPN.Client.UI.Countries;
using ProtonVPN.Client.UI.Countries.Pages;
using ProtonVPN.Client.UI.Gallery;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Client.UI.Home.ConnectionCard;
using ProtonVPN.Client.UI.Home.Help;
using ProtonVPN.Client.UI.Home.Map;
using ProtonVPN.Client.UI.Home.NetShieldStats;
using ProtonVPN.Client.UI.Home.Recents;
using ProtonVPN.Client.UI.Home.VpnStatusComponent;
using ProtonVPN.Client.UI.Settings;
using ProtonVPN.Client.UI.Settings.Pages;
using ProtonVPN.Client.UI.Settings.Pages.Advanced;

namespace ProtonVPN.Client.DependencyInjection
{
    public class ViewModelsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RecentsViewModel>().SingleInstance();
            builder.RegisterType<VpnStatusViewModel>().SingleInstance();
            builder.RegisterType<NetShieldStatsViewModel>().SingleInstance();
            builder.RegisterType<ConnectionCardViewModel>().SingleInstance();
            builder.RegisterType<MapViewModel>().SingleInstance();
            builder.RegisterType<HelpViewModel>().SingleInstance();
            builder.RegisterType<SettingsViewModel>().SingleInstance();
            builder.RegisterType<CountriesViewModel>().SingleInstance();
            builder.RegisterType<HomeViewModel>().SingleInstance();
            builder.RegisterType<GalleryViewModel>().SingleInstance();
            builder.RegisterType<ShellViewModel>().SingleInstance();
            builder.RegisterType<CensorshipViewModel>().SingleInstance();
            builder.RegisterType<AutoConnectViewModel>().SingleInstance();
            builder.RegisterType<CustomDnsServersViewModel>().SingleInstance();
            builder.RegisterType<VpnLogsViewModel>().SingleInstance();
            builder.RegisterType<AdvancedSettingsViewModel>().SingleInstance();
            builder.RegisterType<VpnAcceleratorViewModel>().SingleInstance();
            builder.RegisterType<ProtocolViewModel>().SingleInstance();
            builder.RegisterType<SplitTunnelingViewModel>().SingleInstance();
            builder.RegisterType<PortForwardingViewModel>().SingleInstance();
            builder.RegisterType<KillSwitchViewModel>().SingleInstance();
            builder.RegisterType<NetShieldViewModel>().SingleInstance();
            builder.RegisterType<CountryViewModel>().SingleInstance();
        }
    }
}