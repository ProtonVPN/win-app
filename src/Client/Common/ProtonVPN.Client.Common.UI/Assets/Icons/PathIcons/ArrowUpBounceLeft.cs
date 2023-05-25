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

public class ArrowUpBounceLeft : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M4.707 2h2.329a.5.5 0 1 0 0-1H3.7a.7.7 0 0 0-.7.7v3.336a.5.5 0 1 0 1 0V2.707l4.94 4.94a.5.5 0 0 1 0 .707l-5.794 5.792a.5.5 0 0 0 .708.708L9.646 9.06a1.5 1.5 0 0 0 0-2.122L4.707 2ZM12.5 1a.5.5 0 0 1 .5.5v13a.5.5 0 0 1-1 0v-13a.5.5 0 0 1 .5-.5Z";
}