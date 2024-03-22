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

public class Mobile : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M5 2h6a1 1 0 0 1 1 1v10a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1V3a1 1 0 0 1 1-1ZM3 3a2 2 0 0 1 2-2h6a2 2 0 0 1 2 2v10a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V3Zm6 10a.5.5 0 0 0 0-1H7a.5.5 0 0 0 0 1h2Z";

    protected override string IconGeometry20 { get; }
        = "M6.25 2.5h7.5c.69 0 1.25.56 1.25 1.25v12.5c0 .69-.56 1.25-1.25 1.25h-7.5c-.69 0-1.25-.56-1.25-1.25V3.75c0-.69.56-1.25 1.25-1.25Zm-2.5 1.25a2.5 2.5 0 0 1 2.5-2.5h7.5a2.5 2.5 0 0 1 2.5 2.5v12.5a2.5 2.5 0 0 1-2.5 2.5h-7.5a2.5 2.5 0 0 1-2.5-2.5V3.75Zm7.5 12.5a.625.625 0 1 0 0-1.25h-2.5a.625.625 0 1 0 0 1.25h2.5Z"; 

    protected override string IconGeometry24 { get; }
        = "M7.5 3h9A1.5 1.5 0 0 1 18 4.5v15a1.5 1.5 0 0 1-1.5 1.5h-9A1.5 1.5 0 0 1 6 19.5v-15A1.5 1.5 0 0 1 7.5 3Zm-3 1.5a3 3 0 0 1 3-3h9a3 3 0 0 1 3 3v15a3 3 0 0 1-3 3h-9a3 3 0 0 1-3-3v-15Zm9 15a.75.75 0 0 0 0-1.5h-3a.75.75 0 0 0 0 1.5h3Z"; 

    protected override string IconGeometry32 { get; }
        = "M10 4h12a2 2 0 0 1 2 2v20a2 2 0 0 1-2 2H10a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2ZM6 6a4 4 0 0 1 4-4h12a4 4 0 0 1 4 4v20a4 4 0 0 1-4 4H10a4 4 0 0 1-4-4V6Zm12 20a1 1 0 1 0 0-2h-4a1 1 0 1 0 0 2h4Z";
}