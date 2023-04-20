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
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ProtonVPN.Api.Contracts.Geographical;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Users;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Gui.Enums;
using ProtonVPN.Gui.Messages;
using ProtonVPN.Gui.ViewModels.Components;

namespace ProtonVPN.Gui.Views.Components;

public sealed partial class VpnStatusComponent : UserControl
{
    public VpnStatusComponent()
    {
        ViewModel = App.GetService<VpnStatusViewModel>();

        InitializeComponent();
    }

    public VpnStatusViewModel ViewModel
    {
        get;
    }

    #region Simulation

    /// This code is only meant for testing purposes
    private Server _server = Server.Empty();

    /// This code is only meant for testing purposes
    protected override void OnPointerPressed(PointerRoutedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed)
        {
            ShuffleUserLocation();
            return;
        }

        SwitchVpnState(e.GetCurrentPoint(this).Properties.IsLeftButtonPressed);
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

        // Simmulate NetShield stats are not enabled is user location is in France or Sweden
        ViewModel.IsNetShieldStatEnabled = country is not ("France" or "Sweden");

        // Simulate secure core connection if user location is in Switzerland or Sweden
        sbyte serverFeatures = country is "Switzerland" or "Sweden"
            ? (sbyte)ServerFeatures.SECURE_CORE
            : (sbyte)ServerFeatures.STANDARD;

        _server = new Server(
                "", "ZZ#0", "", "ZZ", "ZZ", "", 0, 0, serverFeatures, 0, 0,
                new LocationResponse { Lat = 0f, Long = 0f },
                new List<PhysicalServer>(0),
                null);

        WeakReferenceMessenger.Default.Send(new VpnStateChangedMessage(new VpnState(VpnStatus.Disconnected, _server)));
        WeakReferenceMessenger.Default.Send(new UserLocationChangedMessage(new UserLocation(ipAddress, string.Empty, country)));
    }

    /// This code is only meant for testing purposes
    private void SwitchVpnState(bool switchForward)
    {
        VpnStatus status = VpnStatus.Disconnected;

        switch (ViewModel.CurrentConnectionStatus)
        {
            case ConnectionStatus.Disconnected:
                status = switchForward ? VpnStatus.Connecting : VpnStatus.Connected;
                break;

            case ConnectionStatus.Connecting:
                status = switchForward ? VpnStatus.Connected : VpnStatus.Disconnected;
                break;

            case ConnectionStatus.Connected:
                status = switchForward ? VpnStatus.Disconnected : VpnStatus.Connecting;
                break;
        }

        WeakReferenceMessenger.Default.Send(new VpnStateChangedMessage(new VpnState(status, _server)));
    }

    #endregion Simulation
}