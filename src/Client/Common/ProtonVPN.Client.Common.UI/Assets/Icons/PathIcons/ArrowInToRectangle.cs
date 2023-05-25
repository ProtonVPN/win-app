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

public class ArrowInToRectangle : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M7 1h6a2 2 0 0 1 2 2v11a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2v-1h1v1a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V3a1 1 0 0 0-1-1H7a1 1 0 0 0-1 1v1H5V3a2 2 0 0 1 2-2Zm-.354 4.646a.5.5 0 0 1 .708 0l2.358 2.359a.7.7 0 0 1 0 .99l-2.358 2.359a.5.5 0 0 1-.708-.708L8.293 9H1.5a.5.5 0 0 1 0-1h6.793L6.646 6.354a.5.5 0 0 1 0-.708Z";
}