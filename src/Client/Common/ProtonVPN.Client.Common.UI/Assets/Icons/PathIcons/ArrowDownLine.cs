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

public class ArrowDownLine : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7.998 10.291 4.854 7.147a.5.5 0 1 0-.708.707l4 4a.5.5 0 0 0 .708 0l4-4a.5.5 0 0 0-.708-.707l-3.148 3.148V1.502a.5.5 0 1 0-1 0v8.79ZM2 13.501a.5.5 0 0 1 .5-.5h12a.5.5 0 1 1 0 1h-12a.5.5 0 0 1-.5-.5Z";

    protected override string IconGeometry20 { get; }
        = "m9.998 12.864-3.931-3.93a.625.625 0 0 0-.884.883l5 5c.244.244.64.244.884 0l5-5a.625.625 0 1 0-.884-.884l-3.935 3.935V1.877a.625.625 0 1 0-1.25 0v10.987ZM2.5 16.875c0-.345.28-.625.625-.625h15a.625.625 0 1 1 0 1.25h-15a.625.625 0 0 1-.625-.625Z"; 

    protected override string IconGeometry24 { get; }
        = "M11.998 15.437 7.28 10.72a.75.75 0 0 0-1.06 1.06l6 6a.75.75 0 0 0 1.06 0l6-6a.75.75 0 1 0-1.06-1.06l-4.722 4.722V2.252a.75.75 0 0 0-1.5 0v13.185ZM3 20.25a.75.75 0 0 1 .75-.75h18a.75.75 0 1 1 0 1.5h-18a.75.75 0 0 1-.75-.75Z"; 

    protected override string IconGeometry32 { get; }
        = "m15.997 20.582-6.29-6.29a1 1 0 0 0-1.414 1.415l8 8a1 1 0 0 0 1.414 0l8-8a1 1 0 0 0-1.414-1.414l-6.296 6.296V3.003a1 1 0 1 0-2 0v17.58ZM4 27a1 1 0 0 1 1-1h24a1 1 0 0 1 0 2H5a1 1 0 0 1-1-1Z";
}