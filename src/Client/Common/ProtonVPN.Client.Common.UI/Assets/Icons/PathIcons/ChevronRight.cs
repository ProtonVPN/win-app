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

public class ChevronRight : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M5.164 13.87a.5.5 0 0 1-.034-.706L9.824 8 5.13 2.836a.5.5 0 1 1 .74-.672l5 5.5a.5.5 0 0 1 0 .672l-5 5.5a.5.5 0 0 1-.706.034Z";

    protected override string IconGeometry20 { get; }
        = "M6.455 17.337a.625.625 0 0 1-.042-.882L12.28 10 6.413 3.545a.625.625 0 0 1 .924-.84l6.25 6.875a.625.625 0 0 1 0 .84l-6.25 6.875a.625.625 0 0 1-.882.042Z"; 

    protected override string IconGeometry24 { get; }
        = "M7.745 20.805a.75.75 0 0 1-.05-1.06L14.736 12 7.695 4.255a.75.75 0 1 1 1.11-1.01l7.5 8.25a.75.75 0 0 1 0 1.01l-7.5 8.25a.75.75 0 0 1-1.06.05Z"; 

    protected override string IconGeometry32 { get; }
        = "M10.327 27.74a1 1 0 0 1-.067-1.413L19.648 16 10.26 5.673a1 1 0 1 1 1.48-1.346l10 11a1 1 0 0 1 0 1.346l-10 11a1 1 0 0 1-1.413.067Z";
}