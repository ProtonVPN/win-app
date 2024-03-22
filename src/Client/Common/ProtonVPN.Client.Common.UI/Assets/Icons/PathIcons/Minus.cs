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

public class Minus : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M15 8.5a.5.5 0 0 1-.5.5h-13a.5.5 0 0 1 0-1h13a.5.5 0 0 1 .5.5Z";

    protected override string IconGeometry20 { get; }
        = "M18.75 10c0 .345-.28.625-.625.625H1.875a.625.625 0 1 1 0-1.25h16.25c.345 0 .625.28.625.625Z"; 

    protected override string IconGeometry24 { get; }
        = "M22.5 12a.75.75 0 0 1-.75.75H2.25a.75.75 0 0 1 0-1.5h19.5a.75.75 0 0 1 .75.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M30 16a1 1 0 0 1-1 1H3a1 1 0 1 1 0-2h26a1 1 0 0 1 1 1Z";
}