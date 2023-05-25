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

public class ArrowLeftAndUp : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M1.851 5.775a.496.496 0 0 1-.705-.002.504.504 0 0 1 .003-.71l3.86-3.859a.695.695 0 0 1 .983 0l3.86 3.859a.504.504 0 0 1 .001.71.496.496 0 0 1-.704.002L6 2.626v9.87a.5.5 0 0 0 .5.5H14v1H6.5a1.5 1.5 0 0 1-1.5-1.5v-9.87L1.851 5.775Z";
}