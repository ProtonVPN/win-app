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

public class ArrowUpCircleLine : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "m9.667 5.373 1.05 1.051a.5.5 0 1 0 .708-.707L9.662 3.954a.7.7 0 0 0-.99 0L6.908 5.717a.5.5 0 0 0 .707.707l1.052-1.051v4.126a.5.5 0 0 0 1 0V5.373Z M9.167 1.5a5.333 5.333 0 1 1 0 10.667 5.333 5.333 0 0 1 0-10.667Zm0 1a4.333 4.333 0 1 1 0 8.667 4.333 4.333 0 0 1 0-8.667Z M2 1.502a.5.5 0 0 1 .5.5v10a1.5 1.5 0 0 0 1.5 1.5h10a.5.5 0 1 1 0 1H4a2.5 2.5 0 0 1-2.5-2.5v-10a.5.5 0 0 1 .5-.5Z";

    protected override string IconGeometry20 { get; }
        = "m12.083 6.716 1.314 1.314a.625.625 0 0 0 .884-.884l-2.204-2.204a.875.875 0 0 0-1.237 0L8.635 7.146a.625.625 0 1 0 .884.884l1.314-1.314v5.158a.625.625 0 0 0 1.25 0V6.716Z M11.458 1.875a6.667 6.667 0 1 1 0 13.333 6.667 6.667 0 0 1 0-13.333Zm0 1.25a5.417 5.417 0 1 1 0 10.833 5.417 5.417 0 0 1 0-10.833Z M2.5 1.877c.345 0 .625.28.625.625v12.5c0 1.036.84 1.875 1.875 1.875h12.5a.625.625 0 0 1 0 1.25H5a3.125 3.125 0 0 1-3.125-3.125v-12.5c0-.345.28-.625.625-.625Z"; 

    protected override string IconGeometry24 { get; }
        = "m14.5 8.06 1.577 1.576a.75.75 0 1 0 1.06-1.06L14.492 5.93a1.05 1.05 0 0 0-1.485 0l-2.644 2.645a.75.75 0 0 0 1.06 1.06L13 8.06v6.19a.75.75 0 1 0 1.5 0V8.06Z M13.75 2.25a8 8 0 1 1 0 16 8 8 0 0 1 0-16Zm0 1.5a6.5 6.5 0 1 1 0 13 6.5 6.5 0 0 1 0-13Z M3 2.253a.75.75 0 0 1 .75.75v15A2.25 2.25 0 0 0 6 20.253h15a.75.75 0 0 1 0 1.5H6a3.75 3.75 0 0 1-3.75-3.75v-15a.75.75 0 0 1 .75-.75Z"; 

    protected override string IconGeometry32 { get; }
        = "m19.333 10.746 2.103 2.102a1 1 0 1 0 1.414-1.414l-3.527-3.527a1.4 1.4 0 0 0-1.98 0l-3.526 3.527a1 1 0 0 0 1.414 1.414l2.102-2.102v8.252a1 1 0 0 0 2 0v-8.252Z M18.333 3C24.224 3 29 7.776 29 13.667c0 5.89-4.776 10.666-10.667 10.666-5.89 0-10.666-4.775-10.666-10.666S12.442 3 18.333 3Zm0 2a8.667 8.667 0 1 1 0 17.333 8.667 8.667 0 0 1 0-17.333Z M4 3.004a1 1 0 0 1 1 1v20a3 3 0 0 0 3 3h20a1 1 0 1 1 0 2H8a5 5 0 0 1-5-5v-20a1 1 0 0 1 1-1Z";
}