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

public class PresentationScreen : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M1 1.5a.5.5 0 0 1 .5-.5h14a.5.5 0 0 1 0 1h-14a.5.5 0 0 1-.5-.5Zm5.584 13.277a.5.5 0 0 1 .139-.693L8 13.232V11H3.5A1.5 1.5 0 0 1 2 9.5V3h1v6.5a.5.5 0 0 0 .5.5h10a.5.5 0 0 0 .5-.5V3h1v6.5a1.5 1.5 0 0 1-1.5 1.5H9v2.232l1.277.852a.5.5 0 1 1-.554.832L8.5 14.101l-1.223.815a.5.5 0 0 1-.693-.139Z";
}