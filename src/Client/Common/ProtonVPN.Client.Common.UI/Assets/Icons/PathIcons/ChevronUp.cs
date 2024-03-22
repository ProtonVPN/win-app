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

public class ChevronUp : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M13.87 10.836a.5.5 0 0 1-.706.034L8 6.176 2.836 10.87a.5.5 0 0 1-.672-.74l5.5-5a.5.5 0 0 1 .672 0l5.5 5a.5.5 0 0 1 .034.706Z";

    protected override string IconGeometry20 { get; }
        = "M17.337 13.545a.625.625 0 0 1-.882.043L10 7.72l-6.455 5.868a.625.625 0 0 1-.84-.925l6.875-6.25a.625.625 0 0 1 .84 0l6.875 6.25a.625.625 0 0 1 .042.882Z"; 

    protected override string IconGeometry24 { get; }
        = "M20.805 16.255a.75.75 0 0 1-1.06.05L12 9.264l-7.745 7.041a.75.75 0 0 1-1.01-1.11l8.25-7.5a.75.75 0 0 1 1.01 0l8.25 7.5a.75.75 0 0 1 .05 1.06Z"; 

    protected override string IconGeometry32 { get; }
        = "M27.74 21.673a1 1 0 0 1-1.413.067L16 12.352 5.673 21.74a1 1 0 0 1-1.346-1.48l11-10a1 1 0 0 1 1.346 0l11 10a1 1 0 0 1 .067 1.413Z";
}