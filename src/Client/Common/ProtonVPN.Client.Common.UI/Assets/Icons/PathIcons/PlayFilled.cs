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

public class PlayFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M5 4.002c0-.83.954-1.3 1.612-.793l5.197 3.997a1.002 1.002 0 0 1 0 1.588L6.612 12.79A1.002 1.002 0 0 1 5 11.997V4.002Z";

    protected override string IconGeometry20 { get; }
        = "M6.25 5.003c0-1.038 1.192-1.625 2.015-.992l6.496 4.997a1.252 1.252 0 0 1 0 1.985l-6.496 4.996c-.823.634-2.015.047-2.015-.992V5.003Z"; 

    protected override string IconGeometry24 { get; }
        = "M7.5 6.004c0-1.247 1.43-1.951 2.418-1.191l7.796 5.996c.782.602.782 1.78 0 2.382l-7.796 5.996c-.988.76-2.418.056-2.418-1.19V6.003Z"; 

    protected override string IconGeometry32 { get; }
        = "M10 8.005c0-1.662 1.907-2.601 3.225-1.588l10.393 7.995a2.003 2.003 0 0 1 0 3.176l-10.393 7.995C11.907 26.596 10 25.657 10 23.995V8.005Z";
}