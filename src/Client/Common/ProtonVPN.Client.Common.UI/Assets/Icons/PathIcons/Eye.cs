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
    protected override string IconGeometry16 { get; }
        = "M8 12c-2.186 0-4.476-1.26-5.987-4C3.523 5.26 5.814 4 8 4s4.476 1.26 5.987 4c-1.51 2.74-3.8 4-5.987 4Zm6.89-4.434c-3.32-6.088-10.46-6.088-13.78 0a.909.909 0 0 0 0 .868c3.32 6.088 10.46 6.088 13.78 0a.908.908 0 0 0 0-.868ZM8 6a2 2 0 0 1-2.989 1.739A3 3 0 1 0 7.74 5.01c.166.292.261.63.261.989Z";

    protected override string IconGeometry20 { get; }
        = "M10 15c-2.733 0-5.595-1.576-7.484-5C4.405 6.576 7.267 5 10 5c2.733 0 5.595 1.576 7.484 5-1.889 3.424-4.751 5-7.484 5Zm8.612-5.542c-4.15-7.61-13.075-7.61-17.224 0a1.136 1.136 0 0 0 0 1.084c4.15 7.61 13.075 7.61 17.224 0a1.135 1.135 0 0 0 0-1.084ZM10 7.5a2.5 2.5 0 0 1-3.736 2.174 3.75 3.75 0 1 0 3.41-3.41C9.88 6.629 10 7.05 10 7.5Z"; 

    protected override string IconGeometry24 { get; }
        = "M12 18c-3.279 0-6.715-1.891-8.98-6C5.284 7.891 8.72 6 12 6c3.28 0 6.715 1.891 8.98 6-2.265 4.109-5.7 6-8.98 6Zm10.335-6.65c-4.98-9.133-15.69-9.133-20.67 0-.22.404-.22.896 0 1.3 4.98 9.133 15.69 9.133 20.67 0 .22-.404.22-.896 0-1.3ZM12 9a3 3 0 0 1-4.483 2.608 4.5 4.5 0 1 0 4.091-4.091c.25.437.392.943.392 1.483Z"; 

    protected override string IconGeometry32 { get; }
        = "M16 24c-4.372 0-8.953-2.522-11.974-8C7.047 10.522 11.628 8 16 8s8.953 2.522 11.974 8c-3.021 5.478-7.602 8-11.974 8Zm13.78-8.868c-6.64-12.176-20.92-12.176-27.56 0a1.817 1.817 0 0 0 0 1.736c6.64 12.176 20.92 12.176 27.56 0a1.817 1.817 0 0 0 0-1.736ZM16 12a4 4 0 0 1-5.978 3.478 6 6 0 1 0 5.455-5.455C15.81 10.605 16 11.28 16 12Z";
}