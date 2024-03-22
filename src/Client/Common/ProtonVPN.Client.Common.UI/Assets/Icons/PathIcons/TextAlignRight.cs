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

public class TextAlignRight : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M15 3.5a.5.5 0 0 0-.5-.5h-6a.5.5 0 0 0 0 1h6a.5.5 0 0 0 .5-.5Zm-14 3a.5.5 0 0 0 .5.5h13a.5.5 0 0 0 0-1h-13a.5.5 0 0 0-.5.5Zm14 3a.5.5 0 0 0-.5-.5h-6a.5.5 0 0 0 0 1h6a.5.5 0 0 0 .5-.5Zm-14 3a.5.5 0 0 0 .5.5h13a.5.5 0 0 0 0-1h-13a.5.5 0 0 0-.5.5Z";

    protected override string IconGeometry20 { get; }
        = "M18.75 3.125a.625.625 0 0 0-.625-.625h-7.5a.625.625 0 1 0 0 1.25h7.5c.345 0 .625-.28.625-.625ZM1.25 7.5c0 .345.28.625.625.625h16.25a.625.625 0 1 0 0-1.25H1.875a.625.625 0 0 0-.625.625Zm17.5 5a.625.625 0 0 0-.625-.625h-7.5a.625.625 0 1 0 0 1.25h7.5c.345 0 .625-.28.625-.625Zm-17.5 4.375c0 .345.28.625.625.625h16.25a.625.625 0 1 0 0-1.25H1.875a.625.625 0 0 0-.625.625Z"; 

    protected override string IconGeometry24 { get; }
        = "M22.5 3.75a.75.75 0 0 0-.75-.75h-9a.75.75 0 0 0 0 1.5h9a.75.75 0 0 0 .75-.75ZM1.5 9c0 .414.336.75.75.75h19.5a.75.75 0 0 0 0-1.5H2.25A.75.75 0 0 0 1.5 9Zm21 6a.75.75 0 0 0-.75-.75h-9a.75.75 0 0 0 0 1.5h9a.75.75 0 0 0 .75-.75Zm-21 5.25c0 .414.336.75.75.75h19.5a.75.75 0 0 0 0-1.5H2.25a.75.75 0 0 0-.75.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M30 5a1 1 0 0 0-1-1H17a1 1 0 1 0 0 2h12a1 1 0 0 0 1-1ZM2 12a1 1 0 0 0 1 1h26a1 1 0 1 0 0-2H3a1 1 0 0 0-1 1Zm28 8a1 1 0 0 0-1-1H17a1 1 0 1 0 0 2h12a1 1 0 0 0 1-1ZM2 27a1 1 0 0 0 1 1h26a1 1 0 1 0 0-2H3a1 1 0 0 0-1 1Z";
}