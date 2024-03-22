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

public class ArrowDownArrowUp : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M4.5 1a.5.5 0 0 1 .5.5v11.793l2.146-2.146a.5.5 0 1 1 .708.707l-3 3a.5.5 0 0 1-.708 0l-3-3a.5.5 0 1 1 .708-.707L4 13.293V1.5a.5.5 0 0 1 .5-.5Zm7 0a.5.5 0 0 1 .354.146l3 3a.5.5 0 0 1-.708.708L12 2.707V14.48a.5.5 0 1 1-1 0V2.707L8.854 4.854a.5.5 0 1 1-.708-.708l3-3A.5.5 0 0 1 11.5 1Z";

    protected override string IconGeometry20 { get; }
        = "M5.625 1.25c.345 0 .625.28.625.625v14.741l2.683-2.683a.625.625 0 1 1 .884.884l-3.75 3.75a.625.625 0 0 1-.884 0l-3.75-3.75a.625.625 0 1 1 .884-.884L5 16.616V1.875c0-.345.28-.625.625-.625Zm8.75 0c.166 0 .325.066.442.183l3.75 3.75a.625.625 0 1 1-.884.884L15 3.384V18.1a.625.625 0 1 1-1.25 0V3.384l-2.683 2.683a.625.625 0 1 1-.884-.884l3.75-3.75a.625.625 0 0 1 .442-.183Z"; 

    protected override string IconGeometry24 { get; }
        = "M6.75 1.5a.75.75 0 0 1 .75.75v17.69l3.22-3.22a.75.75 0 1 1 1.06 1.06l-4.5 4.5a.75.75 0 0 1-1.06 0l-4.5-4.5a.75.75 0 1 1 1.06-1.06L6 19.94V2.25a.75.75 0 0 1 .75-.75Zm10.5 0a.75.75 0 0 1 .53.22l4.5 4.5a.75.75 0 0 1-1.06 1.06L18 4.06v17.66a.75.75 0 0 1-1.5 0V4.06l-3.22 3.22a.75.75 0 1 1-1.06-1.06l4.5-4.5a.75.75 0 0 1 .53-.22Z"; 

    protected override string IconGeometry32 { get; }
        = "M9 2a1 1 0 0 1 1 1v23.586l4.293-4.293a1 1 0 0 1 1.414 1.414l-6 6a1 1 0 0 1-1.414 0l-6-6a1 1 0 1 1 1.414-1.414L8 26.586V3a1 1 0 0 1 1-1Zm14 0a1 1 0 0 1 .707.293l6 6a1 1 0 0 1-1.414 1.414L24 5.414V28.96a1 1 0 1 1-2 0V5.414l-4.293 4.293a1 1 0 0 1-1.414-1.414l6-6A1 1 0 0 1 23 2Z";
}