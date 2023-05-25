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

public class Filter : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M7.003 9.123v3.568l1.994.998V9.123L14.377 3H1.623l5.38 6.123Zm-6.3-5.655 5.3 6.032V13a.5.5 0 0 0 .276.447l2.995 1.5a.5.5 0 0 0 .723-.447v-5l5.3-6.032A.9.9 0 0 0 14.6 2H1.4a.9.9 0 0 0-.697 1.468Z";
}