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

public class At : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M2 8a6 6 0 1 1 12 0v.5c0 1.108-.886 2-1.988 2-1.122 0-1.896-.876-1.697-1.815.04-.13.071-.26.097-.39.242-.923.592-1.959 1.052-3.11a.5.5 0 1 0-.928-.37c-.164.408-.315.805-.452 1.19A2.648 2.648 0 0 0 9.098 5c-1.435-.828-3.382-.143-4.348 1.531-.966 1.674-.587 3.703.848 4.531 1.26.728 2.916.287 3.955-.965.456.85 1.423 1.403 2.46 1.403A2.99 2.99 0 0 0 15 8.5V8a7 7 0 1 0-4.303 6.461.5.5 0 1 0-.385-.922A6 6 0 0 1 2 8Zm7.449.024a18.183 18.183 0 0 0-.108.431c-.064.191-.15.384-.26.576-.766 1.326-2.147 1.648-2.983 1.165-.836-.483-1.247-1.84-.482-3.165.765-1.326 2.146-1.648 2.982-1.165.623.36 1.01 1.203.85 2.158Z";
}