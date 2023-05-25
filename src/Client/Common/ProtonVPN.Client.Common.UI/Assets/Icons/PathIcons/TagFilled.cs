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

public class TagFilled : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M10.132 15.082a1 1 0 0 1-1.414 0L2.293 8.657A1 1 0 0 1 2 7.95V3a1 1 0 0 1 1-1h4.95a1 1 0 0 1 .707.293l6.425 6.425a1 1 0 0 1 0 1.414l-4.95 4.95ZM5.5 6.5a1 1 0 1 0 0-2 1 1 0 0 0 0 2Z";
}