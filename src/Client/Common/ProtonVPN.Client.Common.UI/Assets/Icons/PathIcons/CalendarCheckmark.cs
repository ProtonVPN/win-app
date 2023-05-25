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

public class CalendarCheckmark : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M3 2.17V1.5a.5.5 0 0 1 1 0V2h8v-.5a.5.5 0 0 1 1 0v.67c1.165.413 2 1.524 2 2.83v7a3 3 0 0 1-3 3H4a3 3 0 0 1-3-3V5c0-1.306.835-2.417 2-2.83ZM12 3H4a2 2 0 0 0-2 2h12a2 2 0 0 0-2-2ZM2 12V6h12v6a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2Zm9.354-3.646a.5.5 0 0 0-.708-.708L7.5 10.793 5.854 9.146a.5.5 0 1 0-.708.708l1.859 1.858a.7.7 0 0 0 .99 0l3.359-3.358Z";
}