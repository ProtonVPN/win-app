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

public class TextAlignCenter : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M11 4H5a.5.5 0 0 1 0-1h6a.5.5 0 0 1 0 1Zm0 6H5a.5.5 0 0 1 0-1h6a.5.5 0 0 1 0 1Zm4-3.5a.5.5 0 0 0-.5-.5h-13a.5.5 0 0 0 0 1h13a.5.5 0 0 0 .5-.5Zm-.5 5.5a.5.5 0 0 1 0 1h-13a.5.5 0 0 1 0-1h13Z";

    protected override string IconGeometry20 { get; }
        = "M13.75 3.75h-7.5a.625.625 0 1 1 0-1.25h7.5a.625.625 0 1 1 0 1.25Zm0 9.375h-7.5a.625.625 0 1 1 0-1.25h7.5a.625.625 0 1 1 0 1.25Zm5-5.625a.625.625 0 0 0-.625-.625H1.875a.625.625 0 1 0 0 1.25h16.25c.345 0 .625-.28.625-.625Zm-.625 8.75a.625.625 0 1 1 0 1.25H1.875a.625.625 0 1 1 0-1.25h16.25Z"; 

    protected override string IconGeometry24 { get; }
        = "M16.5 4.5h-9a.75.75 0 0 1 0-1.5h9a.75.75 0 0 1 0 1.5Zm0 11.25h-9a.75.75 0 0 1 0-1.5h9a.75.75 0 0 1 0 1.5Zm6-6.75a.75.75 0 0 0-.75-.75H2.25a.75.75 0 0 0 0 1.5h19.5A.75.75 0 0 0 22.5 9Zm-.75 10.5a.75.75 0 0 1 0 1.5H2.25a.75.75 0 0 1 0-1.5h19.5Z"; 

    protected override string IconGeometry32 { get; }
        = "M22 6H10a1 1 0 1 1 0-2h12a1 1 0 1 1 0 2Zm0 15H10a1 1 0 1 1 0-2h12a1 1 0 1 1 0 2Zm8-9a1 1 0 0 0-1-1H3a1 1 0 1 0 0 2h26a1 1 0 0 0 1-1Zm-1 14a1 1 0 1 1 0 2H3a1 1 0 1 1 0-2h26Z";
}