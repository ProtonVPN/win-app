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

public class ChevronDownFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7.588 9.846 4.138 6.72c-.294-.266-.086-.72.329-.72h7.066c.415 0 .623.454.33.72L8.412 9.846a.626.626 0 0 1-.824 0Z";

    protected override string IconGeometry20 { get; }
        = "M9.485 12.307 5.172 8.4c-.367-.332-.107-.9.411-.9h8.834c.518 0 .778.568.411.9l-4.313 3.907a.782.782 0 0 1-1.03 0Z"; 

    protected override string IconGeometry24 { get; }
        = "M11.434 14.768 6.689 10.08C6.285 9.682 6.57 9 7.142 9h9.716c.57 0 .857.682.453 1.08l-4.745 4.688a.807.807 0 0 1-1.132 0Z"; 

    protected override string IconGeometry32 { get; }
        = "";
}