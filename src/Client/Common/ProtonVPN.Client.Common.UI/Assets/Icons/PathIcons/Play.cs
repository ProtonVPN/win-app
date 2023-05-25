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

public class Play : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M5 4.002c0-.83.954-1.3 1.612-.793l5.197 3.997a1.002 1.002 0 0 1 0 1.588L6.612 12.79A1.002 1.002 0 0 1 5 11.997V4.002Zm1 0v7.996l.002.001L11.2 8h.001v-.002L6.002 4h-.001A.022.022 0 0 0 6 4Z";
}