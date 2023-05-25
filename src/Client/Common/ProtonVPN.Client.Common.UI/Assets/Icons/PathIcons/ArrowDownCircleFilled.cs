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

public class ArrowDownCircleFilled : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M8.5 16a7.5 7.5 0 1 0 0-15 7.5 7.5 0 0 0 0 15Zm0-11.712a.5.5 0 0 1 .5.5v5.793l1.646-1.647a.5.5 0 0 1 .708.707L8.995 12a.7.7 0 0 1-.99 0L5.646 9.641a.5.5 0 1 1 .708-.707L8 10.581V4.788a.5.5 0 0 1 .5-.5Z";
}