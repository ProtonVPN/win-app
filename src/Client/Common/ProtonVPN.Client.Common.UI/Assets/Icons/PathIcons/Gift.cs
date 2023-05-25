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

public class Gift : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M7 3.5A1.5 1.5 0 1 0 5.5 5H7V3.5Zm-4 0c0 .563.186 1.082.5 1.5H1a1 1 0 0 0-1 1v2a1 1 0 0 0 1 1v5a2 2 0 0 0 2 2h9a2 2 0 0 0 2-2V9a1 1 0 0 0 1-1V6a1 1 0 0 0-1-1h-2.5a2.5 2.5 0 0 0-4-3A2.5 2.5 0 0 0 3 3.5ZM1 6h5.99v2H1V6Zm5.99 3H2v5a1 1 0 0 0 1 1h3.99V9Zm1 6V9H13v5a1 1 0 0 1-1 1H7.99Zm0-7V6H14v2H7.99ZM11 3.5A1.5 1.5 0 0 1 9.5 5H8V3.5a1.5 1.5 0 1 1 3 0Z";
}