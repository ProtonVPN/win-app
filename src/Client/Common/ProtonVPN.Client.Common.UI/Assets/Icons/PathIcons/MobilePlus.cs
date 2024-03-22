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

public class MobilePlus : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M1 3a2 2 0 0 1 2-2h6a2 2 0 0 1 2 2v3h-1V3a1 1 0 0 0-1-1H3a1 1 0 0 0-1 1v10a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1v-2h1v2a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V3Zm6.5 9.5a.5.5 0 0 1-.5.5H5a.5.5 0 0 1 0-1h2a.5.5 0 0 1 .5.5Zm5.5-6a.5.5 0 0 0-1 0V8h-1.5a.5.5 0 0 0 0 1H12v1.5a.5.5 0 0 0 1 0V9h1.5a.5.5 0 0 0 0-1H13V6.5Z";

    protected override string IconGeometry20 { get; }
        = "M1.25 3.75a2.5 2.5 0 0 1 2.5-2.5h7.5a2.5 2.5 0 0 1 2.5 2.5v2.5H12.5v-2.5c0-.69-.56-1.25-1.25-1.25h-7.5c-.69 0-1.25.56-1.25 1.25v12.5c0 .69.56 1.25 1.25 1.25h7.5c.69 0 1.25-.56 1.25-1.25v-2.5h1.25v2.5a2.5 2.5 0 0 1-2.5 2.5h-7.5a2.5 2.5 0 0 1-2.5-2.5V3.75Zm8.125 11.875c0 .345-.28.625-.625.625h-2.5a.625.625 0 1 1 0-1.25h2.5c.345 0 .625.28.625.625ZM16.25 7.5a.625.625 0 1 0-1.25 0v1.875h-1.875a.625.625 0 1 0 0 1.25H15V12.5a.625.625 0 1 0 1.25 0v-1.875h1.875a.625.625 0 1 0 0-1.25H16.25V7.5Z"; 

    protected override string IconGeometry24 { get; }
        = "M1.5 4.5a3 3 0 0 1 3-3h9a3 3 0 0 1 3 3v3H15v-3A1.5 1.5 0 0 0 13.5 3h-9A1.5 1.5 0 0 0 3 4.5v15A1.5 1.5 0 0 0 4.5 21h9a1.5 1.5 0 0 0 1.5-1.5v-3h1.5v3a3 3 0 0 1-3 3h-9a3 3 0 0 1-3-3v-15Zm9.75 14.25a.75.75 0 0 1-.75.75h-3a.75.75 0 0 1 0-1.5h3a.75.75 0 0 1 .75.75ZM19.5 9A.75.75 0 0 0 18 9v2.25h-2.25a.75.75 0 0 0 0 1.5H18V15a.75.75 0 0 0 1.5 0v-2.25h2.25a.75.75 0 0 0 0-1.5H19.5V9Z"; 

    protected override string IconGeometry32 { get; }
        = "M2 6a4 4 0 0 1 4-4h12a4 4 0 0 1 4 4v4h-2V6a2 2 0 0 0-2-2H6a2 2 0 0 0-2 2v20a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-4h2v4a4 4 0 0 1-4 4H6a4 4 0 0 1-4-4V6Zm13 19a1 1 0 0 1-1 1h-4a1 1 0 1 1 0-2h4a1 1 0 0 1 1 1Zm11-13a1 1 0 1 0-2 0v3h-3a1 1 0 1 0 0 2h3v3a1 1 0 1 0 2 0v-3h3a1 1 0 1 0 0-2h-3v-3Z";
}