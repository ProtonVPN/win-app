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

public class BrandProton : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M3 1h6.5a4.5 4.5 0 1 1 0 9h-3a.5.5 0 0 0-.5.5V15H3v-5a3 3 0 0 1 3-3h3.5a1.5 1.5 0 0 0 0-3H6v2H3V1Zm1 1v3h1V3h4.5a2.5 2.5 0 0 1 0 5H6a2 2 0 0 0-2 2v4h1v-3.5A1.5 1.5 0 0 1 6.5 9h3a3.5 3.5 0 1 0 0-7H4Z";
}