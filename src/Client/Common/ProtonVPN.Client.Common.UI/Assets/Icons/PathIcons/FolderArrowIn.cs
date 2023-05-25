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

public class FolderArrowIn : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M7.793 9H1V8h6.793L6.646 6.854a.5.5 0 1 1 .708-.708l1.858 1.859a.7.7 0 0 1 0 .99l-1.858 1.859a.5.5 0 0 1-.708-.708L7.793 9Z M3 2a2 2 0 0 0-2 2v3h1V4a1 1 0 0 1 1-1h2.528a1.5 1.5 0 0 1 .67.158l1.156.578A2.5 2.5 0 0 0 8.472 4H13a1 1 0 0 1 1 1v7a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1v-2H1v2a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V5a2 2 0 0 0-2-2H8.472a1.5 1.5 0 0 1-.67-.158l-1.156-.578A2.5 2.5 0 0 0 5.528 2H3Z";
}