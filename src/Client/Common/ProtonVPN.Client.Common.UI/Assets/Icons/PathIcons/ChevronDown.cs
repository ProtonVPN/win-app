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

public class ChevronDown : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M13.87 5.164a.5.5 0 0 0-.706-.034L8 9.824 2.836 5.13a.5.5 0 1 0-.672.74l5.5 5a.5.5 0 0 0 .672 0l5.5-5a.5.5 0 0 0 .034-.706Z";

    protected override string IconGeometry20 { get; }
        = "M17.337 6.455a.625.625 0 0 0-.882-.042L10 12.28 3.545 6.413a.625.625 0 0 0-.84.924l6.875 6.25a.625.625 0 0 0 .84 0l6.875-6.25a.625.625 0 0 0 .042-.882Z"; 

    protected override string IconGeometry24 { get; }
        = "M20.805 7.745a.75.75 0 0 0-1.06-.05L12 14.736 4.255 7.695a.75.75 0 1 0-1.01 1.11l8.25 7.5a.75.75 0 0 0 1.01 0l8.25-7.5a.75.75 0 0 0 .05-1.06Z"; 

    protected override string IconGeometry32 { get; }
        = "M27.74 10.327a1 1 0 0 0-1.413-.067L16 19.648 5.673 10.26a1 1 0 1 0-1.346 1.48l11 10a1 1 0 0 0 1.346 0l11-10a1 1 0 0 0 .067-1.413Z";
}