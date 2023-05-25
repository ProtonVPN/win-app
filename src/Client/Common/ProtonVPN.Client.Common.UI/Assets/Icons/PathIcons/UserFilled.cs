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
    protected override string IconGeometry { get; }
        = "M8 8a3 3 0 1 0 0-6 3 3 0 0 0 0 6ZM5.64 9a3.09 3.09 0 0 0-2.636 1.457l-.783 1.271C1.607 12.728 2.345 14 3.54 14h8.92c1.195 0 1.933-1.272 1.318-2.272l-.782-1.271A3.092 3.092 0 0 0 10.36 9H5.641Z";
}