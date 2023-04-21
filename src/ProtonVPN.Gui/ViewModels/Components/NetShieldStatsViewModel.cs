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

using CommunityToolkit.Mvvm.ComponentModel;

namespace ProtonVPN.Gui.ViewModels.Components;

public partial class NetShieldStatsViewModel : ObservableRecipient
{
    [ObservableProperty]
    private int _numberOfTrackersStopped;

    [ObservableProperty]
    private int _numberOfAdsBlocked;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DataSaved))]
    private long _dataSavedInBytes;

    public NetShieldStatsViewModel()
    {
        _numberOfTrackersStopped = 14;
        _numberOfAdsBlocked = 21;
        _dataSavedInBytes = 1500;
    }

    public string DataSaved => $"{DataSavedInBytes} B";
}