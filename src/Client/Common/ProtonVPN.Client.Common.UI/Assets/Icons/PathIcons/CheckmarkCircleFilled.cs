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

public class CheckmarkCircleFilled : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M8 15A7 7 0 1 0 8 1a7 7 0 0 0 0 14Zm3.854-8.146a.5.5 0 0 0-.708-.708L7.5 9.793 5.354 7.646a.5.5 0 1 0-.708.708l2.359 2.358a.7.7 0 0 0 .99 0l3.859-3.858Z";
}