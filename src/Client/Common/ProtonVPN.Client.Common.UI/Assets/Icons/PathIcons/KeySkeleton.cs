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

public class KeySkeleton : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M13 4.414a2.414 2.414 0 1 1-4.829 0 2.414 2.414 0 0 1 4.829 0Zm1 0A3.414 3.414 0 0 1 8.554 7.16l-3.832 3.89 1.373 1.373a.5.5 0 1 1-.707.707l-1.37-1.37L3 12.775l1.37 1.37a.5.5 0 1 1-.706.708l-1.371-1.371a1 1 0 0 1 0-1.414l1.37-1.37 4.183-4.246A3.414 3.414 0 1 1 14 4.414Z";
}