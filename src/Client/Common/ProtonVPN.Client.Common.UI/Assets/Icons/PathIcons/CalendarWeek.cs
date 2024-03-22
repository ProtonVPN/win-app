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

public class CalendarWeek : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M11 3H5V1.5a.5.5 0 0 0-1 0V3a3 3 0 0 0-3 3v6a3 3 0 0 0 3 3h8a3 3 0 0 0 3-3V6a3 3 0 0 0-3-3V1.5a.5.5 0 0 0-1 0V3ZM4 4h2v10H4V4Zm3 10h2V4H7v10ZM3 4.268v9.464A2 2 0 0 1 2 12V6a2 2 0 0 1 1-1.732ZM10 14V4h2v10h-2Zm3-.268A2 2 0 0 0 14 12V6a2 2 0 0 0-1-1.732v9.464Z";

    protected override string IconGeometry20 { get; }
        = "M13.75 3.75h-7.5V1.875a.625.625 0 1 0-1.25 0V3.75A3.75 3.75 0 0 0 1.25 7.5V15A3.75 3.75 0 0 0 5 18.75h10A3.75 3.75 0 0 0 18.75 15V7.5A3.75 3.75 0 0 0 15 3.75V1.875a.625.625 0 1 0-1.25 0V3.75ZM5 5h2.5v12.5H5V5Zm3.75 12.5h2.5V5h-2.5v12.5Zm-5-12.166v11.832A2.499 2.499 0 0 1 2.5 15V7.5c0-.925.503-1.733 1.25-2.166ZM12.5 17.5V5H15v12.5h-2.5Zm3.75-.334A2.499 2.499 0 0 0 17.5 15V7.5c0-.925-.503-1.733-1.25-2.166v11.832Z"; 

    protected override string IconGeometry24 { get; }
        = "M16.5 4.5h-9V2.25a.75.75 0 0 0-1.5 0V4.5A4.5 4.5 0 0 0 1.5 9v9A4.5 4.5 0 0 0 6 22.5h12a4.5 4.5 0 0 0 4.5-4.5V9A4.5 4.5 0 0 0 18 4.5V2.25a.75.75 0 0 0-1.5 0V4.5ZM6 6h3v15H6V6Zm4.5 15h3V6h-3v15Zm-6-14.599V20.6A2.999 2.999 0 0 1 3 18V9a3 3 0 0 1 1.5-2.599ZM15 21V6h3v15h-3Zm4.5-.401A2.999 2.999 0 0 0 21 18V9a3 3 0 0 0-1.5-2.599V20.6Z"; 

    protected override string IconGeometry32 { get; }
        = "M22 6H10V3a1 1 0 1 0-2 0v3a6 6 0 0 0-6 6v12a6 6 0 0 0 6 6h16a6 6 0 0 0 6-6V12a6 6 0 0 0-6-6V3a1 1 0 1 0-2 0v3ZM8 8h4v20H8V8Zm6 20h4V8h-4v20ZM6 8.535v18.93A3.998 3.998 0 0 1 4 24V12c0-1.48.804-2.773 2-3.465ZM20 28V8h4v20h-4Zm6-.535A3.998 3.998 0 0 0 28 24V12c0-1.48-.804-2.773-2-3.465v18.93Z";
}