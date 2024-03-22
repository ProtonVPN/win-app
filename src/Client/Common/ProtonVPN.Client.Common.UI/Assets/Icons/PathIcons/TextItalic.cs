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

public class TextItalic : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7 1.5a.5.5 0 0 1 .5-.5h4a.5.5 0 0 1 0 1H9.898l-2.77 12H8.5a.5.5 0 0 1 0 1h-4a.5.5 0 0 1 0-1h1.602l2.77-12H7.5a.5.5 0 0 1-.5-.5Z";

    protected override string IconGeometry20 { get; }
        = "M8.75 1.875c0-.345.28-.625.625-.625h5a.625.625 0 1 1 0 1.25h-2.003l-3.461 15h1.714a.625.625 0 1 1 0 1.25h-5a.625.625 0 1 1 0-1.25h2.003l3.461-15H9.375a.625.625 0 0 1-.625-.625Z"; 

    protected override string IconGeometry24 { get; }
        = "M10.5 2.25a.75.75 0 0 1 .75-.75h6a.75.75 0 0 1 0 1.5h-2.403l-4.154 18h2.057a.75.75 0 0 1 0 1.5h-6a.75.75 0 0 1 0-1.5h2.403l4.154-18H11.25a.75.75 0 0 1-.75-.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M14 3a1 1 0 0 1 1-1h8a1 1 0 1 1 0 2h-3.204l-5.539 24H17a1 1 0 1 1 0 2H9a1 1 0 1 1 0-2h3.204l5.539-24H15a1 1 0 0 1-1-1Z";
}