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

public class Bolt : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M5.797 15.957a.5.5 0 0 1-.286-.562L6.881 9H3.5a.5.5 0 0 1-.4-.8l6-8a.5.5 0 0 1 .893.382L9.09 6h3.41a.5.5 0 0 1 .405.793l-6.5 9a.5.5 0 0 1-.608.164ZM11.522 7H8.5a.5.5 0 0 1-.493-.582L8.67 2.44 4.5 8h3a.5.5 0 0 1 .489.605l-1.002 4.674L11.522 7Z";

    protected override string IconGeometry20 { get; }
        = ""; 

    protected override string IconGeometry24 { get; }
        = ""; 

    protected override string IconGeometry32 { get; }
        = "";
}