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

public class TrashCross : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M6.053 1.276A.5.5 0 0 1 6.5 1h3a.5.5 0 0 1 .447.276L10.81 3H14a.5.5 0 0 1 0 1h-1.022l-.435 9.568A1.5 1.5 0 0 1 11.044 15H4.956a1.5 1.5 0 0 1-1.499-1.432L3.022 4H2a.5.5 0 0 1 0-1h3.191l.862-1.724ZM9.19 2l.5 1H6.309l.5-1h2.382Zm2.786 2H4.023l.433 9.523a.5.5 0 0 0 .5.477h6.088a.5.5 0 0 0 .5-.477L11.977 4ZM6.354 6.646a.5.5 0 1 0-.708.708L7.293 9l-1.647 1.646a.5.5 0 0 0 .708.708L8 9.707l1.646 1.647a.5.5 0 0 0 .708-.708L8.707 9l1.647-1.646a.5.5 0 0 0-.708-.708L8 8.293 6.354 6.646Z";
}