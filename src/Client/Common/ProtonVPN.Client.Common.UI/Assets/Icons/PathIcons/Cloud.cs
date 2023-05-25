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

public class Cloud : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M9.888 6 9.6 5.5A3 3 0 0 0 4 7v.878l-.666.236A2.001 2.001 0 0 0 4 12h7a3 3 0 1 0 0-6H9.888Zm.577-1H11a4 4 0 0 1 0 8H4a3 3 0 0 1-1-5.83V7a4 4 0 0 1 7.465-2Z";
}