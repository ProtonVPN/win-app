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

public class ThreeDotsVertical : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8 12a1 1 0 1 1 0 2 1 1 0 0 1 0-2Zm0-5a1 1 0 1 1 0 2 1 1 0 0 1 0-2Zm0-5a1 1 0 1 1 0 2 1 1 0 0 1 0-2Z";

    protected override string IconGeometry20 { get; }
        = "M10 15a1.25 1.25 0 1 1 0 2.5 1.25 1.25 0 0 1 0-2.5Zm0-6.25a1.25 1.25 0 1 1 0 2.5 1.25 1.25 0 0 1 0-2.5Zm0-6.25A1.25 1.25 0 1 1 10 5a1.25 1.25 0 0 1 0-2.5Z"; 

    protected override string IconGeometry24 { get; }
        = "M12 18a1.5 1.5 0 1 1 0 3 1.5 1.5 0 0 1 0-3Zm0-7.5a1.5 1.5 0 1 1 0 3 1.5 1.5 0 0 1 0-3ZM12 3a1.5 1.5 0 1 1 0 3 1.5 1.5 0 0 1 0-3Z"; 

    protected override string IconGeometry32 { get; }
        = "M16 24a2 2 0 1 1 0 4 2 2 0 0 1 0-4Zm0-10a2 2 0 1 1 0 4 2 2 0 0 1 0-4Zm0-10a2 2 0 1 1 0 4 2 2 0 0 1 0-4Z";
}