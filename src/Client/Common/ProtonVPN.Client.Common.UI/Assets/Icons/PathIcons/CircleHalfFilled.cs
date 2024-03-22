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

public class CircleHalfFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8 14V2a6 6 0 1 0 0 12ZM8 1a7 7 0 1 1 0 14A7 7 0 0 1 8 1Z";

    protected override string IconGeometry20 { get; }
        = "M10 17.5v-15a7.5 7.5 0 1 0 0 15Zm0-16.25a8.75 8.75 0 1 1 0 17.5 8.75 8.75 0 0 1 0-17.5Z"; 

    protected override string IconGeometry24 { get; }
        = "M12 21V3a9 9 0 0 0 0 18Zm0-19.5c5.799 0 10.5 4.701 10.5 10.5S17.799 22.5 12 22.5 1.5 17.799 1.5 12 6.201 1.5 12 1.5Z"; 

    protected override string IconGeometry32 { get; }
        = "M16 28V4C9.373 4 4 9.373 4 16s5.373 12 12 12Zm0-26c7.732 0 14 6.268 14 14s-6.268 14-14 14S2 23.732 2 16 8.268 2 16 2Z";
}