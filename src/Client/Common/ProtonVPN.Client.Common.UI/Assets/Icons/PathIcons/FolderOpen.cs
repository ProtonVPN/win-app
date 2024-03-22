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

public class FolderOpen : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M2.61 14H2.5A1.5 1.5 0 0 1 1 12.5v-8A1.5 1.5 0 0 1 2.5 3h2.764a1.5 1.5 0 0 1 .67.158L7.619 4H12.5A1.5 1.5 0 0 1 14 5.5V7h.787a.75.75 0 0 1 .67 1.085l-2.68 5.362a1 1 0 0 1-.895.553H2.611ZM2 4.5a.5.5 0 0 1 .5-.5h2.764a.5.5 0 0 1 .224.053l1.746.873A.7.7 0 0 0 7.547 5H12.5a.5.5 0 0 1 .5.5V7H5.427a1.5 1.5 0 0 0-1.342.83L2 12V4.5Zm.618 8.5L4.98 8.276A.5.5 0 0 1 5.427 8h8.955l-2.5 5H2.618Z";

    protected override string IconGeometry20 { get; }
        = "M2.5 4.375c0-.345.28-.625.625-.625h3.3c.146 0 .287.05.399.143l2.084 1.72c.157.129.354.2.557.2h6.16c.345 0 .625.28.625.625v1.687H7.375c-.692 0-1.329.382-1.654.993L2.5 15.156V4.375ZM10 16.25H3.333l3.49-6.544a.625.625 0 0 1 .552-.331h11.167l-3.667 6.875H10ZM3.317 17.5h-.192a1.875 1.875 0 0 1-1.875-1.875V4.375c0-1.036.84-1.875 1.875-1.875h3.3c.436 0 .858.152 1.194.429L9.6 4.562h6.026c1.035 0 1.875.84 1.875 1.876v1.687h1.563c.707 0 1.16.754.827 1.379l-3.912 7.334a1.25 1.25 0 0 1-1.103.662H3.317Z"; 

    protected override string IconGeometry24 { get; }
        = "M3 5.25a.75.75 0 0 1 .75-.75h3.961a.75.75 0 0 1 .477.171l2.502 2.064c.188.155.424.24.668.24h7.392a.75.75 0 0 1 .75.75V9.75H8.85a2.25 2.25 0 0 0-1.985 1.191L3 18.187V5.25Zm9 14.25H4l4.188-7.853a.75.75 0 0 1 .662-.397h13.4l-4.4 8.25H12ZM3.98 21h-.23a2.25 2.25 0 0 1-2.25-2.25V5.25A2.25 2.25 0 0 1 3.75 3h3.961a2.25 2.25 0 0 1 1.432.514l2.376 1.961h7.231A2.25 2.25 0 0 1 21 7.725V9.75h1.875c.85 0 1.392.905.993 1.654l-4.694 8.802A1.5 1.5 0 0 1 17.85 21H3.98Z"; 

    protected override string IconGeometry32 { get; }
        = "M4 7a1 1 0 0 1 1-1h5.281a1 1 0 0 1 .637.229l3.335 2.75c.25.208.566.321.89.321H25a1 1 0 0 1 1 1V13H11.8a3 3 0 0 0-2.647 1.588L4 24.25V7Zm12 19H5.333l5.585-10.47A1 1 0 0 1 11.8 15h17.867L23.8 26H16ZM5.307 28H5a3 3 0 0 1-3-3V7a3 3 0 0 1 3-3h5.281a3 3 0 0 1 1.91.686L15.359 7.3H25a3 3 0 0 1 3 3V13h2.5a1.5 1.5 0 0 1 1.323 2.206L25.566 26.94A2 2 0 0 1 23.8 28H5.307Z";
}