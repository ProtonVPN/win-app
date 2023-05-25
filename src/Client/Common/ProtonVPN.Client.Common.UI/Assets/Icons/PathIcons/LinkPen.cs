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

public class LinkPen : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M10.793 1.726a1 1 0 0 1 1.424-.01l.795.795a1 1 0 0 1 0 1.415L8.805 8.125a1 1 0 0 1-.591.285l-1.442.168a.5.5 0 0 1-.555-.552l.16-1.458a1 1 0 0 1 .277-.588l4.138-4.254Zm.717.698.795.794-4.206 4.199-.819.095.092-.835 4.138-4.253Z M15 13a3 3 0 0 0-3-3H9.5a.5.5 0 1 0 0 1H12a2 2 0 1 1 0 4H9.5a.5.5 0 1 0 0 1H12a3 3 0 0 0 3-3Z M10.5 13a.5.5 0 0 1-.5.5H6a.5.5 0 0 1 0-1h4a.5.5 0 0 1 .5.5Z M1 13a3 3 0 0 0 3 3h2.5a.5.5 0 0 0 0-1H4a2 2 0 0 1 0-4h2.5a.5.5 0 0 0 0-1H4a3 3 0 0 0-3 3Z";
}