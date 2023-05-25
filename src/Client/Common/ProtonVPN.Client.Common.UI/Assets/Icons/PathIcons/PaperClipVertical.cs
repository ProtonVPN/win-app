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

public class PaperClipVertical : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M6.5 2A2.5 2.5 0 0 0 4 4.5V11a4 4 0 0 0 8 0V5a.5.5 0 0 1 1 0v6a5 5 0 0 1-10 0V4.5a3.5 3.5 0 1 1 7 0V11a2 2 0 1 1-4 0V5a.5.5 0 0 1 1 0v6a1 1 0 1 0 2 0V4.5A2.5 2.5 0 0 0 6.5 2Z";
}