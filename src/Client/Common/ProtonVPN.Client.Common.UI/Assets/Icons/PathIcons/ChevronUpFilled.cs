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

public class ChevronUpFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7.588 6.154 4.138 9.28c-.294.266-.086.72.329.72h7.066c.415 0 .623-.454.33-.72L8.412 6.154a.626.626 0 0 0-.824 0Z";

    protected override string IconGeometry20 { get; }
        = "M9.485 7.693 5.172 11.6c-.367.332-.107.9.411.9h8.834c.518 0 .778-.568.411-.9l-4.313-3.907a.782.782 0 0 0-1.03 0Z"; 

    protected override string IconGeometry24 { get; }
        = "M11.434 9.232 6.689 13.92c-.404.398-.118 1.08.453 1.08h9.716c.57 0 .857-.682.453-1.08l-4.745-4.688a.808.808 0 0 0-1.132 0Z"; 

    protected override string IconGeometry32 { get; }
        = "";
}