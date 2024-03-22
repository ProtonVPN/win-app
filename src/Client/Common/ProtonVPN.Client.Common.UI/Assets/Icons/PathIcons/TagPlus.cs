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

public class TagPlus : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M14 .5a.5.5 0 1 0-1 0V2h-1.5a.5.5 0 0 0 0 1H13v1.5a.5.5 0 0 0 1 0V3h1.5a.5.5 0 1 0 0-1H14V.5Zm-.444 8.925-4.95 4.95L2.182 7.95V3h4.95l6.424 6.425Zm-4.243 5.657a1 1 0 0 1-1.414 0L1.475 8.657a1 1 0 0 1-.293-.707V3a1 1 0 0 1 1-1h4.95a1 1 0 0 1 .706.293l6.425 6.425a1 1 0 0 1 0 1.414l-4.95 4.95ZM5.681 5.5a1 1 0 1 1-2 0 1 1 0 0 1 2 0Z";

    protected override string IconGeometry20 { get; }
        = "M16.875 1.25a.625.625 0 1 0-1.25 0v1.875H13.75a.625.625 0 1 0 0 1.25h1.875V6.25a.625.625 0 1 0 1.25 0V4.375h1.875a.625.625 0 1 0 0-1.25h-1.875V1.25Zm.07 10.531-6.187 6.188-8.03-8.031V3.75h6.186l8.031 8.03Zm-5.303 7.071a1.25 1.25 0 0 1-1.768 0l-8.03-8.03a1.25 1.25 0 0 1-.367-.884V3.75c0-.69.56-1.25 1.25-1.25h6.187c.332 0 .65.131.884.366l8.031 8.03a1.25 1.25 0 0 1 0 1.768l-6.187 6.187ZM7.102 6.875a1.25 1.25 0 1 1-2.5 0 1.25 1.25 0 0 1 2.5 0Z"; 

    protected override string IconGeometry24 { get; }
        = "M20.25 1.5a.75.75 0 0 0-1.5 0v2.25H16.5a.75.75 0 0 0 0 1.5h2.25V7.5a.75.75 0 0 0 1.5 0V5.25h2.25a.75.75 0 0 0 0-1.5h-2.25V1.5Zm.084 12.638-7.424 7.424-9.637-9.637V4.501h7.424l9.637 9.637Zm-6.364 8.485a1.5 1.5 0 0 1-2.121 0l-9.637-9.637a1.5 1.5 0 0 1-.44-1.06V4.5a1.5 1.5 0 0 1 1.5-1.5h7.425a1.5 1.5 0 0 1 1.061.44l9.637 9.636a1.5 1.5 0 0 1 0 2.121l-7.425 7.425ZM8.522 8.25a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0Z"; 

    protected override string IconGeometry32 { get; }
        = "M27 2a1 1 0 1 0-2 0v3h-3a1 1 0 1 0 0 2h3v3a1 1 0 1 0 2 0V7h3a1 1 0 1 0 0-2h-3V2Zm.112 16.85-9.9 9.9L4.365 15.9V6.002h9.9l12.848 12.85Zm-8.485 11.314a2 2 0 0 1-2.828 0l-12.85-12.85a2 2 0 0 1-.585-1.414V6.002a2 2 0 0 1 2-2h9.9a2 2 0 0 1 1.413.586l12.85 12.849a2 2 0 0 1 0 2.828l-9.9 9.9ZM11.363 11a2 2 0 1 1-4 0 2 2 0 0 1 4 0Z";
}