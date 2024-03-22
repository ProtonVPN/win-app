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

public class TagFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M10.132 15.082a1 1 0 0 1-1.414 0L2.293 8.657A1 1 0 0 1 2 7.95V3a1 1 0 0 1 1-1h4.95a1 1 0 0 1 .707.293l6.425 6.425a1 1 0 0 1 0 1.414l-4.95 4.95ZM5.5 6.5a1 1 0 1 0 0-2 1 1 0 0 0 0 2Z";

    protected override string IconGeometry20 { get; }
        = "M12.665 18.852a1.25 1.25 0 0 1-1.768 0l-8.03-8.03a1.25 1.25 0 0 1-.367-.884V3.75c0-.69.56-1.25 1.25-1.25h6.187c.332 0 .65.132.884.367l8.031 8.03a1.25 1.25 0 0 1 0 1.768l-6.187 6.187ZM6.875 8.125a1.25 1.25 0 1 0 0-2.5 1.25 1.25 0 0 0 0 2.5Z"; 

    protected override string IconGeometry24 { get; }
        = "M15.198 22.623a1.5 1.5 0 0 1-2.122 0L3.44 12.986a1.5 1.5 0 0 1-.44-1.06V4.5A1.5 1.5 0 0 1 4.5 3h7.425a1.5 1.5 0 0 1 1.06.44l9.637 9.637a1.5 1.5 0 0 1 0 2.121l-7.424 7.425ZM8.25 9.75a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3Z"; 

    protected override string IconGeometry32 { get; }
        = "M20.264 30.164a2 2 0 0 1-2.829 0L4.586 17.314A2 2 0 0 1 4 15.9V6.002a2 2 0 0 1 2-2h9.9a2 2 0 0 1 1.414.586l12.85 12.849a2 2 0 0 1 0 2.828l-9.9 9.9ZM11 13a2 2 0 1 0 0-4 2 2 0 0 0 0 4Z";
}