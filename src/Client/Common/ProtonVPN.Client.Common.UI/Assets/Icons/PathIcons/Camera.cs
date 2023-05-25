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

public class Camera : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M4.5 4h.618l.276-.553L6.118 2h3.764l.724 1.447.276.553H13a1 1 0 0 1 1 1v7a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V5a1 1 0 0 1 1-1h1.5ZM3 3h1.5l.724-1.447A1 1 0 0 1 6.118 1h3.764a1 1 0 0 1 .894.553L11.5 3H13a2 2 0 0 1 2 2v7a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2Zm7.5 5a2.5 2.5 0 1 1-5 0 2.5 2.5 0 0 1 5 0Zm1 0a3.5 3.5 0 1 1-7 0 3.5 3.5 0 0 1 7 0Z";
}