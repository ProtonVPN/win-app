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

public class Backspace : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M5.518 2a2.5 2.5 0 0 0-2.021 1.03L.096 7.706a.5.5 0 0 0 0 .588l3.4 4.676A2.5 2.5 0 0 0 5.519 14H15.5a.5.5 0 0 0 .5-.5v-11a.5.5 0 0 0-.5-.5H5.518ZM4.305 3.618A1.5 1.5 0 0 1 5.518 3H15v10H5.518a1.5 1.5 0 0 1-1.213-.618L1.118 8l3.187-4.382Zm3.549 2.028a.5.5 0 1 0-.708.708L8.793 8 7.146 9.646a.5.5 0 0 0 .708.708L9.5 8.707l1.646 1.647a.5.5 0 0 0 .708-.708L10.207 8l1.647-1.646a.5.5 0 0 0-.708-.708L9.5 7.293 7.854 5.646Z";
}