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

public class ArrowsToCenter : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M9.5 1.5a.5.5 0 0 1 .5.5v4.293l4.146-4.147a.5.5 0 1 1 .708.708L10.707 7H15a.5.5 0 0 1 0 1H9.5a.5.5 0 0 1-.5-.5V2a.5.5 0 0 1 .5-.5Zm-8 8A.5.5 0 0 1 2 9h5.5a.5.5 0 0 1 .5.5V15a.5.5 0 0 1-1 0v-4.293l-4.146 4.147a.5.5 0 0 1-.708-.708L6.293 10H2a.5.5 0 0 1-.5-.5Z";

    protected override string IconGeometry20 { get; }
        = "M11.875 1.875c.345 0 .625.28.625.625v5.366l5.183-5.183a.625.625 0 0 1 .884.884L13.384 8.75h5.366a.625.625 0 1 1 0 1.25h-6.875a.625.625 0 0 1-.625-.625V2.5c0-.345.28-.625.625-.625Zm-10 10c0-.345.28-.625.625-.625h6.875c.345 0 .625.28.625.625v6.875a.625.625 0 1 1-1.25 0v-5.366l-5.183 5.183a.625.625 0 1 1-.884-.884L7.866 12.5H2.5a.625.625 0 0 1-.625-.625Z"; 

    protected override string IconGeometry24 { get; }
        = "M14.25 2.25A.75.75 0 0 1 15 3v6.44l6.22-6.22a.75.75 0 0 1 1.06 1.06l-6.22 6.22h6.44a.75.75 0 0 1 0 1.5h-8.25a.75.75 0 0 1-.75-.75V3a.75.75 0 0 1 .75-.75Zm-12 12A.75.75 0 0 1 3 13.5h8.25a.75.75 0 0 1 .75.75v8.25a.75.75 0 0 1-1.5 0v-6.44l-6.22 6.22a.75.75 0 1 1-1.06-1.06L9.44 15H3a.75.75 0 0 1-.75-.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M19 3a1 1 0 0 1 1 1v8.586l8.293-8.293a1 1 0 1 1 1.414 1.414L21.414 14H30a1 1 0 1 1 0 2H19a1 1 0 0 1-1-1V4a1 1 0 0 1 1-1ZM3 19a1 1 0 0 1 1-1h11a1 1 0 0 1 1 1v11a1 1 0 1 1-2 0v-8.586l-8.293 8.293a1 1 0 0 1-1.414-1.414L12.586 20H4a1 1 0 0 1-1-1Z";
}