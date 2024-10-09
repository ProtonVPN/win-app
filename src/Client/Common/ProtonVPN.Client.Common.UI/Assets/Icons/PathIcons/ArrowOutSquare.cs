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

public class ArrowOutSquare : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M2 5.5C2 4.11929 3.11929 3 4.5 3H9C9.27614 3 9.5 3.22386 9.5 3.5C9.5 3.77614 9.27614 4 9 4H4.5C3.67157 4 3 4.67157 3 5.5V11.5C3 12.3284 3.67157 13 4.5 13H10.5C11.3284 13 12 12.3284 12 11.5V7C12 6.72386 12.2239 6.5 12.5 6.5C12.7761 6.5 13 6.72386 13 7V11.5C13 12.8807 11.8807 14 10.5 14H4.5C3.11929 14 2 12.8807 2 11.5V5.5Z M10 1.5C10 1.22386 10.2239 1 10.5 1H14.5C14.7761 1 15 1.22386 15 1.5V5.5C15 5.77614 14.7761 6 14.5 6C14.2239 6 14 5.77614 14 5.5V2.70711L7.35355 9.35355C7.15829 9.54882 6.84171 9.54882 6.64645 9.35355C6.45118 9.15829 6.45118 8.84171 6.64645 8.64645L13.2929 2H10.5C10.2239 2 10 1.77614 10 1.5Z";

    protected override string IconGeometry20 { get; }
        = "";

    protected override string IconGeometry24 { get; }
        = "";

    protected override string IconGeometry32 { get; }
        = "";
}
