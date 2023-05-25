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

public class ArrowRightArrowLeft : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M1.146 11.146a.5.5 0 0 0 0 .708l3 3a.5.5 0 0 0 .708-.708L2.707 12H14.48a.5.5 0 0 0 0-1H2.707l2.147-2.146a.5.5 0 1 0-.708-.708l-3 3ZM1.02 4.5a.5.5 0 0 0 .5.5h11.773l-2.147 2.146a.5.5 0 0 0 .708.708l3-3a.5.5 0 0 0 0-.708l-3-3a.5.5 0 0 0-.708.708L13.293 4H1.52a.5.5 0 0 0-.5.5Z";
}