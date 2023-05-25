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

public class PenSquare : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "m13.183 2 .818.818-6.588 6.588a1 1 0 0 1-.518.275l-.712.137.137-.712a1 1 0 0 1 .275-.518L13.183 2Zm1.525.111-.818-.818a1 1 0 0 0-1.414 0L5.888 7.88a2 2 0 0 0-.55 1.036l-.276 1.437a.5.5 0 0 0 .585.586l1.437-.277a2 2 0 0 0 1.036-.55l6.588-6.587a1 1 0 0 0 0-1.415ZM2.001 4.5A1.5 1.5 0 0 1 3.5 3h5.893V2H3.501A2.5 2.5 0 0 0 1 4.5v7.999a2.5 2.5 0 0 0 2.5 2.5h7.889a2.5 2.5 0 0 0 2.5-2.5V6.605h-1V12.5a1.5 1.5 0 0 1-1.5 1.5H3.5A1.5 1.5 0 0 1 2 12.5v-8Z";
}