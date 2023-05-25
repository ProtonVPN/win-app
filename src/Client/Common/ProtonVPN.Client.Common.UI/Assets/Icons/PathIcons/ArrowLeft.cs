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

public class ArrowLeft : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M8.313 2.648a.5.5 0 0 1-.002.707L3.633 8h9.864a.5.5 0 1 1 0 1H3.632l4.678 4.645a.5.5 0 0 1-.705.71L2.21 8.995a.696.696 0 0 1-.199-.389.502.502 0 0 1 0-.212.696.696 0 0 1 .199-.39l5.396-5.359a.5.5 0 0 1 .707.003Z";
}