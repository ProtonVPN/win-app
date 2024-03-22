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

public class CheckmarkCircleFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8 15A7 7 0 1 0 8 1a7 7 0 0 0 0 14Zm3.854-8.146a.5.5 0 0 0-.708-.708L7.5 9.793 5.354 7.646a.5.5 0 1 0-.708.708l2.359 2.358a.7.7 0 0 0 .99 0l3.859-3.858Z";

    protected override string IconGeometry20 { get; }
        = "M10 18.75a8.75 8.75 0 1 0 0-17.5 8.75 8.75 0 0 0 0 17.5Zm4.817-10.183a.625.625 0 1 0-.884-.884l-4.558 4.558-2.683-2.683a.625.625 0 0 0-.884.884l2.948 2.948a.875.875 0 0 0 1.238 0l4.823-4.823Z"; 

    protected override string IconGeometry24 { get; }
        = "M12 22.5c5.799 0 10.5-4.701 10.5-10.5S17.799 1.5 12 1.5 1.5 6.201 1.5 12 6.201 22.5 12 22.5Zm5.78-12.22a.75.75 0 1 0-1.06-1.06l-5.47 5.47-3.22-3.22a.75.75 0 0 0-1.06 1.06l3.538 3.538c.41.41 1.074.41 1.485 0l5.787-5.788Z"; 

    protected override string IconGeometry32 { get; }
        = "M16 30c7.732 0 14-6.268 14-14S23.732 2 16 2 2 8.268 2 16s6.268 14 14 14Zm7.707-16.293a1 1 0 0 0-1.414-1.414L15 19.586l-4.293-4.293a1 1 0 0 0-1.414 1.414l4.717 4.717a1.4 1.4 0 0 0 1.98 0l7.717-7.717Z";
}