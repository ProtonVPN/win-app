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

public class TextAlignLeft : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M1 3.5a.5.5 0 0 1 .5-.5h6a.5.5 0 0 1 0 1h-6a.5.5 0 0 1-.5-.5Zm14 3a.5.5 0 0 1-.5.5h-13a.5.5 0 0 1 0-1h13a.5.5 0 0 1 .5.5Zm-14 3a.5.5 0 0 1 .5-.5h6a.5.5 0 0 1 0 1h-6a.5.5 0 0 1-.5-.5Zm14 3a.5.5 0 0 1-.5.5h-13a.5.5 0 0 1 0-1h13a.5.5 0 0 1 .5.5Z";

    protected override string IconGeometry20 { get; }
        = "M1.25 3.125c0-.345.28-.625.625-.625h7.5a.625.625 0 1 1 0 1.25h-7.5a.625.625 0 0 1-.625-.625ZM18.75 7.5c0 .345-.28.625-.625.625H1.875a.625.625 0 1 1 0-1.25h16.25c.345 0 .625.28.625.625Zm-17.5 5c0-.345.28-.625.625-.625h7.5a.625.625 0 1 1 0 1.25h-7.5a.625.625 0 0 1-.625-.625Zm17.5 4.375c0 .345-.28.625-.625.625H1.875a.625.625 0 1 1 0-1.25h16.25c.345 0 .625.28.625.625Z"; 

    protected override string IconGeometry24 { get; }
        = "M1.5 3.75A.75.75 0 0 1 2.25 3h9a.75.75 0 0 1 0 1.5h-9a.75.75 0 0 1-.75-.75ZM22.5 9a.75.75 0 0 1-.75.75H2.25a.75.75 0 0 1 0-1.5h19.5a.75.75 0 0 1 .75.75Zm-21 6a.75.75 0 0 1 .75-.75h9a.75.75 0 0 1 0 1.5h-9A.75.75 0 0 1 1.5 15Zm21 5.25a.75.75 0 0 1-.75.75H2.25a.75.75 0 0 1 0-1.5h19.5a.75.75 0 0 1 .75.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M2 5a1 1 0 0 1 1-1h12a1 1 0 1 1 0 2H3a1 1 0 0 1-1-1Zm28 7a1 1 0 0 1-1 1H3a1 1 0 1 1 0-2h26a1 1 0 0 1 1 1ZM2 20a1 1 0 0 1 1-1h12a1 1 0 1 1 0 2H3a1 1 0 0 1-1-1Zm28 7a1 1 0 0 1-1 1H3a1 1 0 1 1 0-2h26a1 1 0 0 1 1 1Z";
}