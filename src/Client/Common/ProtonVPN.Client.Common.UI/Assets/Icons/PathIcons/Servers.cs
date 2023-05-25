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

public class Servers : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M3 3h10a1 1 0 0 1 1 1v2a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V4a1 1 0 0 1 1-1ZM1 4a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v2a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V4Zm2 6h10a1 1 0 0 1 1 1v2a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1v-2a1 1 0 0 1 1-1Zm-2 1a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v2a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2v-2Zm12-5.5a.5.5 0 1 1-1 0 .5.5 0 0 1 1 0Zm-.5 7.5a.5.5 0 1 0 0-1 .5.5 0 0 0 0 1ZM11 5.5a.5.5 0 1 1-1 0 .5.5 0 0 1 1 0Zm-.5 7.5a.5.5 0 1 0 0-1 .5.5 0 0 0 0 1Z";
}