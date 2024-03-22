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

public class ChevronsLeft : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8.354 2.146a.5.5 0 0 1 0 .708L3.207 8l5.147 5.146a.5.5 0 0 1-.708.708l-5.5-5.5a.5.5 0 0 1 0-.708l5.5-5.5a.5.5 0 0 1 .708 0Zm5.5 0a.5.5 0 0 1 0 .708L8.707 8l5.147 5.146a.5.5 0 0 1-.708.708l-5.5-5.5a.5.5 0 0 1 0-.708l5.5-5.5a.5.5 0 0 1 .708 0Z";

    protected override string IconGeometry20 { get; }
        = "M10.442 2.683a.625.625 0 0 1 0 .884L4.009 10l6.433 6.433a.625.625 0 1 1-.884.884l-6.875-6.875a.625.625 0 0 1 0-.884l6.875-6.875a.625.625 0 0 1 .884 0Zm6.875 0a.625.625 0 0 1 0 .884L10.884 10l6.433 6.433a.625.625 0 1 1-.884.884l-6.875-6.875a.625.625 0 0 1 0-.884l6.875-6.875a.625.625 0 0 1 .884 0Z"; 

    protected override string IconGeometry24 { get; }
        = "M12.53 3.22a.75.75 0 0 1 0 1.06L4.81 12l7.72 7.72a.75.75 0 1 1-1.06 1.06l-8.25-8.25a.75.75 0 0 1 0-1.06l8.25-8.25a.75.75 0 0 1 1.06 0Zm8.25 0a.75.75 0 0 1 0 1.06L13.06 12l7.72 7.72a.75.75 0 1 1-1.06 1.06l-8.25-8.25a.75.75 0 0 1 0-1.06l8.25-8.25a.75.75 0 0 1 1.06 0Z"; 

    protected override string IconGeometry32 { get; }
        = "M16.707 4.293a1 1 0 0 1 0 1.414L6.414 16l10.293 10.293a1 1 0 0 1-1.414 1.414l-11-11a1 1 0 0 1 0-1.414l11-11a1 1 0 0 1 1.414 0Zm11 0a1 1 0 0 1 0 1.414L17.414 16l10.293 10.293a1 1 0 0 1-1.414 1.414l-11-11a1 1 0 0 1 0-1.414l11-11a1 1 0 0 1 1.414 0Z";
}