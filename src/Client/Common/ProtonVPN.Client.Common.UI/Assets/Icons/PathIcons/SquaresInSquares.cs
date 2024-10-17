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

public class SquaresInSquares : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M14 2H3C2.44772 2 2 2.44772 2 3V14C2 14.5523 2.44772 15 3 15H14C14.5523 15 15 14.5523 15 14V3C15 2.44772 14.5523 2 14 2ZM3 1C1.89543 1 1 1.89543 1 3V14C1 15.1046 1.89543 16 3 16H14C15.1046 16 16 15.1046 16 14V3C16 1.89543 15.1046 1 14 1H3Z M7 5H5V7H7V5ZM5 4C4.44772 4 4 4.44772 4 5V7C4 7.55228 4.44772 8 5 8H7C7.55228 8 8 7.55228 8 7V5C8 4.44772 7.55228 4 7 4H5Z M7 10H5V12H7V10ZM5 9C4.44772 9 4 9.44772 4 10V12C4 12.5523 4.44772 13 5 13H7C7.55228 13 8 12.5523 8 12V10C8 9.44772 7.55228 9 7 9H5Z M12 5H10V7H12V5ZM10 4C9.44772 4 9 4.44772 9 5V7C9 7.55228 9.44772 8 10 8H12C12.5523 8 13 7.55228 13 7V5C13 4.44772 12.5523 4 12 4H10Z";

    protected override string IconGeometry20 { get; }
        = ""; 

    protected override string IconGeometry24 { get; }
        = ""; 

    protected override string IconGeometry32 { get; }
        = "";
}