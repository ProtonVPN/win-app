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

public class CheckmarkCircle : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8 14A6 6 0 1 0 8 2a6 6 0 0 0 0 12Zm0 1A7 7 0 1 0 8 1a7 7 0 0 0 0 14Z M11.854 6.146a.5.5 0 0 1 0 .708l-3.859 3.858a.7.7 0 0 1-.99 0L4.646 8.354a.5.5 0 1 1 .708-.708L7.5 9.793l3.646-3.647a.5.5 0 0 1 .708 0Z";

    protected override string IconGeometry20 { get; }
        = "M10 17.5a7.5 7.5 0 1 0 0-15 7.5 7.5 0 0 0 0 15Zm0 1.25a8.75 8.75 0 1 0 0-17.5 8.75 8.75 0 0 0 0 17.5Z M14.817 7.683a.625.625 0 0 1 0 .884L9.994 13.39a.875.875 0 0 1-1.238 0l-2.948-2.948a.625.625 0 1 1 .884-.884l2.683 2.683 4.558-4.558a.625.625 0 0 1 .884 0Z"; 

    protected override string IconGeometry24 { get; }
        = "M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18Zm0 1.5c5.799 0 10.5-4.701 10.5-10.5S17.799 1.5 12 1.5 1.5 6.201 1.5 12 6.201 22.5 12 22.5Z M17.78 9.22a.75.75 0 0 1 0 1.06l-5.788 5.788a1.05 1.05 0 0 1-1.485 0L6.97 12.53a.75.75 0 1 1 1.06-1.06l3.22 3.22 5.47-5.47a.75.75 0 0 1 1.06 0Z"; 

    protected override string IconGeometry32 { get; }
        = "M16 28c6.627 0 12-5.373 12-12S22.627 4 16 4 4 9.373 4 16s5.373 12 12 12Zm0 2c7.732 0 14-6.268 14-14S23.732 2 16 2 2 8.268 2 16s6.268 14 14 14Z M23.707 12.293a1 1 0 0 1 0 1.414l-7.717 7.717a1.4 1.4 0 0 1-1.98 0l-4.717-4.717a1 1 0 1 1 1.414-1.414L15 19.586l7.293-7.293a1 1 0 0 1 1.414 0Z";
}