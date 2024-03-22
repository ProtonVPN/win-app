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

public class Tv : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M2.5 2A1.5 1.5 0 0 0 1 3.5v7A1.5 1.5 0 0 0 2.5 12h5v2H5a.5.5 0 0 0 0 1h6a.5.5 0 0 0 0-1H8.5v-2h5a1.5 1.5 0 0 0 1.5-1.5v-7A1.5 1.5 0 0 0 13.5 2h-11ZM8 11h5.5a.5.5 0 0 0 .5-.5v-7a.5.5 0 0 0-.5-.5h-11a.5.5 0 0 0-.5.5v7a.5.5 0 0 0 .5.5H8Z";

    protected override string IconGeometry20 { get; }
        = "M3.125 2.5c-1.036 0-1.875.84-1.875 1.875v8.75c0 1.036.84 1.875 1.875 1.875h6.25v2.5H6.25a.625.625 0 1 0 0 1.25h7.5a.625.625 0 1 0 0-1.25h-3.125V15h6.25c1.035 0 1.875-.84 1.875-1.875v-8.75c0-1.036-.84-1.875-1.875-1.875H3.125ZM10 13.75h6.875c.345 0 .625-.28.625-.625v-8.75a.625.625 0 0 0-.625-.625H3.125a.625.625 0 0 0-.625.625v8.75c0 .345.28.625.625.625H10Z"; 

    protected override string IconGeometry24 { get; }
        = "M3.75 3A2.25 2.25 0 0 0 1.5 5.25v10.5A2.25 2.25 0 0 0 3.75 18h7.5v3H7.5a.75.75 0 0 0 0 1.5h9a.75.75 0 0 0 0-1.5h-3.75v-3h7.5a2.25 2.25 0 0 0 2.25-2.25V5.25A2.25 2.25 0 0 0 20.25 3H3.75ZM12 16.5h8.25a.75.75 0 0 0 .75-.75V5.25a.75.75 0 0 0-.75-.75H3.75a.75.75 0 0 0-.75.75v10.5c0 .414.336.75.75.75H12Z"; 

    protected override string IconGeometry32 { get; }
        = "M5 4a3 3 0 0 0-3 3v14a3 3 0 0 0 3 3h10v4h-5a1 1 0 1 0 0 2h12a1 1 0 1 0 0-2h-5v-4h10a3 3 0 0 0 3-3V7a3 3 0 0 0-3-3H5Zm11 18h11a1 1 0 0 0 1-1V7a1 1 0 0 0-1-1H5a1 1 0 0 0-1 1v14a1 1 0 0 0 1 1h11Z";
}