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

public class PaintRoller : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M2 3a1 1 0 0 1 1-1h8a1 1 0 0 1 1 1v2a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V3Zm1-2a2 2 0 0 0-2 2v2a2 2 0 0 0 2 2h8a2 2 0 0 0 2-2V4h.5a.5.5 0 0 1 .5.5v3a.5.5 0 0 1-.5.5h-5A1.5 1.5 0 0 0 7 9.5v.5a1 1 0 0 0-1 1v4a1 1 0 0 0 1 1h1a1 1 0 0 0 1-1v-4a1 1 0 0 0-1-1v-.5a.5.5 0 0 1 .5-.5h5A1.5 1.5 0 0 0 15 7.5v-3A1.5 1.5 0 0 0 13.5 3H13a2 2 0 0 0-2-2H3Zm4 10v4h1v-4H7Z";
}