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

public class ArrowDownToSquare : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8.995 9.854a.7.7 0 0 1-.99 0L5.646 7.495a.5.5 0 1 1 .708-.707L8 8.434V.5a.5.5 0 0 1 1 0v7.934l1.646-1.646a.5.5 0 0 1 .708.707L8.995 9.854Z M3 5a1 1 0 0 1 1-1h2.067V3H4a2 2 0 0 0-2 2v8a2 2 0 0 0 2 2h9a2 2 0 0 0 2-2V5a2 2 0 0 0-2-2h-2v1h2a1 1 0 0 1 1 1v8a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V5Z";

    protected override string IconGeometry20 { get; }
        = "M10.619 12.317a.875.875 0 0 1-1.238 0L6.433 9.369a.625.625 0 0 1 .884-.884l2.058 2.058V1.875a.625.625 0 1 1 1.25 0v8.668l2.058-2.058a.625.625 0 1 1 .884.884l-2.948 2.948Z M3.75 7.5c0-.69.56-1.25 1.25-1.25h2.583V5H5a2.5 2.5 0 0 0-2.5 2.5v8.75a2.5 2.5 0 0 0 2.5 2.5h10a2.5 2.5 0 0 0 2.5-2.5V7.5A2.5 2.5 0 0 0 15 5h-2.5v1.25H15c.69 0 1.25.56 1.25 1.25v8.75c0 .69-.56 1.25-1.25 1.25H5c-.69 0-1.25-.56-1.25-1.25V7.5Z"; 

    protected override string IconGeometry24 { get; }
        = "M12.742 14.78a1.05 1.05 0 0 1-1.485 0L7.72 11.243a.75.75 0 0 1 1.06-1.061l2.47 2.47V2.25a.75.75 0 0 1 1.5 0v10.402l2.47-2.47a.75.75 0 1 1 1.06 1.06l-3.538 3.538Z M4.5 9A1.5 1.5 0 0 1 6 7.5h3.1V6H6a3 3 0 0 0-3 3v10.5a3 3 0 0 0 3 3h12a3 3 0 0 0 3-3V9a3 3 0 0 0-3-3h-3v1.5h3A1.5 1.5 0 0 1 19.5 9v10.5A1.5 1.5 0 0 1 18 21H6a1.5 1.5 0 0 1-1.5-1.5V9Z"; 

    protected override string IconGeometry32 { get; }
        = "M16.99 19.707a1.4 1.4 0 0 1-1.98 0l-4.717-4.717a1 1 0 1 1 1.414-1.414L15 16.869V3a1 1 0 1 1 2 0v13.869l3.293-3.293a1 1 0 0 1 1.414 1.414l-4.717 4.717Z M6 12a2 2 0 0 1 2-2h4.133V8H8a4 4 0 0 0-4 4v14a4 4 0 0 0 4 4h16a4 4 0 0 0 4-4V12a4 4 0 0 0-4-4h-4v2h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2V12Z";
}