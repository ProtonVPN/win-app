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

public class Calendar3Day : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M4 3V1.5a.5.5 0 0 1 1 0V3h6V1.5a.5.5 0 0 1 1 0V3a3 3 0 0 1 3 3v6a3 3 0 0 1-3 3H4a3 3 0 0 1-3-3V6a3 3 0 0 1 3-3Zm0 1h1v10H4a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2Zm7 0v10h1a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2h-1ZM6 4v10h4V4H6Z";
}