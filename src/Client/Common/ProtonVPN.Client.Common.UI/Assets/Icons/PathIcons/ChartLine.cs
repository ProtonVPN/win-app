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

public class ChartLine : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M2 2.5a.5.5 0 0 0-1 0v10A2.5 2.5 0 0 0 3.5 15h10a.5.5 0 0 0 0-1h-10A1.5 1.5 0 0 1 2 12.5v-10Z M13.854 5.854a.5.5 0 0 0-.708-.708L10 8.293 7.854 6.146a.5.5 0 0 0-.708 0l-3.5 3.5a.5.5 0 0 0 .708.708L7.5 7.207l2.146 2.147a.5.5 0 0 0 .708 0l3.5-3.5Z";
}