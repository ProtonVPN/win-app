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

public class ChevronsRight : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7.646 2.146a.5.5 0 0 0 0 .708L12.793 8l-5.147 5.146a.5.5 0 0 0 .708.708l5.5-5.5a.5.5 0 0 0 0-.708l-5.5-5.5a.5.5 0 0 0-.708 0Zm-5.5 0a.5.5 0 0 0 0 .708L7.293 8l-5.147 5.146a.5.5 0 0 0 .708.708l5.5-5.5a.5.5 0 0 0 0-.708l-5.5-5.5a.5.5 0 0 0-.708 0Z";

    protected override string IconGeometry20 { get; }
        = "M9.558 2.683a.625.625 0 0 0 0 .884L15.991 10l-6.433 6.433a.625.625 0 1 0 .884.884l6.875-6.875a.625.625 0 0 0 0-.884l-6.875-6.875a.625.625 0 0 0-.884 0Zm-6.875 0a.625.625 0 0 0 0 .884L9.116 10l-6.433 6.433a.625.625 0 1 0 .884.884l6.875-6.875a.625.625 0 0 0 0-.884L3.567 2.683a.625.625 0 0 0-.884 0Z"; 

    protected override string IconGeometry24 { get; }
        = "M11.47 3.22a.75.75 0 0 0 0 1.06L19.19 12l-7.72 7.72a.75.75 0 1 0 1.06 1.06l8.25-8.25a.75.75 0 0 0 0-1.06l-8.25-8.25a.75.75 0 0 0-1.06 0Zm-8.25 0a.75.75 0 0 0 0 1.06L10.94 12l-7.72 7.72a.75.75 0 1 0 1.06 1.06l8.25-8.25a.75.75 0 0 0 0-1.06L4.28 3.22a.75.75 0 0 0-1.06 0Z"; 

    protected override string IconGeometry32 { get; }
        = "M15.293 4.293a1 1 0 0 0 0 1.414L25.586 16 15.293 26.293a1 1 0 0 0 1.414 1.414l11-11a1 1 0 0 0 0-1.414l-11-11a1 1 0 0 0-1.414 0Zm-11 0a1 1 0 0 0 0 1.414L14.586 16 4.293 26.293a1 1 0 1 0 1.414 1.414l11-11a1 1 0 0 0 0-1.414l-11-11a1 1 0 0 0-1.414 0Z";
}