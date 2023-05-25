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

public class CalendarGrid : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M3 1.5v.67A3.001 3.001 0 0 0 1 5v7a3 3 0 0 0 3 3h8a3 3 0 0 0 3-3V5a3.001 3.001 0 0 0-2-2.83V1.5a.5.5 0 0 0-1 0V2H4v-.5a.5.5 0 0 0-1 0ZM4 3h8a2 2 0 0 1 2 2H2a2 2 0 0 1 2-2Zm6 3H6v2h4V6Zm0 3H6v2h4V9Zm1 2V9h3v2h-3Zm-1 1H6v2h4v-2Zm1 2v-2h3a2 2 0 0 1-2 2h-1Zm0-6V6h3v2h-3ZM2 6h3v2H2V6Zm0 3h3v2H2V9Zm0 3h3v2H4a2 2 0 0 1-2-2Z";
}