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

public class LinesHorizontal : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M13.5 6h-11a.5.5 0 0 0 0 1h11a.5.5 0 0 0 0-1Zm0 3h-11a.5.5 0 0 0 0 1h11a.5.5 0 0 0 0-1Z";

    protected override string IconGeometry20 { get; }
        = "M16.875 7.5H3.125a.625.625 0 1 0 0 1.25h13.75a.625.625 0 1 0 0-1.25Zm0 3.75H3.125a.625.625 0 1 0 0 1.25h13.75a.625.625 0 1 0 0-1.25Z"; 

    protected override string IconGeometry24 { get; }
        = "M20.25 9H3.75a.75.75 0 0 0 0 1.5h16.5a.75.75 0 0 0 0-1.5Zm0 4.5H3.75a.75.75 0 0 0 0 1.5h16.5a.75.75 0 0 0 0-1.5Z"; 

    protected override string IconGeometry32 { get; }
        = "M27 12H5a1 1 0 1 0 0 2h22a1 1 0 1 0 0-2Zm0 6H5a1 1 0 1 0 0 2h22a1 1 0 1 0 0-2Z";
}