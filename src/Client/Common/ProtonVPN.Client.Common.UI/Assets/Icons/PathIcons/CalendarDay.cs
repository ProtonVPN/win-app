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

public class CalendarDay : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M11 3H5V1.5a.5.5 0 0 0-1 0V3a3 3 0 0 0-3 3v6a3 3 0 0 0 3 3h8a3 3 0 0 0 3-3V6a3 3 0 0 0-3-3V1.5a.5.5 0 0 0-1 0V3ZM4 4h8a2 2 0 0 1 2 2v6a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2Z";

    protected override string IconGeometry20 { get; }
        = "M13.75 3.75h-7.5V1.875a.625.625 0 1 0-1.25 0V3.75A3.75 3.75 0 0 0 1.25 7.5V15A3.75 3.75 0 0 0 5 18.75h10A3.75 3.75 0 0 0 18.75 15V7.5A3.75 3.75 0 0 0 15 3.75V1.875a.625.625 0 1 0-1.25 0V3.75ZM5 5h10a2.5 2.5 0 0 1 2.5 2.5V15a2.5 2.5 0 0 1-2.5 2.5H5A2.5 2.5 0 0 1 2.5 15V7.5A2.5 2.5 0 0 1 5 5Z"; 

    protected override string IconGeometry24 { get; }
        = "M16.5 4.5h-9V2.25a.75.75 0 0 0-1.5 0V4.5A4.5 4.5 0 0 0 1.5 9v9A4.5 4.5 0 0 0 6 22.5h12a4.5 4.5 0 0 0 4.5-4.5V9A4.5 4.5 0 0 0 18 4.5V2.25a.75.75 0 0 0-1.5 0V4.5ZM6 6h12a3 3 0 0 1 3 3v9a3 3 0 0 1-3 3H6a3 3 0 0 1-3-3V9a3 3 0 0 1 3-3Z"; 

    protected override string IconGeometry32 { get; }
        = "M22 6H10V3a1 1 0 0 0-2 0v3a6 6 0 0 0-6 6v12a6 6 0 0 0 6 6h16a6 6 0 0 0 6-6V12a6 6 0 0 0-6-6V3a1 1 0 1 0-2 0v3ZM8 8h16a4 4 0 0 1 4 4v12a4 4 0 0 1-4 4H8a4 4 0 0 1-4-4V12a4 4 0 0 1 4-4Z";
}