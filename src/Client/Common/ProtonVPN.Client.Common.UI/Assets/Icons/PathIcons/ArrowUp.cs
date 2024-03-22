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

public class ArrowUp : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8.5 13.998a.5.5 0 0 0 .5-.5V3.641l4.144 4.21a.5.5 0 1 0 .712-.702L8.997 2.213l-.001-.002a.697.697 0 0 0-.39-.198.502.502 0 0 0-.213 0 .697.697 0 0 0-.389.198l-.001.002-4.86 4.936a.5.5 0 1 0 .713.702L8 3.641v9.857a.5.5 0 0 0 .5.5Z";

    protected override string IconGeometry20 { get; }
        = "M10.625 17.498c.345 0 .625-.28.625-.625V4.552l5.18 5.262a.625.625 0 0 0 .89-.877l-6.073-6.17-.002-.003a.872.872 0 0 0-.487-.247.627.627 0 0 0-.266 0 .872.872 0 0 0-.487.247l-.002.002-6.073 6.17a.625.625 0 0 0 .89.878L10 4.552v12.32c0 .346.28.626.625.626Z"; 

    protected override string IconGeometry24 { get; }
        = "M12.75 20.997a.75.75 0 0 0 .75-.75V5.462l6.215 6.315a.75.75 0 1 0 1.07-1.053l-7.29-7.404-.002-.003a1.046 1.046 0 0 0-.583-.297.753.753 0 0 0-.32 0c-.214.033-.42.132-.584.297l-.002.003-7.289 7.404a.75.75 0 1 0 1.07 1.053L12 5.462v14.785c0 .415.336.75.75.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M17 27.997a1 1 0 0 0 1-1V7.283l8.287 8.419a1 1 0 1 0 1.426-1.403l-9.719-9.873-.003-.003a1.395 1.395 0 0 0-.778-.397 1.004 1.004 0 0 0-.427 0c-.285.045-.56.177-.777.397l-.004.003L6.287 14.3a1 1 0 1 0 1.426 1.403L16 7.282v19.715a1 1 0 0 0 1 1Z";
}