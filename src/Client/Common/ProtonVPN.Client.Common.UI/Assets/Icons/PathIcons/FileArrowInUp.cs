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

public class FileArrowInUp : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M4.5 2A1.5 1.5 0 0 0 3 3.5v10A1.5 1.5 0 0 0 4.5 15H6v1H4.5A2.5 2.5 0 0 1 2 13.5v-10A2.5 2.5 0 0 1 4.5 1h5.672a2.5 2.5 0 0 1 1.767.732l2.329 2.329A2.5 2.5 0 0 1 15 5.828V13.5a2.5 2.5 0 0 1-2.5 2.5H11v-1h1.5a1.5 1.5 0 0 0 1.5-1.5V6h-2c-.828 0-2-.672-2-1.5V2H4.5Zm6.5.25V4.5a.5.5 0 0 0 .5.5h2.25a1.503 1.503 0 0 0-.19-.232l-2.328-2.329A1.497 1.497 0 0 0 11 2.25ZM8.005 7.288a.7.7 0 0 1 .99 0l1.859 1.858a.5.5 0 0 1-.708.708L9 8.707V15.5a.5.5 0 0 1-1 0V8.707L6.854 9.854a.5.5 0 1 1-.708-.708l1.859-1.858Z";

    protected override string IconGeometry20 { get; }
        = "M5.625 2.5c-1.036 0-1.875.84-1.875 1.875v11.25c0 1.035.84 1.875 1.875 1.875H7.5v1.25H5.625A3.125 3.125 0 0 1 2.5 15.625V4.375c0-1.726 1.4-3.125 3.125-3.125h5.84c.828 0 1.623.33 2.21.915l2.91 2.91c.586.587.915 1.382.915 2.21v8.34c0 1.726-1.4 3.125-3.125 3.125H12.5V17.5h1.875c1.036 0 1.875-.84 1.875-1.875V7.5h-3.125a1.875 1.875 0 0 1-1.875-1.875V2.5H5.625Zm6.875.312v2.813c0 .345.28.625.625.625h2.813a1.873 1.873 0 0 0-.237-.29l-2.91-2.91a1.876 1.876 0 0 0-.291-.238ZM9.381 9.11a.875.875 0 0 1 1.238 0l2.323 2.323a.625.625 0 0 1-.884.884l-1.433-1.433v7.241a.625.625 0 1 1-1.25 0v-7.241l-1.433 1.433a.625.625 0 1 1-.884-.884L9.381 9.11Z"; 

    protected override string IconGeometry24 { get; }
        = "M6.75 3A2.25 2.25 0 0 0 4.5 5.25v13.5A2.25 2.25 0 0 0 6.75 21H9v1.5H6.75A3.75 3.75 0 0 1 3 18.75V5.25A3.75 3.75 0 0 1 6.75 1.5h7.007a3.75 3.75 0 0 1 2.652 1.098l3.493 3.493A3.75 3.75 0 0 1 21 8.743V18.75a3.75 3.75 0 0 1-3.75 3.75H15V21h2.25a2.25 2.25 0 0 0 2.25-2.25V9h-3.75a2.25 2.25 0 0 1-2.25-2.25V3H6.75Zm8.25.374V6.75c0 .414.336.75.75.75h3.376a2.249 2.249 0 0 0-.285-.348l-3.493-3.493A2.25 2.25 0 0 0 15 3.374Zm-3.742 7.558a1.05 1.05 0 0 1 1.485 0l2.787 2.788a.75.75 0 0 1-1.06 1.06l-1.72-1.72v8.69a.75.75 0 0 1-1.5 0v-8.69l-1.72 1.72a.75.75 0 1 1-1.06-1.06l2.787-2.788Z"; 

    protected override string IconGeometry32 { get; }
        = "M9 4a3 3 0 0 0-3 3v18a3 3 0 0 0 3 3h3v2H9a5 5 0 0 1-5-5V7a5 5 0 0 1 5-5h9.343a5 5 0 0 1 3.536 1.464l4.656 4.657A5 5 0 0 1 28 11.657V25a5 5 0 0 1-5 5h-3v-2h3a3 3 0 0 0 3-3V12h-5a3 3 0 0 1-3-3V4H9Zm11 .499V9a1 1 0 0 0 1 1h4.501a3 3 0 0 0-.38-.464l-4.656-4.657a3.004 3.004 0 0 0-.465-.38Zm-4.99 10.077a1.4 1.4 0 0 1 1.98 0l3.717 3.717a1 1 0 0 1-1.414 1.414L17 17.414V29a1 1 0 0 1-2 0V17.414l-2.293 2.293a1 1 0 0 1-1.414-1.414l3.717-3.717Z";
}