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

public class Hook : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M12 4.915a1.5 1.5 0 1 0-1 0V10a3 3 0 0 1-5.992.215l.992.992.707-.707L4 7.793V10a4 4 0 0 0 8 0V4.915ZM11 3.5a.5.5 0 1 1 1 0 .5.5 0 0 1-1 0Z";

    protected override string IconGeometry20 { get; }
        = "M14.375 4.375a.625.625 0 1 1-1.25 0 .625.625 0 0 1 1.25 0Zm0 1.768a1.876 1.876 0 1 0-1.25 0v6.982a3.125 3.125 0 0 1-6.238.27l.796.797.884-.884-1.875-1.875-1.067-1.067v2.759a4.375 4.375 0 0 0 8.75 0V6.143Z"; 

    protected override string IconGeometry24 { get; }
        = "M17.25 5.25a.75.75 0 1 1-1.5 0 .75.75 0 0 1 1.5 0Zm0 2.122a2.25 2.25 0 1 0-1.5 0v8.378a3.75 3.75 0 0 1-7.486.325l.956.955 1.06-1.06-2.25-2.25-1.28-1.28v3.31a5.25 5.25 0 1 0 10.5 0V7.372Z"; 

    protected override string IconGeometry32 { get; }
        = "M23 7a1 1 0 1 1-2 0 1 1 0 0 1 2 0Zm0 2.83a3.001 3.001 0 1 0-2 0V21a5 5 0 0 1-9.982.433l1.275 1.274 1.414-1.414-3-3L9 16.586V21a7 7 0 1 0 14 0V9.83Z";
}