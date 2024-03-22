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

public class FileLines : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M13 6h-2.5A1.5 1.5 0 0 1 9 4.5V2H4a1 1 0 0 0-1 1v10a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1V6Zm-3-3.586L12.586 5H10.5a.5.5 0 0 1-.5-.5V2.414ZM4 1a2 2 0 0 0-2 2v10a2 2 0 0 0 2 2h8a2 2 0 0 0 2-2V5.828a2 2 0 0 0-.586-1.414l-2.828-2.828A2 2 0 0 0 9.172 1H4Zm1 6a.5.5 0 0 0 0 1h5.5a.5.5 0 0 0 0-1H5Zm-.5 2.5A.5.5 0 0 1 5 9h3.5a.5.5 0 0 1 0 1H5a.5.5 0 0 1-.5-.5ZM5 11a.5.5 0 0 0 0 1h5.5a.5.5 0 0 0 0-1H5Z";

    protected override string IconGeometry20 { get; }
        = "M16.25 7.5h-3.125a1.875 1.875 0 0 1-1.875-1.875V2.5H5c-.69 0-1.25.56-1.25 1.25v12.5c0 .69.56 1.25 1.25 1.25h10c.69 0 1.25-.56 1.25-1.25V7.5ZM12.5 3.018l3.232 3.232h-2.607a.625.625 0 0 1-.625-.625V3.018ZM5 1.25a2.5 2.5 0 0 0-2.5 2.5v12.5a2.5 2.5 0 0 0 2.5 2.5h10a2.5 2.5 0 0 0 2.5-2.5V7.286a2.5 2.5 0 0 0-.732-1.768l-3.536-3.536a2.5 2.5 0 0 0-1.768-.732H5Zm1.25 7.5a.625.625 0 1 0 0 1.25h6.875a.625.625 0 1 0 0-1.25H6.25Zm-.625 3.125c0-.345.28-.625.625-.625h4.375a.625.625 0 1 1 0 1.25H6.25a.625.625 0 0 1-.625-.625Zm.625 1.875a.625.625 0 1 0 0 1.25h6.875a.625.625 0 1 0 0-1.25H6.25Z"; 

    protected override string IconGeometry24 { get; }
        = "M19.5 9h-3.75a2.25 2.25 0 0 1-2.25-2.25V3H6a1.5 1.5 0 0 0-1.5 1.5v15A1.5 1.5 0 0 0 6 21h12a1.5 1.5 0 0 0 1.5-1.5V9ZM15 3.621 18.879 7.5H15.75a.75.75 0 0 1-.75-.75V3.621ZM6 1.5a3 3 0 0 0-3 3v15a3 3 0 0 0 3 3h12a3 3 0 0 0 3-3V8.743a3 3 0 0 0-.879-2.122L15.88 2.38a3 3 0 0 0-2.122-.879H6Zm1.5 9a.75.75 0 0 0 0 1.5h8.25a.75.75 0 0 0 0-1.5H7.5Zm-.75 3.75a.75.75 0 0 1 .75-.75h5.25a.75.75 0 0 1 0 1.5H7.5a.75.75 0 0 1-.75-.75Zm.75 2.25a.75.75 0 0 0 0 1.5h8.25a.75.75 0 0 0 0-1.5H7.5Z"; 

    protected override string IconGeometry32 { get; }
        = "M26 12h-5a3 3 0 0 1-3-3V4H8a2 2 0 0 0-2 2v20a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V12Zm-6-7.172L25.172 10H21a1 1 0 0 1-1-1V4.828ZM8 2a4 4 0 0 0-4 4v20a4 4 0 0 0 4 4h16a4 4 0 0 0 4-4V11.657a4 4 0 0 0-1.172-2.829l-5.656-5.656A4 4 0 0 0 18.343 2H8Zm2 12a1 1 0 1 0 0 2h11a1 1 0 1 0 0-2H10Zm-1 5a1 1 0 0 1 1-1h7a1 1 0 1 1 0 2h-7a1 1 0 0 1-1-1Zm1 3a1 1 0 1 0 0 2h11a1 1 0 1 0 0-2H10Z";
}