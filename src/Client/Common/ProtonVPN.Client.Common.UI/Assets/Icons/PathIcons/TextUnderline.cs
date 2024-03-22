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

public class TextUnderline : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M3.5 1a.5.5 0 0 0 0 1H4v6.5a4.5 4.5 0 1 0 9 0V2h.5a.5.5 0 0 0 0-1h-2a.5.5 0 0 0 0 1h.5v6.5a3.5 3.5 0 1 1-7 0V2h.5a.5.5 0 0 0 0-1h-2Zm0 13a.5.5 0 0 0 0 1h10a.5.5 0 0 0 0-1h-10Z";

    protected override string IconGeometry20 { get; }
        = "M2.5 1.875c0-.345.28-.625.625-.625h3.75a.625.625 0 1 1 0 1.25h-1.25v8.125a4.375 4.375 0 0 0 8.75 0V2.5h-1.25a.625.625 0 1 1 0-1.25h3.75a.625.625 0 1 1 0 1.25h-1.25v8.125a5.625 5.625 0 0 1-11.25 0V2.5h-1.25a.625.625 0 0 1-.625-.625ZM3.125 17.5a.625.625 0 1 0 0 1.25h13.75a.625.625 0 1 0 0-1.25H3.125Z"; 

    protected override string IconGeometry24 { get; }
        = "M3 2.25a.75.75 0 0 1 .75-.75h4.5a.75.75 0 0 1 0 1.5h-1.5v9.75a5.25 5.25 0 1 0 10.5 0V3h-1.5a.75.75 0 0 1 0-1.5h4.5a.75.75 0 0 1 0 1.5h-1.5v9.75a6.75 6.75 0 0 1-13.5 0V3h-1.5A.75.75 0 0 1 3 2.25ZM3.75 21a.75.75 0 0 0 0 1.5h16.5a.75.75 0 0 0 0-1.5H3.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M4 3a1 1 0 0 1 1-1h6a1 1 0 1 1 0 2H9v13a7 7 0 1 0 14 0V4h-2a1 1 0 1 1 0-2h6a1 1 0 1 1 0 2h-2v13a9 9 0 1 1-18 0V4H5a1 1 0 0 1-1-1Zm1 25a1 1 0 1 0 0 2h22a1 1 0 1 0 0-2H5Z";
}