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

public class ArrowDownToSquare : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M8.995 9.854a.7.7 0 0 1-.99 0L5.646 7.495a.5.5 0 1 1 .708-.707L8 8.434V.5a.5.5 0 0 1 1 0v7.934l1.646-1.646a.5.5 0 0 1 .708.707L8.995 9.854Z M3 5a1 1 0 0 1 1-1h2.067V3H4a2 2 0 0 0-2 2v8a2 2 0 0 0 2 2h9a2 2 0 0 0 2-2V5a2 2 0 0 0-2-2h-2v1h2a1 1 0 0 1 1 1v8a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V5Z";
}