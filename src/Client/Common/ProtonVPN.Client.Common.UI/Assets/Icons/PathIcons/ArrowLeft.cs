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

public class ArrowLeft : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8.313 2.647a.5.5 0 0 1-.002.708L3.633 8h9.863a.5.5 0 1 1 0 1H3.633l4.678 4.645a.5.5 0 1 1-.705.71L2.21 8.994a.696.696 0 0 1-.199-.39.501.501 0 0 1 0-.211.696.696 0 0 1 .199-.39l5.396-5.359a.5.5 0 0 1 .707.002Z";

    protected override string IconGeometry20 { get; }
        = "M10.391 3.31a.625.625 0 0 1-.003.883L4.541 10h12.33a.625.625 0 1 1 0 1.25H4.54l5.847 5.806a.625.625 0 1 1-.88.887l-6.746-6.699a.87.87 0 0 1-.248-.487.628.628 0 0 1 0-.265.87.87 0 0 1 .248-.487l6.745-6.699a.625.625 0 0 1 .884.003Z"; 

    protected override string IconGeometry24 { get; }
        = "M12.47 3.971a.75.75 0 0 1-.004 1.06L5.45 12h14.795a.75.75 0 0 1 0 1.5H5.45l7.016 6.968a.75.75 0 0 1-1.057 1.064l-8.094-8.039a1.043 1.043 0 0 1-.298-.584.752.752 0 0 1 0-.318c.033-.214.132-.42.298-.585l8.094-8.039a.75.75 0 0 1 1.06.004Z"; 

    protected override string IconGeometry32 { get; }
        = "M16.626 5.295a1 1 0 0 1-.005 1.414L7.266 16h19.727a1 1 0 1 1 0 2H7.266l9.355 9.291a1 1 0 1 1-1.409 1.42L4.419 17.99a1.391 1.391 0 0 1-.396-.78 1.004 1.004 0 0 1 0-.423c.043-.286.175-.56.396-.78L15.212 5.29a1 1 0 0 1 1.414.005Z";
}