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

public class ListBullets : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M5 4a.5.5 0 0 0 0 1h7.5a.5.5 0 0 0 0-1H5Zm0 5h7.5a.5.5 0 0 0 0-1H5a.5.5 0 0 0 0 1Zm-.5 3.5A.5.5 0 0 1 5 12h7.5a.5.5 0 0 1 0 1H5a.5.5 0 0 1-.5-.5ZM1.5 4a.5.5 0 0 0 0 1h1a.5.5 0 0 0 0-1h-1Zm0 5h1a.5.5 0 0 0 0-1h-1a.5.5 0 0 0 0 1ZM1 12.5a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 0 1h-1a.5.5 0 0 1-.5-.5Z";

    protected override string IconGeometry20 { get; }
        = "M2.5 6.25a1.25 1.25 0 1 0 0-2.5 1.25 1.25 0 0 0 0 2.5Zm5-1.875a.625.625 0 1 0 0 1.25h9.375a.625.625 0 1 0 0-1.25H7.5Zm0 6.25h9.375a.625.625 0 1 0 0-1.25H7.5a.625.625 0 1 0 0 1.25ZM6.875 15c0-.345.28-.625.625-.625h9.375a.625.625 0 1 1 0 1.25H7.5A.625.625 0 0 1 6.875 15ZM3.75 10a1.25 1.25 0 1 1-2.5 0 1.25 1.25 0 0 1 2.5 0ZM2.5 16.25a1.25 1.25 0 1 0 0-2.5 1.25 1.25 0 0 0 0 2.5Z"; 

    protected override string IconGeometry24 { get; }
        = "M3 7.5a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3Zm6-2.25a.75.75 0 0 0 0 1.5h11.25a.75.75 0 0 0 0-1.5H9Zm0 7.5h11.25a.75.75 0 0 0 0-1.5H9a.75.75 0 0 0 0 1.5ZM8.25 18a.75.75 0 0 1 .75-.75h11.25a.75.75 0 0 1 0 1.5H9a.75.75 0 0 1-.75-.75ZM4.5 12a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0ZM3 19.5a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3Z"; 

    protected override string IconGeometry32 { get; }
        = "M4 10a2 2 0 1 0 0-4 2 2 0 0 0 0 4Zm8-3a1 1 0 1 0 0 2h15a1 1 0 1 0 0-2H12Zm0 10h15a1 1 0 1 0 0-2H12a1 1 0 1 0 0 2Zm-1 7a1 1 0 0 1 1-1h15a1 1 0 1 1 0 2H12a1 1 0 0 1-1-1Zm-5-8a2 2 0 1 1-4 0 2 2 0 0 1 4 0ZM4 26a2 2 0 1 0 0-4 2 2 0 0 0 0 4Z";
}