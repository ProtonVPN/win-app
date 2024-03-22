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

public class PowerOff : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8.5 0a.5.5 0 0 1 .5.5v8a.5.5 0 0 1-1 0v-8a.5.5 0 0 1 .5-.5ZM5.433 2.186a.5.5 0 0 1-.183.683 6.5 6.5 0 1 0 6.5 0 .5.5 0 1 1 .5-.865 7.5 7.5 0 1 1-7.5 0 .5.5 0 0 1 .683.182Z";

    protected override string IconGeometry20 { get; }
        = "M10.625 0c.345 0 .625.28.625.625v10a.625.625 0 1 1-1.25 0v-10c0-.345.28-.625.625-.625ZM6.791 2.733a.625.625 0 0 1-.228.854 8.125 8.125 0 1 0 8.124 0 .625.625 0 0 1 .626-1.082 9.375 9.375 0 1 1-9.376 0 .625.625 0 0 1 .854.228Z"; 

    protected override string IconGeometry24 { get; }
        = "M12.75 0a.75.75 0 0 1 .75.75v12a.75.75 0 0 1-1.5 0v-12a.75.75 0 0 1 .75-.75Zm-4.6 3.28a.75.75 0 0 1-.274 1.024A9.746 9.746 0 0 0 3 12.75c0 5.385 4.365 9.75 9.75 9.75s9.75-4.365 9.75-9.75c0-3.608-1.96-6.76-4.875-8.446a.75.75 0 1 1 .75-1.298A11.246 11.246 0 0 1 24 12.75C24 18.963 18.963 24 12.75 24S1.5 18.963 1.5 12.75c0-4.165 2.264-7.8 5.624-9.744a.75.75 0 0 1 1.025.273Z"; 

    protected override string IconGeometry32 { get; }
        = "M17 0a1 1 0 0 1 1 1v16a1 1 0 1 1-2 0V1a1 1 0 0 1 1-1Zm-6.134 4.372A1 1 0 0 1 10.5 5.74 12.994 12.994 0 0 0 4 17c0 7.18 5.82 13 13 13s13-5.82 13-13c0-4.81-2.612-9.012-6.5-11.261a1 1 0 0 1 1-1.732C28.983 6.6 32 11.447 32 17c0 8.284-6.716 15-15 15-8.284 0-15-6.716-15-15C2 11.447 5.018 6.6 9.5 4.007a1 1 0 0 1 1.366.365Z";
}