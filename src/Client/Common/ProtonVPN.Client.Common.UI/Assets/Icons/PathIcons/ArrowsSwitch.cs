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

public class ArrowsSwitch : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M12.4 1.144a.5.5 0 1 0-.702.712L13.363 3.5H3.43A1.5 1.5 0 0 0 1.93 5v1.5h1V5a.5.5 0 0 1 .5-.5h9.934l-1.665 1.644a.5.5 0 1 0 .703.712l2.387-2.358a.7.7 0 0 0 0-.996l-2.387-2.358Zm.668 7.356V11a.5.5 0 0 1-.5.5H2.634l1.665-1.644a.5.5 0 0 0-.703-.712L1.21 11.502a.7.7 0 0 0 0 .996l2.387 2.358a.5.5 0 0 0 .703-.712L2.634 12.5h9.934a1.5 1.5 0 0 0 1.5-1.5V8.5h-1Z";

    protected override string IconGeometry20 { get; }
        = "M15.5 1.43a.625.625 0 0 0-.878.89l2.082 2.055H4.285c-1.035 0-1.875.84-1.875 1.875v1.875h1.25V6.25c0-.345.28-.625.625-.625h12.419L14.622 7.68a.625.625 0 0 0 .879.89l2.984-2.947a.875.875 0 0 0 0-1.246L15.501 1.43Zm.835 9.195v3.125c0 .345-.28.625-.625.625H3.292l2.081-2.055a.625.625 0 1 0-.878-.89l-2.984 2.947a.875.875 0 0 0 0 1.246l2.984 2.947a.625.625 0 0 0 .878-.89l-2.081-2.055H15.71c1.036 0 1.875-.84 1.875-1.875v-3.125h-1.25Z"; 

    protected override string IconGeometry24 { get; }
        = "M18.6 1.716a.75.75 0 0 0-1.053 1.068l2.497 2.466H5.143a2.25 2.25 0 0 0-2.25 2.25v2.25h1.5V7.5a.75.75 0 0 1 .75-.75h14.901l-2.497 2.466a.75.75 0 0 0 1.054 1.068l3.581-3.537a1.05 1.05 0 0 0 0-1.494l-3.581-3.537Zm1.002 11.034v3.75a.75.75 0 0 1-.75.75H3.95l2.498-2.466a.75.75 0 0 0-1.054-1.068l-3.581 3.537a1.05 1.05 0 0 0 0 1.494l3.581 3.537a.75.75 0 0 0 1.054-1.068L3.95 18.75h14.902a2.25 2.25 0 0 0 2.25-2.25v-3.75h-1.5Z"; 

    protected override string IconGeometry32 { get; }
        = "M24.8 2.288a1 1 0 0 0-1.404 1.424L26.726 7H6.856a3 3 0 0 0-3 3v3h2v-3a1 1 0 0 1 1-1h19.87l-3.33 3.288a1 1 0 0 0 1.405 1.424l4.775-4.716a1.4 1.4 0 0 0 0-1.992l-4.775-4.716ZM26.137 17v5a1 1 0 0 1-1 1H5.267l3.33-3.288a1 1 0 1 0-1.405-1.424l-4.775 4.716a1.4 1.4 0 0 0 0 1.992l4.775 4.716a1 1 0 1 0 1.405-1.424L5.267 25h19.87a3 3 0 0 0 3-3v-5h-2Z";
}