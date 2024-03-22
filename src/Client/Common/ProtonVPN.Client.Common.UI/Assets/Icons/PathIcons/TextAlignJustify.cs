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

public class TextAlignJustify : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M2.5 3h11a.5.5 0 0 1 0 1h-11a.5.5 0 0 1 0-1ZM2 6.5a.5.5 0 0 1 .5-.5h11a.5.5 0 0 1 0 1h-11a.5.5 0 0 1-.5-.5ZM2.5 9h11a.5.5 0 0 1 0 1h-11a.5.5 0 0 1 0-1ZM2 12.5a.5.5 0 0 1 .5-.5h11a.5.5 0 0 1 0 1h-11a.5.5 0 0 1-.5-.5Z";

    protected override string IconGeometry20 { get; }
        = "M1.875 2.5h16.25a.625.625 0 1 1 0 1.25H1.875a.625.625 0 1 1 0-1.25Zm-.625 5c0-.345.28-.625.625-.625h16.25a.625.625 0 1 1 0 1.25H1.875A.625.625 0 0 1 1.25 7.5Zm.625 4.375h16.25a.625.625 0 1 1 0 1.25H1.875a.625.625 0 1 1 0-1.25Zm-.625 5c0-.345.28-.625.625-.625h16.25a.625.625 0 1 1 0 1.25H1.875a.625.625 0 0 1-.625-.625Z"; 

    protected override string IconGeometry24 { get; }
        = "M2.25 3h19.5a.75.75 0 0 1 0 1.5H2.25a.75.75 0 1 1 0-1.5ZM1.5 9a.75.75 0 0 1 .75-.75h19.5a.75.75 0 0 1 0 1.5H2.25A.75.75 0 0 1 1.5 9Zm.75 5.25h19.5a.75.75 0 0 1 0 1.5H2.25a.75.75 0 0 1 0-1.5Zm-.75 6a.75.75 0 0 1 .75-.75h19.5a.75.75 0 0 1 0 1.5H2.25a.75.75 0 0 1-.75-.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M3 4h26a1 1 0 1 1 0 2H3a1 1 0 1 1 0-2Zm-1 8a1 1 0 0 1 1-1h26a1 1 0 1 1 0 2H3a1 1 0 0 1-1-1Zm1 7h26a1 1 0 1 1 0 2H3a1 1 0 1 1 0-2Zm-1 8a1 1 0 0 1 1-1h26a1 1 0 1 1 0 2H3a1 1 0 0 1-1-1Z";
}