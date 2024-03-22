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

public class CalendarRow : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M3 1.5v.67A3.001 3.001 0 0 0 1 5v7a3 3 0 0 0 3 3h8a3 3 0 0 0 3-3V5a3.001 3.001 0 0 0-2-2.83V1.5a.5.5 0 0 0-1 0V2H4v-.5a.5.5 0 0 0-1 0ZM12 3H4a2 2 0 0 0-2 2h12a2 2 0 0 0-2-2Zm2 3H2v6a2 2 0 0 0 2 2h8a2 2 0 0 0 2-2V6Zm-2.5 2h-7a.5.5 0 0 0 0 1h7a.5.5 0 0 0 0-1Zm-7-1a1.5 1.5 0 1 0 0 3h7a1.5 1.5 0 0 0 0-3h-7Z";

    protected override string IconGeometry20 { get; }
        = "M3.75 1.875v.838a3.752 3.752 0 0 0-2.5 3.537V15A3.75 3.75 0 0 0 5 18.75h10A3.75 3.75 0 0 0 18.75 15V6.25a3.752 3.752 0 0 0-2.5-3.537v-.838a.625.625 0 1 0-1.25 0V2.5H5v-.625a.625.625 0 1 0-1.25 0ZM15 3.75H5a2.5 2.5 0 0 0-2.5 2.5h15a2.5 2.5 0 0 0-2.5-2.5Zm2.5 3.75h-15V15A2.5 2.5 0 0 0 5 17.5h10a2.5 2.5 0 0 0 2.5-2.5V7.5ZM14.375 10h-8.75a.625.625 0 1 0 0 1.25h8.75a.625.625 0 1 0 0-1.25Zm-8.75-1.25a1.875 1.875 0 0 0 0 3.75h8.75a1.875 1.875 0 0 0 0-3.75h-8.75Z"; 

    protected override string IconGeometry24 { get; }
        = "M4.5 2.25v1.006a4.502 4.502 0 0 0-3 4.244V18A4.5 4.5 0 0 0 6 22.5h12a4.5 4.5 0 0 0 4.5-4.5V7.5a4.502 4.502 0 0 0-3-4.244V2.25a.75.75 0 0 0-1.5 0V3H6v-.75a.75.75 0 0 0-1.5 0ZM18 4.5H6a3 3 0 0 0-3 3h18a3 3 0 0 0-3-3ZM21 9H3v9a3 3 0 0 0 3 3h12a3 3 0 0 0 3-3V9Zm-3.75 3H6.75a.75.75 0 0 0 0 1.5h10.5a.75.75 0 0 0 0-1.5Zm-10.5-1.5a2.25 2.25 0 0 0 0 4.5h10.5a2.25 2.25 0 0 0 0-4.5H6.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M6 3v1.341C3.67 5.165 2 7.388 2 10v14a6 6 0 0 0 6 6h16a6 6 0 0 0 6-6V10a6.002 6.002 0 0 0-4-5.659V3a1 1 0 1 0-2 0v1H8V3a1 1 0 0 0-2 0Zm18 3H8a4 4 0 0 0-4 4h24a4 4 0 0 0-4-4Zm4 6H4v12a4 4 0 0 0 4 4h16a4 4 0 0 0 4-4V12Zm-5 4H9a1 1 0 1 0 0 2h14a1 1 0 1 0 0-2ZM9 14a3 3 0 1 0 0 6h14a3 3 0 1 0 0-6H9Z";
}