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

public class SizeArrowDown : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M1 2C1 1.44772 1.44772 1 2 1H6C6.55228 1 7 1.44772 7 2V5C7 5.55228 6.55228 6 6 6H2C1.44772 6 1 5.55228 1 5V2ZM6 2H2V5H6V2Z M1 8C1 7.44772 1.44772 7 2 7H4C4.55228 7 5 7.44772 5 8V10C5 10.5523 4.55228 11 4 11H2C1.44772 11 1 10.5523 1 10V8ZM4 8H2V10H4V8Z M11.5 1C11.7761 1 12 1.22386 12 1.5V13.2929L14.1464 11.1464C14.3417 10.9512 14.6583 10.9512 14.8536 11.1464C15.0488 11.3417 15.0488 11.6583 14.8536 11.8536L11.8536 14.8536C11.6583 15.0488 11.3417 15.0488 11.1464 14.8536L8.14645 11.8536C7.95118 11.6583 7.95118 11.3417 8.14645 11.1464C8.34171 10.9512 8.65829 10.9512 8.85355 11.1464L11 13.2929V1.5C11 1.22386 11.2239 1 11.5 1Z";

    protected override string IconGeometry20 { get; }
        = ""; 

    protected override string IconGeometry24 { get; }
        = ""; 

    protected override string IconGeometry32 { get; }
        = "";
}