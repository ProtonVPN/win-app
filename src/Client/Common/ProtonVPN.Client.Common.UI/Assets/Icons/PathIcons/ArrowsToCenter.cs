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

public class ArrowsToCenter : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M9.5 1.5a.5.5 0 0 1 .5.5v4.293l4.146-4.147a.5.5 0 1 1 .708.708L10.707 7H15a.5.5 0 0 1 0 1H9.5a.5.5 0 0 1-.5-.5V2a.5.5 0 0 1 .5-.5Zm-8 8A.5.5 0 0 1 2 9h5.5a.5.5 0 0 1 .5.5V15a.5.5 0 0 1-1 0v-4.293l-4.146 4.147a.5.5 0 0 1-.708-.708L6.293 10H2a.5.5 0 0 1-.5-.5Z";
}