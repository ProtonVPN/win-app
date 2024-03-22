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

public class LinesVertical : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M6 2.5v11a.5.5 0 0 0 1 0v-11a.5.5 0 0 0-1 0Zm3 0v11a.5.5 0 0 0 1 0v-11a.5.5 0 0 0-1 0Z";

    protected override string IconGeometry20 { get; }
        = "M7.5 3.125v13.75a.625.625 0 1 0 1.25 0V3.125a.625.625 0 1 0-1.25 0Zm3.75 0v13.75a.625.625 0 1 0 1.25 0V3.125a.625.625 0 1 0-1.25 0Z"; 

    protected override string IconGeometry24 { get; }
        = "M9 3.75v16.5a.75.75 0 0 0 1.5 0V3.75a.75.75 0 0 0-1.5 0Zm4.5 0v16.5a.75.75 0 0 0 1.5 0V3.75a.75.75 0 0 0-1.5 0Z"; 

    protected override string IconGeometry32 { get; }
        = "M12 5v22a1 1 0 1 0 2 0V5a1 1 0 1 0-2 0Zm6 0v22a1 1 0 1 0 2 0V5a1 1 0 1 0-2 0Z";
}