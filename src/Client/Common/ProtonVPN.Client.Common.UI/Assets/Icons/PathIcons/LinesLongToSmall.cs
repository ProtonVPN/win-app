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

public class LinesLongToSmall : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M2 4.5a.5.5 0 0 1 .5-.5h11a.5.5 0 0 1 0 1h-11a.5.5 0 0 1-.5-.5Zm2 4a.5.5 0 0 1 .5-.5h7a.5.5 0 0 1 0 1h-7a.5.5 0 0 1-.5-.5ZM6.5 12a.5.5 0 0 0 0 1h3a.5.5 0 0 0 0-1h-3Z";

    protected override string IconGeometry20 { get; }
        = "M2.5 5.625c0-.345.28-.625.625-.625h13.75a.625.625 0 1 1 0 1.25H3.125a.625.625 0 0 1-.625-.625Zm2.5 5c0-.345.28-.625.625-.625h8.75a.625.625 0 1 1 0 1.25h-8.75A.625.625 0 0 1 5 10.625ZM8.125 15a.625.625 0 1 0 0 1.25h3.75a.625.625 0 1 0 0-1.25h-3.75Z"; 

    protected override string IconGeometry24 { get; }
        = "M3 6.75A.75.75 0 0 1 3.75 6h16.5a.75.75 0 0 1 0 1.5H3.75A.75.75 0 0 1 3 6.75Zm3 6a.75.75 0 0 1 .75-.75h10.5a.75.75 0 0 1 0 1.5H6.75a.75.75 0 0 1-.75-.75ZM9.75 18a.75.75 0 0 0 0 1.5h4.5a.75.75 0 0 0 0-1.5h-4.5Z"; 

    protected override string IconGeometry32 { get; }
        = "M4 9a1 1 0 0 1 1-1h22a1 1 0 1 1 0 2H5a1 1 0 0 1-1-1Zm4 8a1 1 0 0 1 1-1h14a1 1 0 1 1 0 2H9a1 1 0 0 1-1-1Zm5 7a1 1 0 1 0 0 2h6a1 1 0 1 0 0-2h-6Z";
}