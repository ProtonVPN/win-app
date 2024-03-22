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

public class ChevronLeftFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "m6.154 7.588 3.126-3.45c.266-.294.72-.086.72.329v7.066c0 .415-.454.623-.72.33L6.154 8.412a.626.626 0 0 1 0-.824Z";

    protected override string IconGeometry20 { get; }
        = "M7.193 9.485 11.1 5.172c.332-.367.9-.107.9.411v8.834c0 .518-.568.778-.9.411l-3.907-4.313a.782.782 0 0 1 0-1.03Z"; 

    protected override string IconGeometry24 { get; }
        = "m8.232 11.434 4.688-4.745c.398-.404 1.08-.118 1.08.453v9.716c0 .57-.682.857-1.08.453l-4.688-4.745a.808.808 0 0 1 0-1.132Z"; 

    protected override string IconGeometry32 { get; }
        = "";
}