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
    protected override string IconGeometry { get; }
        = "M13.854 4.148a.51.51 0 0 1 0 .714l-6.859 6.93a.695.695 0 0 1-.99 0L3.146 8.905a.509.509 0 0 1 0-.714.496.496 0 0 1 .708 0L6.5 10.864l6.646-6.716a.496.496 0 0 1 .708 0Z";
}