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

public class TextUnderline : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M3.5 1a.5.5 0 0 0 0 1H4v6.5a4.5 4.5 0 1 0 9 0V2h.5a.5.5 0 0 0 0-1h-2a.5.5 0 0 0 0 1h.5v6.5a3.5 3.5 0 1 1-7 0V2h.5a.5.5 0 0 0 0-1h-2Z M3.5 14a.5.5 0 0 0 0 1h10a.5.5 0 0 0 0-1h-10Z";
}