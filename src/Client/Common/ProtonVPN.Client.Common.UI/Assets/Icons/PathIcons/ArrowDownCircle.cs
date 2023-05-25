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

public class ArrowDownCircle : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M8 10.58 6.354 8.935a.5.5 0 1 0-.708.707L8.005 12a.7.7 0 0 0 .99 0l2.359-2.359a.5.5 0 0 0-.708-.707L9 10.581V4.5a.5.5 0 0 0-1 0v6.08Z M8.5 15a6.5 6.5 0 1 0 0-13 6.5 6.5 0 0 0 0 13Zm0 1a7.5 7.5 0 1 0 0-15 7.5 7.5 0 0 0 0 15Z";
}