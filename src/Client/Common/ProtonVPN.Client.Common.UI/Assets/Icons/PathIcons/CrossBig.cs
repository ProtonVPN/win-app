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

public class CrossBig : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M2.146 2.146a.5.5 0 0 1 .708 0L8 7.293l5.146-5.147a.5.5 0 0 1 .708.708L8.707 8l5.147 5.146a.5.5 0 0 1-.708.708L8 8.707l-5.146 5.147a.5.5 0 0 1-.708-.708L7.293 8 2.146 2.854a.5.5 0 0 1 0-.708Z";

    protected override string IconGeometry20 { get; }
        = "M2.683 2.683a.625.625 0 0 1 .884 0L10 9.116l6.433-6.433a.625.625 0 1 1 .884.884L10.884 10l6.433 6.433a.625.625 0 1 1-.884.884L10 10.884l-6.433 6.433a.625.625 0 1 1-.884-.884L9.116 10 2.683 3.567a.625.625 0 0 1 0-.884Z"; 

    protected override string IconGeometry24 { get; }
        = "M3.22 3.22a.75.75 0 0 1 1.06 0L12 10.94l7.72-7.72a.75.75 0 1 1 1.06 1.06L13.06 12l7.72 7.72a.75.75 0 1 1-1.06 1.06L12 13.06l-7.72 7.72a.75.75 0 0 1-1.06-1.06L10.94 12 3.22 4.28a.75.75 0 0 1 0-1.06Z"; 

    protected override string IconGeometry32 { get; }
        = "";
}