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

public class ArrowRotateRight : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M7.146 1.646a.5.5 0 0 1 .708 0l1.358 1.359a.7.7 0 0 1 0 .99L7.854 5.354a.5.5 0 1 1-.708-.708l.642-.642a5 5 0 1 0 3.52 1.246.5.5 0 0 1 .66-.75 6 6 0 1 1-4.173-1.497l-.649-.65a.5.5 0 0 1 0-.707Z";
}