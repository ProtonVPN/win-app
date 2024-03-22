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

public class BrandProtonMailFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M2.61 2.21a1 1 0 0 0-1.612.792v7.995a2 2 0 0 0 2 2h10.004a2 2 0 0 0 2-2V3.002a1 1 0 0 0-1.611-.791L8.306 6.143a.5.5 0 0 1-.612 0L2.61 2.21Zm-.612.792 5.084 3.932c.18.139.383.231.595.278l-.518.463a.9.9 0 0 1-1.165.03L1.998 4.487V3.002Zm11.004 8.995h-.817v-7.59l1.817-1.405v7.995a1 1 0 0 1-1 1Z";

    protected override string IconGeometry20 { get; }
        = "M3.262 3.389c-.822-.635-2.015-.05-2.015.989v9.993a2.5 2.5 0 0 0 2.5 2.5h12.506a2.5 2.5 0 0 0 2.5-2.5V4.378c0-1.039-1.193-1.624-2.015-.99l-6.356 4.916a.625.625 0 0 1-.764 0L3.262 3.389Zm-.765.989 6.356 4.915c.225.173.479.29.743.347l-.647.58a1.125 1.125 0 0 1-1.456.037L2.497 6.234V4.378ZM16.253 15.62H15.23V6.135l2.272-1.757v9.993c0 .69-.56 1.25-1.25 1.25Z"; 

    protected override string IconGeometry24 { get; }
        = "M3.914 4.067c-.986-.763-2.418-.06-2.418 1.186v11.992a3 3 0 0 0 3 3h15.007a3 3 0 0 0 3-3V5.253c0-1.246-1.431-1.949-2.417-1.186l-7.627 5.897a.75.75 0 0 1-.918 0L3.914 4.067Zm-.918 1.186 7.627 5.898c.27.209.575.348.892.417l-.777.695a1.35 1.35 0 0 1-1.746.046L2.996 7.48V5.253Zm16.507 13.492h-1.226V7.362l2.726-2.109v11.992a1.5 1.5 0 0 1-1.5 1.5Z"; 

    protected override string IconGeometry32 { get; }
        = "M5.219 5.422c-1.315-1.016-3.224-.08-3.224 1.582v15.99a4 4 0 0 0 4 4h20.01a4 4 0 0 0 4-4V7.004c0-1.662-1.91-2.598-3.224-1.582l-10.17 7.864a1 1 0 0 1-1.223 0L5.218 5.422ZM3.995 7.004l10.17 7.864c.359.278.766.463 1.188.557l-1.035.926a1.8 1.8 0 0 1-2.33.06L3.996 9.975v-2.97Zm22.01 17.99h-1.636V9.815l3.636-2.81v15.989a2 2 0 0 1-2 2Z";
}