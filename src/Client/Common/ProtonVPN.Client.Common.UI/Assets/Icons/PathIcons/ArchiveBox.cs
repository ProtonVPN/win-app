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

public class ArchiveBox : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M5.5 8.5A.5.5 0 0 1 6 8h4a.5.5 0 0 1 0 1H6a.5.5 0 0 1-.5-.5Z M2 6a1 1 0 0 1-1-1V3a1 1 0 0 1 1-1h12a1 1 0 0 1 1 1v2a1 1 0 0 1-1 1v7a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V6Zm0-3h12v2H2V3Zm1 3v7a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1V6H3Z";
}