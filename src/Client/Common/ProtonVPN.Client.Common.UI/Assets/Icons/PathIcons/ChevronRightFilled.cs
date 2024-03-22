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

public class ChevronRightFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M9.846 7.588 6.72 4.138c-.266-.294-.72-.086-.72.329v7.066c0 .415.454.623.72.33l3.126-3.451a.626.626 0 0 0 0-.824Z";

    protected override string IconGeometry20 { get; }
        = "M12.807 9.485 8.9 5.172c-.332-.367-.9-.107-.9.411v8.834c0 .518.568.778.9.411l3.907-4.313a.782.782 0 0 0 0-1.03Z"; 

    protected override string IconGeometry24 { get; }
        = "M15.768 11.434 11.08 6.689c-.398-.404-1.08-.118-1.08.453v9.716c0 .57.682.857 1.08.453l4.688-4.745a.807.807 0 0 0 0-1.132Z"; 

    protected override string IconGeometry32 { get; }
        = "";
}