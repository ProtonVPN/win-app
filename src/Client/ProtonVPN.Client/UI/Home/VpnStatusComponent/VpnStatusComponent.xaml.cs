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
using ProtonVPN.Client.Messages;
using ProtonVPN.Core.Users;

namespace ProtonVPN.Client.UI.Home.VpnStatusComponent;

public sealed partial class VpnStatusComponent
{
    public VpnStatusViewModel ViewModel { get; }

    public VpnStatusComponent()
    {
        ViewModel = App.GetService<VpnStatusViewModel>();

        InitializeComponent();
    }

    #region Simulation

    /// This code is only meant for testing purposes
    protected override void OnPointerPressed(PointerRoutedEventArgs e)
    {
        ShuffleUserLocation();
    }

    /// This code is only meant for testing purposes
    private void ShuffleUserLocation()
    {
        Random random = new();

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

        // Generate random user location
        string ipAddress = $"{random.Next(255)}.{random.Next(255)}.{random.Next(255)}.{random.Next(255)}";
        string country = countries.ElementAt(random.Next(countries.Count));

        // Simulate unknown user location
        UserLocation userLocation = random.Next(10) switch
        {
            0 => new UserLocation(string.Empty, string.Empty, string.Empty),// Simulate unknown user location
            1 => new UserLocation(ipAddress, string.Empty, string.Empty),// Simulate unknown country location but known ip
            2 => new UserLocation(string.Empty, string.Empty, country),// Simulate unknown ip but known country location
            _ => new UserLocation(ipAddress, string.Empty, country),
        };
        WeakReferenceMessenger.Default.Send(new UserLocationChangedMessage(userLocation));
    }

    #endregion Simulation
}