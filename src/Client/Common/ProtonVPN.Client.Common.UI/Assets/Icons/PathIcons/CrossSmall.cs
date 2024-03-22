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

public class CrossSmall : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M10.854 5.854a.5.5 0 0 0-.708-.708L8 7.293 5.854 5.146a.5.5 0 1 0-.708.708L7.293 8l-2.147 2.146a.5.5 0 0 0 .708.708L8 8.707l2.146 2.147a.5.5 0 0 0 .708-.708L8.707 8l2.147-2.146Z";

    protected override string IconGeometry20 { get; }
        = "M13.567 7.317a.625.625 0 1 0-.884-.884L10 9.116 7.317 6.433a.625.625 0 0 0-.884.884L9.116 10l-2.683 2.683a.625.625 0 1 0 .884.884L10 10.884l2.683 2.683a.625.625 0 0 0 .884-.884L10.884 10l2.683-2.683Z"; 

    protected override string IconGeometry24 { get; }
        = "M16.28 8.78a.75.75 0 1 0-1.06-1.06L12 10.94 8.78 7.72a.75.75 0 0 0-1.06 1.06L10.94 12l-3.22 3.22a.75.75 0 1 0 1.06 1.06L12 13.06l3.22 3.22a.75.75 0 0 0 1.06-1.06L13.06 12l3.22-3.22Z"; 

    protected override string IconGeometry32 { get; }
        = "";
}