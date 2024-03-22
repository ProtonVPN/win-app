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

public class Circle : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8 12a4 4 0 1 0 0-8 4 4 0 0 0 0 8Zm0 1A5 5 0 1 0 8 3a5 5 0 0 0 0 10Z";

    protected override string IconGeometry20 { get; }
        = "M10 15a5 5 0 1 0 0-10 5 5 0 0 0 0 10Zm0 1.25a6.25 6.25 0 1 0 0-12.5 6.25 6.25 0 0 0 0 12.5Z"; 

    protected override string IconGeometry24 { get; }
        = "M12 18a6 6 0 1 0 0-12 6 6 0 0 0 0 12Zm0 1.5a7.5 7.5 0 1 0 0-15 7.5 7.5 0 0 0 0 15Z"; 

    protected override string IconGeometry32 { get; }
        = "M16 24a8 8 0 1 0 0-16 8 8 0 0 0 0 16Zm0 2c5.523 0 10-4.477 10-10S21.523 6 16 6 6 10.477 6 16s4.477 10 10 10Z";
}