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

public class Calendar3Day : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M4 3V1.5a.5.5 0 0 1 1 0V3h6V1.5a.5.5 0 0 1 1 0V3a3 3 0 0 1 3 3v6a3 3 0 0 1-3 3H4a3 3 0 0 1-3-3V6a3 3 0 0 1 3-3Zm0 1h1v10H4a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2Zm7 0v10h1a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2h-1ZM6 4v10h4V4H6Z";

    protected override string IconGeometry20 { get; }
        = "M5 3.75V1.875a.625.625 0 1 1 1.25 0V3.75h7.5V1.875a.625.625 0 1 1 1.25 0V3.75a3.75 3.75 0 0 1 3.75 3.75V15A3.75 3.75 0 0 1 15 18.75H5A3.75 3.75 0 0 1 1.25 15V7.5A3.75 3.75 0 0 1 5 3.75ZM5 5h1.25v12.5H5A2.5 2.5 0 0 1 2.5 15V7.5A2.5 2.5 0 0 1 5 5Zm8.75 0v12.5H15a2.5 2.5 0 0 0 2.5-2.5V7.5A2.5 2.5 0 0 0 15 5h-1.25ZM7.5 5v12.5h5V5h-5Z"; 

    protected override string IconGeometry24 { get; }
        = "M6 4.5V2.25a.75.75 0 0 1 1.5 0V4.5h9V2.25a.75.75 0 0 1 1.5 0V4.5A4.5 4.5 0 0 1 22.5 9v9a4.5 4.5 0 0 1-4.5 4.5H6A4.5 4.5 0 0 1 1.5 18V9A4.5 4.5 0 0 1 6 4.5ZM6 6h1.5v15H6a3 3 0 0 1-3-3V9a3 3 0 0 1 3-3Zm10.5 0v15H18a3 3 0 0 0 3-3V9a3 3 0 0 0-3-3h-1.5ZM9 6v15h6V6H9Z"; 

    protected override string IconGeometry32 { get; }
        = "M8 6V3a1 1 0 0 1 2 0v3h12V3a1 1 0 1 1 2 0v3a6 6 0 0 1 6 6v12a6 6 0 0 1-6 6H8a6 6 0 0 1-6-6V12a6 6 0 0 1 6-6Zm0 2h2v20H8a4 4 0 0 1-4-4V12a4 4 0 0 1 4-4Zm14 0v20h2a4 4 0 0 0 4-4V12a4 4 0 0 0-4-4h-2ZM12 8v20h8V8h-8Z";
}