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

public class ChevronLeft : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M10.836 13.87a.5.5 0 0 0 .034-.706L6.176 8l4.694-5.164a.5.5 0 0 0-.74-.672l-5 5.5a.5.5 0 0 0 0 .672l5 5.5a.5.5 0 0 0 .706.034Z";

    protected override string IconGeometry20 { get; }
        = "M13.545 17.337a.625.625 0 0 0 .043-.882L7.72 10l5.868-6.455a.625.625 0 0 0-.925-.84L6.412 9.58a.625.625 0 0 0 0 .84l6.25 6.875a.625.625 0 0 0 .882.042Z"; 

    protected override string IconGeometry24 { get; }
        = "M16.255 20.805a.75.75 0 0 0 .05-1.06L9.264 12l7.041-7.745a.75.75 0 0 0-1.11-1.01l-7.5 8.25a.75.75 0 0 0 0 1.01l7.5 8.25a.75.75 0 0 0 1.06.05Z"; 

    protected override string IconGeometry32 { get; }
        = "M21.673 27.74a1 1 0 0 0 .067-1.413L12.352 16 21.74 5.673a1 1 0 0 0-1.48-1.346l-10 11a1 1 0 0 0 0 1.346l10 11a1 1 0 0 0 1.413.067Z";
}