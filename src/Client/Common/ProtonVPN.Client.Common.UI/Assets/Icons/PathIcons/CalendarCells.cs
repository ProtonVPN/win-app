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

public class CalendarCells : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M3 1.5v.67A3.001 3.001 0 0 0 1 5v7a3 3 0 0 0 3 3h8a3 3 0 0 0 3-3V5a3.001 3.001 0 0 0-2-2.83V1.5a.5.5 0 0 0-1 0V2H4v-.5a.5.5 0 0 0-1 0ZM12 3H4a2 2 0 0 0-2 2h12a2 2 0 0 0-2-2ZM2 12V6h12v6a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2Zm5.5-3a.5.5 0 0 0 0 1h1a.5.5 0 0 0 0-1h-1Zm3 .5A.5.5 0 0 1 11 9h1a.5.5 0 0 1 0 1h-1a.5.5 0 0 1-.5-.5ZM4 9a.5.5 0 0 0 0 1h1a.5.5 0 0 0 0-1H4Z";

    protected override string IconGeometry20 { get; }
        = "M3.75 1.875v.838a3.752 3.752 0 0 0-2.5 3.537V15A3.75 3.75 0 0 0 5 18.75h10A3.75 3.75 0 0 0 18.75 15V6.25a3.752 3.752 0 0 0-2.5-3.537v-.838a.625.625 0 1 0-1.25 0V2.5H5v-.625a.625.625 0 1 0-1.25 0ZM15 3.75H5a2.5 2.5 0 0 0-2.5 2.5h15a2.5 2.5 0 0 0-2.5-2.5ZM2.5 15V7.5h15V15a2.5 2.5 0 0 1-2.5 2.5H5A2.5 2.5 0 0 1 2.5 15Zm6.875-3.75a.625.625 0 1 0 0 1.25h1.25a.625.625 0 1 0 0-1.25h-1.25Zm3.75.625c0-.345.28-.625.625-.625H15a.625.625 0 1 1 0 1.25h-1.25a.625.625 0 0 1-.625-.625ZM5 11.25a.625.625 0 1 0 0 1.25h1.25a.625.625 0 1 0 0-1.25H5Z"; 

    protected override string IconGeometry24 { get; }
        = "M4.5 2.25v1.006a4.502 4.502 0 0 0-3 4.244V18A4.5 4.5 0 0 0 6 22.5h12a4.5 4.5 0 0 0 4.5-4.5V7.5a4.502 4.502 0 0 0-3-4.244V2.25a.75.75 0 0 0-1.5 0V3H6v-.75a.75.75 0 0 0-1.5 0ZM18 4.5H6a3 3 0 0 0-3 3h18a3 3 0 0 0-3-3ZM3 18V9h18v9a3 3 0 0 1-3 3H6a3 3 0 0 1-3-3Zm8.25-4.5a.75.75 0 0 0 0 1.5h1.5a.75.75 0 0 0 0-1.5h-1.5Zm4.5.75a.75.75 0 0 1 .75-.75H18a.75.75 0 0 1 0 1.5h-1.5a.75.75 0 0 1-.75-.75ZM6 13.5A.75.75 0 0 0 6 15h1.5a.75.75 0 0 0 0-1.5H6Z"; 

    protected override string IconGeometry32 { get; }
        = "M6 3v1.341C3.67 5.165 2 7.388 2 10v14a6 6 0 0 0 6 6h16a6 6 0 0 0 6-6V10a6.002 6.002 0 0 0-4-5.659V3a1 1 0 1 0-2 0v1H8V3a1 1 0 0 0-2 0Zm18 3H8a4 4 0 0 0-4 4h24a4 4 0 0 0-4-4ZM4 24V12h24v12a4 4 0 0 1-4 4H8a4 4 0 0 1-4-4Zm11-6a1 1 0 1 0 0 2h2a1 1 0 1 0 0-2h-2Zm6 1a1 1 0 0 1 1-1h2a1 1 0 1 1 0 2h-2a1 1 0 0 1-1-1ZM8 18a1 1 0 1 0 0 2h2a1 1 0 1 0 0-2H8Z";
}