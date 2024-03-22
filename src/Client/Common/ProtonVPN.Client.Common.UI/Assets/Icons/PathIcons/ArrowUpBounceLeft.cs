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

public class ArrowUpBounceLeft : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M4.707 2h2.329a.5.5 0 1 0 0-1H3.7a.7.7 0 0 0-.7.7v3.336a.5.5 0 1 0 1 0V2.707l4.94 4.94a.5.5 0 0 1 0 .707l-5.794 5.792a.5.5 0 0 0 .708.708L9.646 9.06a1.5 1.5 0 0 0 0-2.122L4.707 2ZM12.5 1a.5.5 0 0 1 .5.5v13a.5.5 0 0 1-1 0v-13a.5.5 0 0 1 .5-.5Z";

    protected override string IconGeometry20 { get; }
        = "M5.884 2.5h2.91a.625.625 0 1 0 0-1.25H4.625a.875.875 0 0 0-.875.875v4.17a.625.625 0 1 0 1.25 0V3.383l6.174 6.174a.625.625 0 0 1 0 .884l-7.24 7.241a.625.625 0 1 0 .883.884l7.241-7.241a1.875 1.875 0 0 0 0-2.652L5.884 2.5Zm9.741-1.25c.345 0 .625.28.625.625v16.25a.625.625 0 1 1-1.25 0V1.875c0-.345.28-.625.625-.625Z"; 

    protected override string IconGeometry24 { get; }
        = "M7.06 3h3.493a.75.75 0 0 0 0-1.5H5.55c-.58 0-1.05.47-1.05 1.05v5.003a.75.75 0 0 0 1.5 0V4.061l7.409 7.409a.75.75 0 0 1 0 1.06l-8.69 8.69a.75.75 0 1 0 1.061 1.06l8.69-8.689a2.25 2.25 0 0 0 0-3.182L7.06 3Zm11.69-1.5a.75.75 0 0 1 .75.75v19.5a.75.75 0 0 1-1.5 0V2.25a.75.75 0 0 1 .75-.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M9.414 4h4.657a1 1 0 1 0 0-2H7.4A1.4 1.4 0 0 0 6 3.4v6.671a1 1 0 1 0 2 0V5.414l9.879 9.879a1 1 0 0 1 0 1.414L6.293 28.293a1 1 0 1 0 1.414 1.414l11.586-11.586a3 3 0 0 0 0-4.242L9.414 4ZM25 2a1 1 0 0 1 1 1v26a1 1 0 1 1-2 0V3a1 1 0 0 1 1-1Z";
}