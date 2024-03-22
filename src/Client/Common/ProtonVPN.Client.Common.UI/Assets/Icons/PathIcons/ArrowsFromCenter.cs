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

public class ArrowsFromCenter : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8 2.5a.5.5 0 0 1 .5-.5h5a.5.5 0 0 1 .5.5v5a.5.5 0 0 1-1 0V3.703L3.707 13H7.5a.5.5 0 0 1 0 1h-5a.5.5 0 0 1-.5-.5v-5a.5.5 0 1 1 1 0v3.793L12.289 3H8.5a.5.5 0 0 1-.5-.5Z";

    protected override string IconGeometry20 { get; }
        = "M10 3.125c0-.345.28-.625.625-.625h6.25c.345 0 .625.28.625.625v6.25a.625.625 0 1 1-1.25 0V4.629L4.634 16.249h4.741a.625.625 0 0 1 0 1.25h-6.25a.625.625 0 0 1-.625-.624v-6.25a.625.625 0 0 1 1.25 0v4.74L15.361 3.75h-4.736A.625.625 0 0 1 10 3.125Z"; 

    protected override string IconGeometry24 { get; }
        = "M12 3.75a.75.75 0 0 1 .75-.75h7.5a.75.75 0 0 1 .75.75v7.5a.75.75 0 0 1-1.5 0V5.555L5.56 19.5h5.69a.75.75 0 0 1 0 1.5h-7.5a.75.75 0 0 1-.75-.75v-7.5a.75.75 0 0 1 1.5 0v5.69L18.433 4.5H12.75a.75.75 0 0 1-.75-.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M16 5a1 1 0 0 1 1-1h10a1 1 0 0 1 1 1v10a1 1 0 1 1-2 0V7.406L7.414 26H15a1 1 0 1 1 0 2H5a1 1 0 0 1-1-1V17a1 1 0 1 1 2 0v7.585L24.578 6H17a1 1 0 0 1-1-1Z";
}