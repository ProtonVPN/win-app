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

public class Eye : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M8 12c-2.186 0-4.476-1.26-5.987-4C3.523 5.26 5.814 4 8 4s4.476 1.26 5.987 4c-1.51 2.74-3.8 4-5.987 4Zm6.89-4.434c-3.32-6.088-10.46-6.088-13.78 0a.909.909 0 0 0 0 .868c3.32 6.088 10.46 6.088 13.78 0a.908.908 0 0 0 0-.868ZM8 6a2 2 0 0 1-2.989 1.739A3 3 0 1 0 7.74 5.01c.166.292.261.63.261.989Z";
}