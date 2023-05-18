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

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Input;
using ProtonVPN.Api.Contracts.Geographical;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Users;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Client.Messages;

namespace ProtonVPN.Client.UI.Home.VpnStatusComponent;

public sealed partial class VpnStatusComponent
{
    public VpnStatusComponent()
    {
        ViewModel = App.GetService<VpnStatusViewModel>();

        InitializeComponent();
    }

    public VpnStatusViewModel ViewModel { get; }

    #region Simulation

    /// This code is only meant for testing purposes
    protected override void OnPointerPressed(PointerRoutedEventArgs e)
    {
        ShuffleUserLocation();
    }

    /// This code is only meant for testing purposes
    private void ShuffleUserLocation()
    {
        List<string> countries = new()
        {
            "France",
            "Italy",
            "Lithuania",
            "Poland",
            "Portugal",
            "Spain",
            "Switzerland",
            "Sweden",
        };

        Random random = new();

        string ipAddress = $"{random.Next(255)}.{random.Next(255)}.{random.Next(255)}.{random.Next(255)}";
        string country = countries.ElementAt(random.Next(countries.Count));

        WeakReferenceMessenger.Default.Send(new UserLocationChangedMessage(new UserLocation(ipAddress, string.Empty, country)));
    }

    #endregion Simulation
}