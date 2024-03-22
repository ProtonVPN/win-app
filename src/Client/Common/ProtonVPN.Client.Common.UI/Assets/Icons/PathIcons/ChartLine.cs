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

public class ChartLine : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M2 2.5a.5.5 0 0 0-1 0v10A2.5 2.5 0 0 0 3.5 15h10a.5.5 0 0 0 0-1h-10A1.5 1.5 0 0 1 2 12.5v-10Z M13.854 5.854a.5.5 0 0 0-.708-.708L10 8.293 7.854 6.146a.5.5 0 0 0-.708 0l-3.5 3.5a.5.5 0 0 0 .708.708L7.5 7.207l2.146 2.147a.5.5 0 0 0 .708 0l3.5-3.5Z";

    protected override string IconGeometry20 { get; }
        = "M2.5 3.125a.625.625 0 1 0-1.25 0v12.5c0 1.726 1.4 3.125 3.125 3.125h12.5a.625.625 0 1 0 0-1.25h-12.5A1.875 1.875 0 0 1 2.5 15.625v-12.5Z M17.317 7.317a.625.625 0 1 0-.884-.884L12.5 10.366 9.817 7.683a.625.625 0 0 0-.884 0l-4.375 4.375a.625.625 0 1 0 .884.884l3.933-3.933 2.683 2.683c.244.244.64.244.884 0l4.375-4.375Z"; 

    protected override string IconGeometry24 { get; }
        = "M3 3.75a.75.75 0 0 0-1.5 0v15a3.75 3.75 0 0 0 3.75 3.75h15a.75.75 0 0 0 0-1.5h-15A2.25 2.25 0 0 1 3 18.75v-15Z M20.78 8.78a.75.75 0 0 0-1.06-1.06L15 12.44l-3.22-3.22a.75.75 0 0 0-1.06 0l-5.25 5.25a.75.75 0 1 0 1.06 1.06l4.72-4.72 3.22 3.22a.75.75 0 0 0 1.06 0l5.25-5.25Z"; 

    protected override string IconGeometry32 { get; }
        = "M4 5a1 1 0 0 0-2 0v20a5 5 0 0 0 5 5h20a1 1 0 1 0 0-2H7a3 3 0 0 1-3-3V5Z M27.707 11.707a1 1 0 0 0-1.414-1.414L20 16.586l-4.293-4.293a1 1 0 0 0-1.414 0l-7 7a1 1 0 1 0 1.414 1.414L15 14.414l4.293 4.293a1 1 0 0 0 1.414 0l7-7Z";
}