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

public class FileArrowInUp : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M4.5 2A1.5 1.5 0 0 0 3 3.5v10A1.5 1.5 0 0 0 4.5 15H6v1H4.5A2.5 2.5 0 0 1 2 13.5v-10A2.5 2.5 0 0 1 4.5 1h5.672a2.5 2.5 0 0 1 1.767.732l2.329 2.329A2.5 2.5 0 0 1 15 5.828V13.5a2.5 2.5 0 0 1-2.5 2.5H11v-1h1.5a1.5 1.5 0 0 0 1.5-1.5V6h-2c-.828 0-2-.672-2-1.5V2H4.5Zm6.5.25V4.5a.5.5 0 0 0 .5.5h2.25a1.503 1.503 0 0 0-.19-.232l-2.328-2.329A1.497 1.497 0 0 0 11 2.25ZM8.005 7.288a.7.7 0 0 1 .99 0l1.859 1.858a.5.5 0 0 1-.708.708L9 8.707V15.5a.5.5 0 0 1-1 0V8.707L6.854 9.854a.5.5 0 1 1-.708-.708l1.859-1.858Z";
}