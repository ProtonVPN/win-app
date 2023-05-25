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

public class WindowTerminal : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M3 3h10a1 1 0 0 1 1 1H2a1 1 0 0 1 1-1ZM1 5V4a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V5Zm1 0h12v7a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V5Zm6.5 6H12a.5.5 0 0 0 0-1H8.5a.5.5 0 0 0 0 1ZM4.146 6.646a.5.5 0 0 1 .708 0l1.858 1.859a.7.7 0 0 1 0 .99l-1.858 1.859a.5.5 0 0 1-.708-.708L5.793 9 4.146 7.354a.5.5 0 0 1 0-.708Z";
}