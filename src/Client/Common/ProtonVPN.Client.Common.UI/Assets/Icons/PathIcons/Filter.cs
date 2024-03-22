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

public class Filter : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7.003 9.123v3.568l1.994.998V9.123L14.377 3H1.623l5.38 6.123Zm-6.3-5.655 5.3 6.032V13a.5.5 0 0 0 .276.447l2.995 1.5a.5.5 0 0 0 .723-.447v-5l5.3-6.032A.9.9 0 0 0 14.6 2H1.4a.9.9 0 0 0-.697 1.468Z";

    protected override string IconGeometry20 { get; }
        = "M8.754 11.404v4.46l2.492 1.248v-5.708l6.725-7.654H2.029l6.725 7.654ZM.879 4.334l6.625 7.541v4.375c0 .236.133.453.345.558l3.744 1.875a.624.624 0 0 0 .903-.558v-6.25l6.625-7.54c.597-.736.074-1.835-.871-1.835H1.75C.805 2.5.282 3.6.88 4.334Z"; 

    protected override string IconGeometry24 { get; }
        = "M10.504 13.685v5.351l2.992 1.498v-6.85l8.07-9.184H2.434l8.07 9.185ZM1.054 5.2l7.95 9.049v5.25c0 .283.16.543.414.67l4.494 2.25a.749.749 0 0 0 1.084-.67v-7.5l7.95-9.049c.714-.88.088-2.2-1.047-2.2H2.101C.965 3 .339 4.319 1.054 5.201Z"; 

    protected override string IconGeometry32 { get; }
        = "M14.006 18.246v7.136l3.988 1.997v-9.133L28.754 6H3.246l10.759 12.246Zm-12.6-11.31L12.006 19v7a1 1 0 0 0 .552.894l5.991 3a.999.999 0 0 0 1.445-.895V19l10.6-12.065C31.55 5.76 30.713 4 29.2 4H2.801C1.287 4 .45 5.759 1.406 6.935Z";
}