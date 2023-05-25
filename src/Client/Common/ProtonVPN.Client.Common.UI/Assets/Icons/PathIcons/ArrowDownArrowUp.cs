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
    protected override string IconGeometry { get; }
        = "M4.5 1a.5.5 0 0 1 .5.5v11.793l2.146-2.146a.5.5 0 1 1 .708.707l-3 3a.5.5 0 0 1-.708 0l-3-3a.5.5 0 1 1 .708-.707L4 13.293V1.5a.5.5 0 0 1 .5-.5Zm7 0a.5.5 0 0 1 .354.146l3 3a.5.5 0 0 1-.708.708L12 2.707V14.48a.5.5 0 1 1-1 0V2.707L8.854 4.854a.5.5 0 1 1-.708-.708l3-3A.5.5 0 0 1 11.5 1Z";
}