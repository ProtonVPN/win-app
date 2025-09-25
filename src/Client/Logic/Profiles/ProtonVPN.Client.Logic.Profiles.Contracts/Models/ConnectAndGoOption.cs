/*
 * Copyright (c) 2025 Proton AG
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

using ProtonVPN.Client.Common.Enums;

namespace ProtonVPN.Client.Logic.Profiles.Contracts.Models;

public class ConnectAndGoOption : IConnectAndGoOption
{
    public static IConnectAndGoOption Default => new ConnectAndGoOption()
    {
        IsEnabled = DefaultProfileSettings.IsConnectAndGoEnabled,
        Mode = DefaultProfileSettings.ConnectAndGoMode,
    };

    public bool IsEnabled { get; set; }

    public ConnectAndGoMode Mode { get; set; }

    public bool UsePrivateBrowsingMode { get; set; } = false;

    public string Url { get; set; } = string.Empty;

    public string? AppPath { get; set; }

    public IConnectAndGoOption Copy()
    {
        return (IConnectAndGoOption)MemberwiseClone();
    }
}