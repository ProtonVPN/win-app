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

public class ArrowLeftAndUp : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M1.851 5.776a.496.496 0 0 1-.705-.003.504.504 0 0 1 .003-.71l3.86-3.858a.695.695 0 0 1 .983 0L9.85 5.064a.504.504 0 0 1 .002.709.496.496 0 0 1-.705.002L6 2.627v9.87a.5.5 0 0 0 .5.5H14v1H6.5a1.5 1.5 0 0 1-1.5-1.5v-9.87L1.85 5.776Z";

    protected override string IconGeometry20 { get; }
        = "M2.314 7.22a.62.62 0 0 1-.881-.004.63.63 0 0 1 .003-.886L6.26 1.505a.87.87 0 0 1 1.23 0l4.824 4.824a.63.63 0 0 1 .003.887.62.62 0 0 1-.881.003L7.5 3.283v12.338c0 .345.28.625.625.625H17.5v1.25H8.125a1.875 1.875 0 0 1-1.875-1.875V3.283L2.314 7.22Z"; 

    protected override string IconGeometry24 { get; }
        = "M2.777 8.663a.744.744 0 0 1-1.057-.004.756.756 0 0 1 .003-1.064l5.79-5.788a1.042 1.042 0 0 1 1.475 0l5.789 5.788c.293.293.294.77.003 1.064a.744.744 0 0 1-1.057.003L9 3.94v14.805c0 .414.336.75.75.75H21v1.5H9.75a2.25 2.25 0 0 1-2.25-2.25V3.94L2.777 8.663Z"; 

    protected override string IconGeometry32 { get; }
        = "M3.703 11.55a.992.992 0 0 1-1.41-.004 1.008 1.008 0 0 1 .004-1.419l7.72-7.718a1.39 1.39 0 0 1 1.967 0l7.718 7.718c.39.39.393 1.025.005 1.418a.992.992 0 0 1-1.41.005L12 5.253v19.74a1 1 0 0 0 1 1h15v2H13a3 3 0 0 1-3-3V5.254l-6.297 6.298Z";
}