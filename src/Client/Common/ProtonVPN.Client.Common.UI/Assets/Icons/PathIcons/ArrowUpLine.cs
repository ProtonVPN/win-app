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

public class ArrowUpLine : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8.148 1.148a.5.5 0 0 1 .707 0l4 3.998a.5.5 0 1 1-.707.708L9 2.707V11.5a.5.5 0 1 1-1 0V2.71L4.855 5.854a.5.5 0 0 1-.707-.708l4-3.998ZM2 13.5a.5.5 0 0 1 .5-.5h12a.5.5 0 0 1 0 1h-12a.5.5 0 0 1-.5-.5Z";

    protected override string IconGeometry20 { get; }
        = "M10.185 1.435a.625.625 0 0 1 .884 0l5 4.998a.625.625 0 0 1-.884.884L11.25 3.383v10.992a.625.625 0 1 1-1.25 0V3.387l-3.931 3.93a.625.625 0 1 1-.884-.884l5-4.998ZM2.5 16.875c0-.345.28-.625.625-.625h15a.625.625 0 1 1 0 1.25h-15a.625.625 0 0 1-.625-.625Z"; 

    protected override string IconGeometry24 { get; }
        = "M12.222 1.722a.75.75 0 0 1 1.06 0l6 5.998a.75.75 0 0 1-1.06 1.06L13.5 4.06v13.19a.75.75 0 0 1-1.5 0V4.065L7.283 8.78a.75.75 0 0 1-1.06-1.06l6-5.998ZM3 20.25a.75.75 0 0 1 .75-.75h18a.75.75 0 0 1 0 1.5h-18a.75.75 0 0 1-.75-.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M16.296 2.296a1 1 0 0 1 1.414 0l8 7.997a1 1 0 1 1-1.414 1.414L18 5.413V23a1 1 0 1 1-2 0V5.42l-6.29 6.287a1 1 0 1 1-1.414-1.414l8-7.997ZM4 27a1 1 0 0 1 1-1h24a1 1 0 1 1 0 2H5a1 1 0 0 1-1-1Z";
}