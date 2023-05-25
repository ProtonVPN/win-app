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

public class Placeholder : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M5.707 2 2 5.707 1.293 5 5 1.293 5.707 2Zm3 0L2 8.707 1.293 8 8 1.293 8.707 2Zm3 0L2 11.707 1.293 11 11 1.293l.707.707Zm3 0L2 14.707 1.293 14 14 1.293l.707.707Zm0 3L5 14.707 4.293 14 14 4.293l.707.707Zm0 3L8 14.707 7.293 14 14 7.293l.707.707Zm0 3L11 14.707 10.293 14 14 10.293l.707.707Z";
}