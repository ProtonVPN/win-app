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

public class ArrowOutFromRectangle : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M10 5V3a1 1 0 0 0-1-1H3a1 1 0 0 0-1 1v11a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1v-2h1v2a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V3a2 2 0 0 1 2-2h6a2 2 0 0 1 2 2v2h-1Z M11.646 5.652a.5.5 0 0 1 .707-.011l2.432 2.356a.7.7 0 0 1 0 1.006l-2.432 2.356a.5.5 0 0 1-.696-.718L13.351 9H6.507a.5.5 0 0 1 0-1h6.844l-1.694-1.64a.5.5 0 0 1-.011-.708Z";

    protected override string IconGeometry20 { get; }
        = "M12.5 6.25v-2.5c0-.69-.56-1.25-1.25-1.25h-7.5c-.69 0-1.25.56-1.25 1.25V17.5c0 .69.56 1.25 1.25 1.25h7.5c.69 0 1.25-.56 1.25-1.25V15h1.25v2.5a2.5 2.5 0 0 1-2.5 2.5h-7.5a2.5 2.5 0 0 1-2.5-2.5V3.75a2.5 2.5 0 0 1 2.5-2.5h7.5a2.5 2.5 0 0 1 2.5 2.5v2.5H12.5Z M14.558 7.065a.625.625 0 0 1 .884-.014l3.04 2.946a.875.875 0 0 1 0 1.256l-3.04 2.946a.625.625 0 0 1-.87-.898l2.117-2.051H8.134a.625.625 0 1 1 0-1.25h8.555l-2.117-2.051a.625.625 0 0 1-.014-.884Z"; 

    protected override string IconGeometry24 { get; }
        = "M15 7.5v-3A1.5 1.5 0 0 0 13.5 3h-9A1.5 1.5 0 0 0 3 4.5V21a1.5 1.5 0 0 0 1.5 1.5h9A1.5 1.5 0 0 0 15 21v-3h1.5v3a3 3 0 0 1-3 3h-9a3 3 0 0 1-3-3V4.5a3 3 0 0 1 3-3h9a3 3 0 0 1 3 3v3H15Z M17.47 8.478a.75.75 0 0 1 1.06-.017l3.648 3.535a1.05 1.05 0 0 1 0 1.508L18.53 17.04a.75.75 0 1 1-1.044-1.078l2.54-2.461H9.761a.75.75 0 0 1 0-1.5h10.265l-2.54-2.461a.75.75 0 0 1-.017-1.06Z"; 

    protected override string IconGeometry32 { get; }
        = "M20 10V6a2 2 0 0 0-2-2H6a2 2 0 0 0-2 2v22a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-4h2v4a4 4 0 0 1-4 4H6a4 4 0 0 1-4-4V6a4 4 0 0 1 4-4h12a4 4 0 0 1 4 4v4h-2Z M23.293 11.304a1 1 0 0 1 1.413-.022l4.864 4.713a1.4 1.4 0 0 1 0 2.01l-4.864 4.713a1 1 0 1 1-1.391-1.436L26.702 18H13.015a1 1 0 1 1 0-2h13.687l-3.387-3.282a1 1 0 0 1-.023-1.414Z";
}