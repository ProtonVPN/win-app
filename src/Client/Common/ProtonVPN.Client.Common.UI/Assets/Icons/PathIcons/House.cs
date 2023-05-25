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

public class House : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M7.367 1.373a1 1 0 0 1 1.266 0l5 4.09a1 1 0 0 1 .367.774v6.764a1 1 0 0 1-1 1H9v-4H7v4H3a1 1 0 0 1-1-1V6.237a1 1 0 0 1 .367-.774l5-4.09ZM8 2.146 3 6.237v6.764h3v-3a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v3h3V6.237l-5-4.09Z";
}