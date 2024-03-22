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

public class CalendarMonth : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M11 3H5V1.5a.5.5 0 0 0-1 0V3a3 3 0 0 0-3 3v6a3 3 0 0 0 3 3h8a3 3 0 0 0 3-3V6a3 3 0 0 0-3-3V1.5a.5.5 0 0 0-1 0V3ZM4 4h1v2H2a2 2 0 0 1 2-2Zm1 3H2v4h3V7Zm1 4V7h4v4H6Zm-1 1H2a2 2 0 0 0 2 2h1v-2Zm1 2h4v-2H6v2Zm0-8V4h4v2H6Zm5 0V4h1a2 2 0 0 1 2 2h-3Zm0 1v4h3V7h-3Zm3 5a2 2 0 0 1-2 2h-1v-2h3Z";

    protected override string IconGeometry20 { get; }
        = "M13.75 3.75h-7.5V1.875a.625.625 0 1 0-1.25 0V3.75A3.75 3.75 0 0 0 1.25 7.5V15A3.75 3.75 0 0 0 5 18.75h10A3.75 3.75 0 0 0 18.75 15V7.5A3.75 3.75 0 0 0 15 3.75V1.875a.625.625 0 1 0-1.25 0V3.75ZM5 5h1.25v2.5H2.5A2.5 2.5 0 0 1 5 5Zm1.25 3.75H2.5v5h3.75v-5Zm1.25 5v-5h5v5h-5ZM6.25 15H2.5A2.5 2.5 0 0 0 5 17.5h1.25V15Zm1.25 2.5h5V15h-5v2.5Zm0-10V5h5v2.5h-5Zm6.25 0V5H15a2.5 2.5 0 0 1 2.5 2.5h-3.75Zm0 1.25v5h3.75v-5h-3.75ZM17.5 15a2.5 2.5 0 0 1-2.5 2.5h-1.25V15h3.75Z"; 

    protected override string IconGeometry24 { get; }
        = "M16.5 4.5h-9V2.25a.75.75 0 0 0-1.5 0V4.5A4.5 4.5 0 0 0 1.5 9v9A4.5 4.5 0 0 0 6 22.5h12a4.5 4.5 0 0 0 4.5-4.5V9A4.5 4.5 0 0 0 18 4.5V2.25a.75.75 0 0 0-1.5 0V4.5ZM6 6h1.5v3H3a3 3 0 0 1 3-3Zm1.5 4.5H3v6h4.5v-6Zm1.5 6v-6h6v6H9ZM7.5 18H3a3 3 0 0 0 3 3h1.5v-3ZM9 21h6v-3H9v3ZM9 9V6h6v3H9Zm7.5 0V6H18a3 3 0 0 1 3 3h-4.5Zm0 1.5v6H21v-6h-4.5ZM21 18a3 3 0 0 1-3 3h-1.5v-3H21Z"; 

    protected override string IconGeometry32 { get; }
        = "M22 6H10V3a1 1 0 1 0-2 0v3a6 6 0 0 0-6 6v12a6 6 0 0 0 6 6h16a6 6 0 0 0 6-6V12a6 6 0 0 0-6-6V3a1 1 0 1 0-2 0v3ZM8 8h2v4H4a4 4 0 0 1 4-4Zm2 6H4v8h6v-8Zm2 8v-8h8v8h-8Zm-2 2H4a4 4 0 0 0 4 4h2v-4Zm2 4h8v-4h-8v4Zm0-16V8h8v4h-8Zm10 0V8h2a4 4 0 0 1 4 4h-6Zm0 2v8h6v-8h-6Zm6 10a4 4 0 0 1-4 4h-2v-4h6Z";
}