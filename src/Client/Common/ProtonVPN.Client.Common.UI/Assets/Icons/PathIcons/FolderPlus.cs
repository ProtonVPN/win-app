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

public class FolderPlus : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M0 4.05a2 2 0 0 1 2-2h2.528c.388 0 .77.09 1.118.264L6.8 2.89a1.5 1.5 0 0 0 .671.159H13a2 2 0 0 1 2 2v7a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2v-8Zm2-1a1 1 0 0 0-1 1v8a1 1 0 0 0 1 1h11a1 1 0 0 0 1-1v-7a1 1 0 0 0-1-1H7.472a2.5 2.5 0 0 1-1.118-.264L5.2 3.208a1.5 1.5 0 0 0-.671-.158H2Z M7.5 6a.5.5 0 0 1 .5.5V8h1.5a.5.5 0 0 1 0 1H8v1.5a.5.5 0 0 1-1 0V9H5.5a.5.5 0 1 1 0-1H7V6.5a.5.5 0 0 1 .5-.5Z";
}