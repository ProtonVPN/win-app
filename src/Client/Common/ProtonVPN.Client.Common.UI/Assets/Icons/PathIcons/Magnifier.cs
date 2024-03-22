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

public class Magnifier : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M11 6.5a4.5 4.5 0 1 1-9 0 4.5 4.5 0 0 1 9 0Zm-.98 4.227a5.5 5.5 0 1 1 .707-.707l3.127 3.126a.5.5 0 0 1-.708.708l-3.127-3.127Z";

    protected override string IconGeometry20 { get; }
        = "M13.75 8.125a5.625 5.625 0 1 1-11.25 0 5.625 5.625 0 0 1 11.25 0Zm-1.226 5.283a6.875 6.875 0 1 1 .884-.884l3.909 3.91a.625.625 0 1 1-.884.883l-3.909-3.909Z"; 

    protected override string IconGeometry24 { get; }
        = "M16.5 9.75a6.75 6.75 0 1 1-13.5 0 6.75 6.75 0 0 1 13.5 0Zm-1.47 6.34a8.25 8.25 0 1 1 1.06-1.06l4.69 4.69a.75.75 0 1 1-1.06 1.06l-4.69-4.69Z"; 

    protected override string IconGeometry32 { get; }
        = "M22 13a9 9 0 1 1-18 0 9 9 0 0 1 18 0Zm-1.96 8.453A10.956 10.956 0 0 1 13 24C6.925 24 2 19.075 2 13S6.925 2 13 2s11 4.925 11 11c0 2.678-.957 5.132-2.547 7.04l6.254 6.253a1 1 0 0 1-1.414 1.414l-6.254-6.254Z";
}