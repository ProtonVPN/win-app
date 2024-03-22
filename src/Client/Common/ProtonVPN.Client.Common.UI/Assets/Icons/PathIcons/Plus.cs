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

public class Plus : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8.5 1.5A.5.5 0 0 1 9 2v6h6a.5.5 0 0 1 0 1H9v6a.5.5 0 0 1-1 0V9H2a.5.5 0 0 1 0-1h6V2a.5.5 0 0 1 .5-.5Z";

    protected override string IconGeometry20 { get; }
        = "M10.625 1.875c.345 0 .625.28.625.625V10h7.5a.625.625 0 1 1 0 1.25h-7.5v7.5a.625.625 0 1 1-1.25 0v-7.5H2.5a.625.625 0 1 1 0-1.25H10V2.5c0-.345.28-.625.625-.625Z"; 

    protected override string IconGeometry24 { get; }
        = "M12.75 2.25a.75.75 0 0 1 .75.75v9h9a.75.75 0 0 1 0 1.5h-9v9a.75.75 0 0 1-1.5 0v-9H3A.75.75 0 0 1 3 12h9V3a.75.75 0 0 1 .75-.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M17 3a1 1 0 0 1 1 1v12h12a1 1 0 1 1 0 2H18v12a1 1 0 1 1-2 0V18H4a1 1 0 1 1 0-2h12V4a1 1 0 0 1 1-1Z";
}