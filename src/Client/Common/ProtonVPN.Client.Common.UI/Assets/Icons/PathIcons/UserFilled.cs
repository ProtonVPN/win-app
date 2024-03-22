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

public class UserFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8 8a3 3 0 1 0 0-6 3 3 0 0 0 0 6ZM5.64 9a3.09 3.09 0 0 0-2.636 1.457l-.783 1.271C1.607 12.728 2.345 14 3.54 14h8.92c1.195 0 1.933-1.272 1.318-2.272l-.782-1.271A3.092 3.092 0 0 0 10.36 9H5.641Z";

    protected override string IconGeometry20 { get; }
        = "M10 10a3.75 3.75 0 1 0 0-7.5 3.75 3.75 0 0 0 0 7.5Zm-2.949 1.25c-1.35 0-2.602.691-3.296 1.82l-.978 1.59c-.769 1.25.154 2.84 1.648 2.84h11.15c1.494 0 2.417-1.59 1.648-2.84l-.978-1.59a3.865 3.865 0 0 0-3.296-1.82H7.051Z"; 

    protected override string IconGeometry24 { get; }
        = "M12 12a4.5 4.5 0 1 0 0-9 4.5 4.5 0 0 0 0 9Zm-3.539 1.5c-1.62 0-3.122.83-3.955 2.185l-1.174 1.907C2.41 19.092 3.517 21 5.31 21h13.38c1.793 0 2.9-1.908 1.978-3.408l-1.174-1.907c-.833-1.356-2.335-2.185-3.955-2.185H8.46Z"; 

    protected override string IconGeometry32 { get; }
        = "M16 16a6 6 0 1 0 0-12 6 6 0 0 0 0 12Zm-4.718 2c-2.16 0-4.163 1.106-5.275 2.913l-1.564 2.543C3.213 25.456 4.69 28 7.08 28h17.84c2.39 0 3.867-2.544 2.637-4.544l-1.564-2.543C24.88 19.106 22.879 18 20.718 18h-9.436Z";
}