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

public class FolderArrowUp : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M1 4a2 2 0 0 1 2-2h3.028c.388 0 .77.09 1.118.264l1.155.578A1.5 1.5 0 0 0 8.972 3H13a2 2 0 0 1 2 2v7a2 2 0 0 1-2 2h-3v-1h3a1 1 0 0 0 1-1V5a1 1 0 0 0-1-1H8.972a2.5 2.5 0 0 1-1.118-.264L6.7 3.158A1.5 1.5 0 0 0 6.028 3H3a1 1 0 0 0-1 1v8a1 1 0 0 0 1 1h4v1H3a2 2 0 0 1-2-2V4Z M8.854 6.646a.5.5 0 0 0-.708 0l-2 2a.5.5 0 1 0 .708.708L8 8.207V14h1V8.207l1.146 1.147a.5.5 0 0 0 .708-.708l-2-2Z";
}