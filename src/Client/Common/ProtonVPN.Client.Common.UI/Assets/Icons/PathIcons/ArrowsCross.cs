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

public class ArrowsCross : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M10.294 4.418 9 3.124V8h4.876l-1.294-1.293a.5.5 0 0 1 .707-.708l2.006 2.006a.7.7 0 0 1 0 .99l-2.006 2.006a.5.5 0 0 1-.707-.707L13.875 9H9v4.876l1.294-1.294a.5.5 0 0 1 .707.707l-2.006 2.006a.7.7 0 0 1-.99 0L5.999 13.29a.5.5 0 1 1 .707-.708L8 13.876V9H3.124l1.294 1.294a.5.5 0 1 1-.707.707L1.705 8.995a.7.7 0 0 1 0-.99L3.711 6a.5.5 0 0 1 .707.708L3.124 8H8V3.124L6.706 4.418A.5.5 0 0 1 6 3.71l2.006-2.006a.7.7 0 0 1 .99 0L11 3.711a.5.5 0 0 1-.707.707Z";
}