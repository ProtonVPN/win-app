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

public class UserArrowRight : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M8 7a2 2 0 1 0 0-4 2 2 0 0 0 0 4Zm0 1a3 3 0 1 0 0-6 3 3 0 0 0 0 6Zm-2.497 2a1.5 1.5 0 0 0-1.2.6l-1.2 1.6a.5.5 0 0 0 .4.8H9v1H3.503c-1.236 0-1.942-1.411-1.2-2.4l1.2-1.6a2.5 2.5 0 0 1 2-1H9v1H5.503Zm6.694 3.85a.5.5 0 0 0 .708.006L14.79 12a.7.7 0 0 0 0-.998l-1.886-1.857a.5.5 0 0 0-.702.712L13.364 11H9.508a.5.5 0 0 0 0 1h3.856l-1.161 1.144a.5.5 0 0 0-.006.707Z";
}