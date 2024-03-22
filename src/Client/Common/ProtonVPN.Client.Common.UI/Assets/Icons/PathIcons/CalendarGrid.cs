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

using ProtonVPN.Client.Common.UI.Assets.Icons.Base;

namespace ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;

public class CalendarGrid : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M3 1.5v.67A3.001 3.001 0 0 0 1 5v7a3 3 0 0 0 3 3h8a3 3 0 0 0 3-3V5a3.001 3.001 0 0 0-2-2.83V1.5a.5.5 0 0 0-1 0V2H4v-.5a.5.5 0 0 0-1 0ZM4 3h8a2 2 0 0 1 2 2H2a2 2 0 0 1 2-2Zm6 3H6v2h4V6Zm0 3H6v2h4V9Zm1 2V9h3v2h-3Zm-1 1H6v2h4v-2Zm1 2v-2h3a2 2 0 0 1-2 2h-1Zm0-6V6h3v2h-3ZM2 6h3v2H2V6Zm0 3h3v2H2V9Zm0 3h3v2H4a2 2 0 0 1-2-2Z";

    protected override string IconGeometry20 { get; }
        = "M3.75 1.875v.838a3.752 3.752 0 0 0-2.5 3.537V15A3.75 3.75 0 0 0 5 18.75h10A3.75 3.75 0 0 0 18.75 15V6.25a3.752 3.752 0 0 0-2.5-3.537v-.838a.625.625 0 1 0-1.25 0V2.5H5v-.625a.625.625 0 1 0-1.25 0ZM5 3.75h10a2.5 2.5 0 0 1 2.5 2.5h-15A2.5 2.5 0 0 1 5 3.75Zm7.5 3.75h-5V10h5V7.5Zm0 3.75h-5v2.5h5v-2.5Zm1.25 2.5v-2.5h3.75v2.5h-3.75ZM12.5 15h-5v2.5h5V15Zm1.25 2.5V15h3.75a2.5 2.5 0 0 1-2.5 2.5h-1.25Zm0-7.5V7.5h3.75V10h-3.75ZM2.5 7.5h3.75V10H2.5V7.5Zm0 3.75h3.75v2.5H2.5v-2.5Zm0 3.75h3.75v2.5H5A2.5 2.5 0 0 1 2.5 15Z"; 

    protected override string IconGeometry24 { get; }
        = "M4.5 2.25v1.006a4.502 4.502 0 0 0-3 4.244V18A4.5 4.5 0 0 0 6 22.5h12a4.5 4.5 0 0 0 4.5-4.5V7.5a4.502 4.502 0 0 0-3-4.244V2.25a.75.75 0 0 0-1.5 0V3H6v-.75a.75.75 0 0 0-1.5 0ZM6 4.5h12a3 3 0 0 1 3 3H3a3 3 0 0 1 3-3ZM15 9H9v3h6V9Zm0 4.5H9v3h6v-3Zm1.5 3v-3H21v3h-4.5ZM15 18H9v3h6v-3Zm1.5 3v-3H21a3 3 0 0 1-3 3h-1.5Zm0-9V9H21v3h-4.5ZM3 9h4.5v3H3V9Zm0 4.5h4.5v3H3v-3ZM3 18h4.5v3H6a3 3 0 0 1-3-3Z"; 

    protected override string IconGeometry32 { get; }
        = "M6 3v1.341C3.67 5.165 2 7.388 2 10v14a6 6 0 0 0 6 6h16a6 6 0 0 0 6-6V10a6.002 6.002 0 0 0-4-5.659V3a1 1 0 1 0-2 0v1H8V3a1 1 0 0 0-2 0Zm2 3h16a4 4 0 0 1 4 4H4a4 4 0 0 1 4-4Zm12 6h-8v4h8v-4Zm0 6h-8v4h8v-4Zm2 4v-4h6v4h-6Zm-2 2h-8v4h8v-4Zm2 4v-4h6a4 4 0 0 1-4 4h-2Zm0-12v-4h6v4h-6ZM4 12h6v4H4v-4Zm0 6h6v4H4v-4Zm0 6h6v4H8a4 4 0 0 1-4-4Z";
}