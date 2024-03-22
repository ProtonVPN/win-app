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

public class Checkmark : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M13.854 4.148a.51.51 0 0 1 0 .714l-6.859 6.93a.695.695 0 0 1-.99 0L3.146 8.905a.509.509 0 0 1 0-.714.496.496 0 0 1 .708 0L6.5 10.864l6.646-6.716a.496.496 0 0 1 .708 0Z";

    protected override string IconGeometry20 { get; }
        = "M17.317 5.185a.636.636 0 0 1 0 .893l-8.573 8.663a.869.869 0 0 1-1.238 0l-3.573-3.61a.636.636 0 0 1 0-.894.62.62 0 0 1 .884 0l3.308 3.343 8.308-8.395a.62.62 0 0 1 .884 0Z"; 

    protected override string IconGeometry24 { get; }
        = "M20.78 6.222a.763.763 0 0 1 0 1.072L10.492 17.689a1.042 1.042 0 0 1-1.484 0L4.72 13.357a.763.763 0 0 1 0-1.072.744.744 0 0 1 1.06 0l3.97 4.011 9.97-10.074a.744.744 0 0 1 1.06 0Z"; 

    protected override string IconGeometry32 { get; }
        = "M27.707 8.296c.39.395.39 1.034 0 1.429L13.99 23.585a1.39 1.39 0 0 1-1.98 0L6.293 17.81a1.018 1.018 0 0 1 0-1.43.993.993 0 0 1 1.414 0L13 21.729 26.293 8.296a.993.993 0 0 1 1.414 0Z";
}