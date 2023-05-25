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

public class TagPlus : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M14 .5a.5.5 0 0 0-1 0V2h-1.5a.5.5 0 0 0 0 1H13v1.5a.5.5 0 0 0 1 0V3h1.5a.5.5 0 0 0 0-1H14V.5Zm-.444 8.925-4.95 4.95L2.182 7.95V3h4.95l6.424 6.425Zm-4.242 5.657a1 1 0 0 1-1.415 0L1.475 8.657a1 1 0 0 1-.293-.707V3a1 1 0 0 1 1-1h4.95a1 1 0 0 1 .707.293l6.424 6.425a1 1 0 0 1 0 1.414l-4.95 4.95ZM5.682 5.5a1 1 0 1 1-2 0 1 1 0 0 1 2 0Z";
}