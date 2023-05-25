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

public class MapPin : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M8 2C5.783 2 4 3.78 4 5.96a5.88 5.88 0 0 0 .906 3.133l2.929 4.648A11.062 11.062 0 0 0 8 14l.012-.018c.034-.05.078-.12.153-.24l2.929-4.648A5.88 5.88 0 0 0 12 5.961C12 3.78 10.217 2 8 2ZM4.06 9.626A6.877 6.877 0 0 1 3 5.96C3 3.22 5.239 1 8 1s5 2.22 5 4.96a6.877 6.877 0 0 1-1.06 3.666l-2.929 4.648c-.143.228-.215.342-.282.413a1.006 1.006 0 0 1-1.458 0c-.067-.07-.139-.185-.282-.413L4.06 9.626Z M8 7a1 1 0 1 0 0-2 1 1 0 0 0 0 2Zm0 1a2 2 0 1 0 0-4 2 2 0 0 0 0 4Z";
}