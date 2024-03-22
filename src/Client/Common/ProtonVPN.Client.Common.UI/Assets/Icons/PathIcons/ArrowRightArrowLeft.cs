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

public class ArrowRightArrowLeft : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M1.146 11.146a.5.5 0 0 0 0 .708l3 3a.5.5 0 0 0 .708-.708L2.707 12H14.48a.5.5 0 0 0 0-1H2.707l2.147-2.146a.5.5 0 1 0-.708-.708l-3 3ZM1.02 4.5a.5.5 0 0 0 .5.5h11.773l-2.147 2.146a.5.5 0 0 0 .708.708l3-3a.5.5 0 0 0 0-.708l-3-3a.5.5 0 0 0-.708.708L13.293 4H1.52a.5.5 0 0 0-.5.5Z";

    protected override string IconGeometry20 { get; }
        = "M1.433 13.933a.625.625 0 0 0 0 .884l3.75 3.75a.625.625 0 1 0 .884-.884L3.384 15H18.1a.625.625 0 1 0 0-1.25H3.384l2.683-2.683a.625.625 0 1 0-.884-.884l-3.75 3.75Zm-.158-8.308c0 .345.28.625.625.625h14.716l-2.683 2.683a.625.625 0 1 0 .884.884l3.75-3.75a.625.625 0 0 0 0-.884l-3.75-3.75a.625.625 0 1 0-.884.884L16.616 5H1.9a.625.625 0 0 0-.625.625Z"; 

    protected override string IconGeometry24 { get; }
        = "M1.72 16.72a.75.75 0 0 0 0 1.06l4.5 4.5a.75.75 0 0 0 1.06-1.06L4.06 18h17.66a.75.75 0 0 0 0-1.5H4.06l3.22-3.22a.75.75 0 1 0-1.06-1.06l-4.5 4.5Zm-.19-9.97c0 .414.336.75.75.75h17.66l-3.22 3.22a.75.75 0 1 0 1.06 1.06l4.5-4.5a.75.75 0 0 0 0-1.06l-4.5-4.5a.75.75 0 1 0-1.06 1.06L19.94 6H2.28a.75.75 0 0 0-.75.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M2.293 22.293a1 1 0 0 0 0 1.414l6 6a1 1 0 0 0 1.414-1.414L5.414 24H28.96a1 1 0 1 0 0-2H5.414l4.293-4.293a1 1 0 0 0-1.414-1.414l-6 6ZM2.04 9a1 1 0 0 0 1 1h23.546l-4.293 4.293a1 1 0 0 0 1.414 1.414l6-6a1 1 0 0 0 0-1.414l-6-6a1 1 0 1 0-1.414 1.414L26.586 8H3.04a1 1 0 0 0-1 1Z";
}