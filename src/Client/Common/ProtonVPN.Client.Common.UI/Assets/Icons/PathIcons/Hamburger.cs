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

public class Hamburger : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M2 4.5a.5.5 0 0 1 .5-.5h11a.5.5 0 0 1 0 1h-11a.5.5 0 0 1-.5-.5Zm0 4a.5.5 0 0 1 .5-.5h11a.5.5 0 0 1 0 1h-11a.5.5 0 0 1-.5-.5Zm.5 4.5h11a.5.5 0 0 0 0-1h-11a.5.5 0 0 0 0 1Z";

    protected override string IconGeometry20 { get; }
        = "M2.5 4.375c0-.345.28-.625.625-.625h13.75a.625.625 0 1 1 0 1.25H3.125a.625.625 0 0 1-.625-.625ZM2.5 10c0-.345.28-.625.625-.625h13.75a.625.625 0 1 1 0 1.25H3.125A.625.625 0 0 1 2.5 10Zm.625 6.25h13.75a.625.625 0 1 0 0-1.25H3.125a.625.625 0 1 0 0 1.25Z"; 

    protected override string IconGeometry24 { get; }
        = "M3 5.25a.75.75 0 0 1 .75-.75h16.5a.75.75 0 0 1 0 1.5H3.75A.75.75 0 0 1 3 5.25ZM3 12a.75.75 0 0 1 .75-.75h16.5a.75.75 0 0 1 0 1.5H3.75A.75.75 0 0 1 3 12Zm.75 7.5h16.5a.75.75 0 0 0 0-1.5H3.75a.75.75 0 0 0 0 1.5Z"; 

    protected override string IconGeometry32 { get; }
        = "M4 7a1 1 0 0 1 1-1h22a1 1 0 1 1 0 2H5a1 1 0 0 1-1-1Zm0 9a1 1 0 0 1 1-1h22a1 1 0 1 1 0 2H5a1 1 0 0 1-1-1Zm1 10h22a1 1 0 1 0 0-2H5a1 1 0 1 0 0 2Z";
}