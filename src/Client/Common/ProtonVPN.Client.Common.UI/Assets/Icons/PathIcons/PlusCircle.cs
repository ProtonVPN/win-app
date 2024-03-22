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

public class PlusCircle : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M9 5.5a.5.5 0 0 0-1 0V8H5.5a.5.5 0 0 0 0 1H8v2.5a.5.5 0 0 0 1 0V9h2.5a.5.5 0 0 0 0-1H9V5.5Z M8.5 1a7.5 7.5 0 1 0 0 15 7.5 7.5 0 0 0 0-15ZM2 8.5a6.5 6.5 0 1 1 13 0 6.5 6.5 0 0 1-13 0Z";

    protected override string IconGeometry20 { get; }
        = "M11.25 6.875a.625.625 0 1 0-1.25 0V10H6.875a.625.625 0 1 0 0 1.25H10v3.125a.625.625 0 1 0 1.25 0V11.25h3.125a.625.625 0 1 0 0-1.25H11.25V6.875Z M10.625 1.25a9.375 9.375 0 1 0 0 18.75 9.375 9.375 0 0 0 0-18.75ZM2.5 10.625a8.125 8.125 0 1 1 16.25 0 8.125 8.125 0 0 1-16.25 0Z"; 

    protected override string IconGeometry24 { get; }
        = "M13.5 8.25a.75.75 0 0 0-1.5 0V12H8.25a.75.75 0 0 0 0 1.5H12v3.75a.75.75 0 0 0 1.5 0V13.5h3.75a.75.75 0 0 0 0-1.5H13.5V8.25Z M12.75 1.5C6.537 1.5 1.5 6.537 1.5 12.75S6.537 24 12.75 24 24 18.963 24 12.75 18.963 1.5 12.75 1.5ZM3 12.75C3 7.365 7.365 3 12.75 3s9.75 4.365 9.75 9.75-4.365 9.75-9.75 9.75S3 18.135 3 12.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M18 11a1 1 0 1 0-2 0v5h-5a1 1 0 1 0 0 2h5v5a1 1 0 1 0 2 0v-5h5a1 1 0 1 0 0-2h-5v-5Z M17 2C8.716 2 2 8.716 2 17c0 8.284 6.716 15 15 15 8.284 0 15-6.716 15-15 0-8.284-6.716-15-15-15ZM4 17C4 9.82 9.82 4 17 4s13 5.82 13 13-5.82 13-13 13S4 24.18 4 17Z";
}