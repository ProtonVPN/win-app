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

public class ArchiveBox : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M5.5 8.5A.5.5 0 0 1 6 8h4a.5.5 0 0 1 0 1H6a.5.5 0 0 1-.5-.5Z M2 6a1 1 0 0 1-1-1V3a1 1 0 0 1 1-1h12a1 1 0 0 1 1 1v2a1 1 0 0 1-1 1v7a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V6Zm0-3h12v2H2V3Zm1 3v7a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1V6H3Z";

    protected override string IconGeometry20 { get; }
        = "M6.875 10c0-.345.28-.625.625-.625h5a.625.625 0 1 1 0 1.25h-5A.625.625 0 0 1 6.875 10Z M2.5 7.5c-.69 0-1.25-.56-1.25-1.25v-2.5c0-.69.56-1.25 1.25-1.25h15c.69 0 1.25.56 1.25 1.25v2.5c0 .69-.56 1.25-1.25 1.25v8.75a2.5 2.5 0 0 1-2.5 2.5H5a2.5 2.5 0 0 1-2.5-2.5V7.5Zm0-3.75h15v2.5h-15v-2.5ZM3.75 7.5v8.75c0 .69.56 1.25 1.25 1.25h10c.69 0 1.25-.56 1.25-1.25V7.5H3.75Z"; 

    protected override string IconGeometry24 { get; }
        = "M8.25 12a.75.75 0 0 1 .75-.75h6a.75.75 0 0 1 0 1.5H9a.75.75 0 0 1-.75-.75Z M3 9a1.5 1.5 0 0 1-1.5-1.5v-3A1.5 1.5 0 0 1 3 3h18a1.5 1.5 0 0 1 1.5 1.5v3A1.5 1.5 0 0 1 21 9v10.5a3 3 0 0 1-3 3H6a3 3 0 0 1-3-3V9Zm0-4.5h18v3H3v-3ZM4.5 9v10.5A1.5 1.5 0 0 0 6 21h12a1.5 1.5 0 0 0 1.5-1.5V9h-15Z"; 

    protected override string IconGeometry32 { get; }
        = "M11 16a1 1 0 0 1 1-1h8a1 1 0 1 1 0 2h-8a1 1 0 0 1-1-1Z M4 12a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h24a2 2 0 0 1 2 2v4a2 2 0 0 1-2 2v14a4 4 0 0 1-4 4H8a4 4 0 0 1-4-4V12Zm0-6h24v4H4V6Zm2 6v14a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V12H6Z";
}