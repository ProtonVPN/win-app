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

public class ArrowUpBigLine : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M7 6.545v-1H5.317L8.5 2.169l3.183 3.376H10v6.478H7V6.545Zm4 0v5.853c0 .345-.28.625-.625.625h-3.75A.625.625 0 0 1 6 12.398V6.545H4.448a.625.625 0 0 1-.455-1.054l4.052-4.298a.625.625 0 0 1 .91 0l4.052 4.298a.625.625 0 0 1-.455 1.054H11ZM3.5 14a.5.5 0 1 0 0 1h10a.5.5 0 0 0 0-1h-10Z";
}