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

public class QuestionCircle : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M7 6.648a1.5 1.5 0 0 1 1.5-1.5h.148C9.395 5.148 10 5.754 10 6.5c0 .25-.118.487-.319.637L9 7.648a2.5 2.5 0 0 0-1 2 .5.5 0 0 0 1 0 1.5 1.5 0 0 1 .6-1.2l.681-.51c.453-.34.719-.872.719-1.438a2.352 2.352 0 0 0-2.352-2.352H8.5a2.5 2.5 0 0 0-2.5 2.5.5.5 0 0 0 1 0Z M8.5 12.75a.75.75 0 1 0 0-1.5.75.75 0 0 0 0 1.5Z M1 8.5a7.5 7.5 0 1 1 15 0 7.5 7.5 0 0 1-15 0ZM8.5 2a6.5 6.5 0 1 0 0 13 6.5 6.5 0 0 0 0-13Z";
}