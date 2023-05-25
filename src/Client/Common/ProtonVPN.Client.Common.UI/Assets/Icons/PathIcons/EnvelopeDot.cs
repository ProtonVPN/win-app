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

public class EnvelopeDot : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M13.5 7a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5ZM4.2 3h6.137c-.148.31-.251.647-.302 1H4.2c-.577 0-.949 0-1.232.024-.272.022-.373.06-.422.085a1 1 0 0 0-.437.437c-.025.05-.063.15-.085.422a4.518 4.518 0 0 0-.006.086l5.72 3.52a.5.5 0 0 0 .524 0l2.701-1.662c.247.258.532.48.846.653l-3.023 1.86a1.5 1.5 0 0 1-1.572 0L2 6.218V10.8c0 .577 0 .949.024 1.232.022.272.06.373.085.422a1 1 0 0 0 .437.437c.05.025.15.063.422.085C3.25 13 3.623 13 4.2 13h7.6c.577 0 .949 0 1.232-.024.272-.022.373-.06.422-.085a1 1 0 0 0 .437-.437c.025-.05.063-.15.085-.422C14 11.75 14 11.377 14 10.8V7.965c.353-.051.69-.154 1-.302V10.8c0 1.12 0 1.68-.218 2.108a2 2 0 0 1-.874.874C13.48 14 12.92 14 11.8 14H4.2c-1.12 0-1.68 0-2.108-.218a2 2 0 0 1-.874-.874C1 12.48 1 11.92 1 10.8V6.2c0-1.12 0-1.68.218-2.108a2 2 0 0 1 .874-.874C2.52 3 3.08 3 4.2 3Z";
}