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

public class ArrowUpAndLeft : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "m2.64 6.998 3.206-3.141a.5.5 0 1 0-.7-.714L1.21 6.999a.7.7 0 0 0 0 1l3.936 3.858a.5.5 0 0 0 .7-.714L2.64 7.998h9.858a1.5 1.5 0 0 1 1.5 1.5V12a.5.5 0 1 0 1 0V9.498a2.5 2.5 0 0 0-2.5-2.5H2.639Z";

    protected override string IconGeometry20 { get; }
        = "M3.3 8.748 7.307 4.82a.625.625 0 1 0-.875-.892l-4.92 4.82a.875.875 0 0 0 0 1.249l4.92 4.823a.625.625 0 0 0 .875-.892l-4.01-3.931H15.62c1.036 0 1.875.84 1.875 1.875V15a.625.625 0 1 0 1.25 0v-3.127c0-1.726-1.4-3.125-3.125-3.125H3.299Z"; 

    protected override string IconGeometry24 { get; }
        = "m3.96 10.498 4.81-4.713a.75.75 0 1 0-1.05-1.07l-5.905 5.783a1.05 1.05 0 0 0 0 1.5l5.904 5.787a.75.75 0 0 0 1.05-1.07l-4.811-4.717h14.787a2.25 2.25 0 0 1 2.25 2.25V18a.75.75 0 0 0 1.5 0v-3.752a3.75 3.75 0 0 0-3.75-3.75H3.96Z"; 

    protected override string IconGeometry32 { get; }
        = "m5.279 13.997 6.414-6.283a1 1 0 1 0-1.4-1.428L2.42 13.997a1.4 1.4 0 0 0 0 2l7.872 7.717a1 1 0 0 0 1.4-1.428l-6.415-6.29h19.717a3 3 0 0 1 3 3V24a1 1 0 1 0 2 0v-5.003a5 5 0 0 0-5-5H5.279Z";
}