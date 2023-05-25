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

public class CalendarRow : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M3 1.5v.67A3.001 3.001 0 0 0 1 5v7a3 3 0 0 0 3 3h8a3 3 0 0 0 3-3V5a3.001 3.001 0 0 0-2-2.83V1.5a.5.5 0 0 0-1 0V2H4v-.5a.5.5 0 0 0-1 0ZM12 3H4a2 2 0 0 0-2 2h12a2 2 0 0 0-2-2Zm2 3H2v6a2 2 0 0 0 2 2h8a2 2 0 0 0 2-2V6Zm-2.5 2h-7a.5.5 0 0 0 0 1h7a.5.5 0 0 0 0-1Zm-7-1a1.5 1.5 0 1 0 0 3h7a1.5 1.5 0 0 0 0-3h-7Z";
}