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

using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;

namespace ProtonVPN.Client.Common.UI.Gallery.Pages;

public sealed partial class VpnSpecificPage
{
    internal ObservableCollection<FakeIntent> FakeIntents { get; }

    public VpnSpecificPage()
    {
        FakeIntents = new ObservableCollection<FakeIntent>()
        {
            new FakeIntent()
            {
                ExitCountryCode = "FR",
                CountryName = "France",
                PrimaryButton = "Connect",
                SecondaryButton = "Cities",
                SecondaryIcon = new ChevronRight(),
            },
            new FakeIntent()
            {
                ExitCountryCode = "LT",
                CountryName = "Lithuania",
                PrimaryButton = "Connect",
                SecondaryIcon = new ThreeDotsVertical(),
            },
            new FakeIntent()
            {
                ExitCountryCode = "CH",
                GatewayName = "Proton",
                PrimaryButton = "Connect",
            },
            new FakeIntent()
            {
                ExitCountryCode = "AU",
                EntryCountryCode = "SE",
                CountryName = "Australia via Sweden",
                PrimaryButton = "Connect",
            },
        };

        InitializeComponent();
    }
}

internal class FakeIntent
{
    public string EntryCountryCode { get; set; }
    public string ExitCountryCode { get; set; }
    public string GatewayName { get; set; }
    public string CountryName { get; set; }
    public string Title => IsGateway ? GatewayName : CountryName;
    public string PrimaryButton { get; set; }
    public string SecondaryButton { get; set; }
    public IconElement SecondaryIcon { get; set; }
    public bool IsSecureCore => !string.IsNullOrEmpty(EntryCountryCode) && !string.IsNullOrEmpty(ExitCountryCode);
    public bool IsGateway => !string.IsNullOrEmpty(GatewayName);
}
