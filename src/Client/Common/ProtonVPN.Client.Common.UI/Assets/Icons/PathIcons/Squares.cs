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

public class Squares : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M6.818 1A1.818 1.818 0 0 0 5 2.818V4h1V2.818C6 2.366 6.366 2 6.818 2H13a1 1 0 0 1 1 1v6a1 1 0 0 1-1 1h-1v1h1a2 2 0 0 0 2-2V3a2 2 0 0 0-2-2H6.818ZM2 7a1 1 0 0 1 1-1h6a1 1 0 0 1 1 1v6a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V7ZM1 7a2 2 0 0 1 2-2h6a2 2 0 0 1 2 2v6a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V7Z";
}