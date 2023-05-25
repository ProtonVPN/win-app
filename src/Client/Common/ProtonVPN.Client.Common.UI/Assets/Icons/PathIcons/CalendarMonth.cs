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

public class CalendarMonth : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M11 3H5V1.5a.5.5 0 0 0-1 0V3a3 3 0 0 0-3 3v6a3 3 0 0 0 3 3h8a3 3 0 0 0 3-3V6a3 3 0 0 0-3-3V1.5a.5.5 0 0 0-1 0V3ZM4 4h1v2H2a2 2 0 0 1 2-2Zm1 3H2v4h3V7Zm1 4V7h4v4H6Zm-1 1H2a2 2 0 0 0 2 2h1v-2Zm1 2h4v-2H6v2Zm0-8V4h4v2H6Zm5 0V4h1a2 2 0 0 1 2 2h-3Zm0 1v4h3V7h-3Zm3 5a2 2 0 0 1-2 2h-1v-2h3Z";
}