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

public class BrandBitcoin : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8 14A6 6 0 1 0 8 2a6 6 0 0 0 0 12Zm0 1A7 7 0 1 0 8 1a7 7 0 0 0 0 14Z M8 3.25a.5.5 0 0 1 .5.5V4H9a2 2 0 0 1 1.4 3.428A2.5 2.5 0 0 1 9 12h-.5v.25a.5.5 0 0 1-1 0V12h-2a.5.5 0 0 1 0-1H6V5h-.5a.5.5 0 0 1 0-1h2v-.25a.5.5 0 0 1 .5-.5ZM7 5v2h2a1 1 0 0 0 0-2H7Zm0 3v3h2a1.5 1.5 0 0 0 0-3H7Z";

    protected override string IconGeometry20 { get; }
        = "M10 17.5a7.5 7.5 0 1 0 0-15 7.5 7.5 0 0 0 0 15Zm0 1.25a8.75 8.75 0 1 0 0-17.5 8.75 8.75 0 0 0 0 17.5Z M10 4.063c.345 0 .625.28.625.625V5h.625A2.5 2.5 0 0 1 13 9.285 3.125 3.125 0 0 1 11.25 15h-.625v.313a.625.625 0 1 1-1.25 0V15h-2.5a.625.625 0 1 1 0-1.25H7.5v-7.5h-.625a.625.625 0 1 1 0-1.25h2.5v-.313c0-.345.28-.625.625-.625ZM8.75 6.25v2.5h2.5a1.25 1.25 0 1 0 0-2.5h-2.5Zm0 3.75v3.75h2.5a1.875 1.875 0 0 0 0-3.75h-2.5Z"; 

    protected override string IconGeometry24 { get; }
        = "M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18Zm0 1.5c5.799 0 10.5-4.701 10.5-10.5S17.799 1.5 12 1.5 1.5 6.201 1.5 12 6.201 22.5 12 22.5Z M12 4.875a.75.75 0 0 1 .75.75V6h.75a3 3 0 0 1 2.1 5.143A3.75 3.75 0 0 1 13.5 18h-.75v.375a.75.75 0 0 1-1.5 0V18h-3a.75.75 0 0 1 0-1.5H9v-9h-.75a.75.75 0 0 1 0-1.5h3v-.375a.75.75 0 0 1 .75-.75ZM10.5 7.5v3h3a1.5 1.5 0 0 0 0-3h-3Zm0 4.5v4.5h3a2.25 2.25 0 0 0 0-4.5h-3Z"; 

    protected override string IconGeometry32 { get; }
        = "M16 28c6.627 0 12-5.373 12-12S22.627 4 16 4 4 9.373 4 16s5.373 12 12 12Zm0 2c7.732 0 14-6.268 14-14S23.732 2 16 2 2 8.268 2 16s6.268 14 14 14Z M16 6.5a1 1 0 0 1 1 1V8h1a4 4 0 0 1 2.8 6.857A5 5 0 0 1 18 24h-1v.5a1 1 0 1 1-2 0V24h-4a1 1 0 1 1 0-2h1V10h-1a1 1 0 1 1 0-2h4v-.5a1 1 0 0 1 1-1ZM14 10v4h4a2 2 0 1 0 0-4h-4Zm0 6v6h4a3 3 0 1 0 0-6h-4Z";
}