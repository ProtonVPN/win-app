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

public class Printer : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M11 7.5a.5.5 0 1 1-1 0 .5.5 0 0 1 1 0Z M12.5 8a.5.5 0 1 0 0-1 .5.5 0 0 0 0 1Z M13 4.035V5h.5A1.5 1.5 0 0 1 15 6.5v5a1.5 1.5 0 0 1-1.5 1.5H13v1a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1v-1h-.5A1.5 1.5 0 0 1 1 11.5v-5A1.5 1.5 0 0 1 2.5 5H3V2.5A1.5 1.5 0 0 1 4.5 1h4.697a1.5 1.5 0 0 1 .832.252l2.303 1.535A1.5 1.5 0 0 1 13 4.035ZM4 2.5a.5.5 0 0 1 .5-.5h4.697a.5.5 0 0 1 .278.084l2.302 1.535a.5.5 0 0 1 .223.416V5H4V2.5ZM4 14h8v-4H4v4Zm9-2V9H3v3h-.5a.5.5 0 0 1-.5-.5v-5a.5.5 0 0 1 .5-.5h11a.5.5 0 0 1 .5.5v5a.5.5 0 0 1-.5.5H13Z";
}