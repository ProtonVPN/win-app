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

public class CrossCircleFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8 15A7 7 0 1 0 8 1a7 7 0 0 0 0 14Zm2.854-4.146a.5.5 0 0 1-.708 0L8 8.707l-2.146 2.147a.5.5 0 0 1-.708-.708L7.293 8 5.146 5.854a.5.5 0 1 1 .708-.708L8 7.293l2.146-2.147a.5.5 0 0 1 .708.708L8.707 8l2.147 2.146a.5.5 0 0 1 0 .708Z";

    protected override string IconGeometry20 { get; }
        = "M10 18.75a8.75 8.75 0 1 0 0-17.5 8.75 8.75 0 0 0 0 17.5Zm3.567-5.183a.625.625 0 0 1-.884 0L10 10.884l-2.683 2.683a.625.625 0 1 1-.884-.884L9.116 10 6.433 7.317a.625.625 0 1 1 .884-.884L10 9.116l2.683-2.683a.625.625 0 1 1 .884.884L10.884 10l2.683 2.683a.625.625 0 0 1 0 .884Z"; 

    protected override string IconGeometry24 { get; }
        = "M12 22.5c5.799 0 10.5-4.701 10.5-10.5S17.799 1.5 12 1.5 1.5 6.201 1.5 12 6.201 22.5 12 22.5Zm4.28-6.22a.75.75 0 0 1-1.06 0L12 13.06l-3.22 3.22a.75.75 0 0 1-1.06-1.06L10.94 12 7.72 8.78a.75.75 0 0 1 1.06-1.06L12 10.94l3.22-3.22a.75.75 0 1 1 1.06 1.06L13.06 12l3.22 3.22a.75.75 0 0 1 0 1.06Z"; 

    protected override string IconGeometry32 { get; }
        = "M16 30c7.732 0 14-6.268 14-14S23.732 2 16 2 2 8.268 2 16s6.268 14 14 14Zm5.707-8.293a1 1 0 0 1-1.414 0L16 17.414l-4.293 4.293a1 1 0 0 1-1.414-1.414L14.586 16l-4.293-4.293a1 1 0 1 1 1.414-1.414L16 14.586l4.293-4.293a1 1 0 1 1 1.414 1.414L17.414 16l4.293 4.293a1 1 0 0 1 0 1.414Z";
}