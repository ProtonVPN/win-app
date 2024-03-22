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

public class ChevronSquareLeft : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8.354 11.354a.5.5 0 0 1-.708 0l-3-3a.5.5 0 0 1 0-.708l3-3a.5.5 0 1 1 .708.708L5.707 8l2.647 2.646a.5.5 0 0 1 0 .708Z M13 1a2 2 0 0 1 2 2v10a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V3a2 2 0 0 1 2-2h10Zm-1 1h1a1 1 0 0 1 1 1v10a1 1 0 0 1-1 1h-1V2Zm-1 12H3a1 1 0 0 1-1-1V3a1 1 0 0 1 1-1h8v12Z";

    protected override string IconGeometry20 { get; }
        = "M10.442 14.192a.625.625 0 0 1-.884 0l-3.75-3.75a.625.625 0 0 1 0-.884l3.75-3.75a.625.625 0 1 1 .884.884L7.134 10l3.308 3.308a.625.625 0 0 1 0 .884Z M16.25 1.25a2.5 2.5 0 0 1 2.5 2.5v12.5a2.5 2.5 0 0 1-2.5 2.5H3.75a2.5 2.5 0 0 1-2.5-2.5V3.75a2.5 2.5 0 0 1 2.5-2.5h12.5ZM15 2.5h1.25c.69 0 1.25.56 1.25 1.25v12.5c0 .69-.56 1.25-1.25 1.25H15v-15Zm-1.25 15h-10c-.69 0-1.25-.56-1.25-1.25V3.75c0-.69.56-1.25 1.25-1.25h10v15Z"; 

    protected override string IconGeometry24 { get; }
        = "M12.53 17.03a.75.75 0 0 1-1.06 0l-4.5-4.5a.75.75 0 0 1 0-1.06l4.5-4.5a.75.75 0 1 1 1.06 1.06L8.56 12l3.97 3.97a.75.75 0 0 1 0 1.06Z M19.5 1.5a3 3 0 0 1 3 3v15a3 3 0 0 1-3 3h-15a3 3 0 0 1-3-3v-15a3 3 0 0 1 3-3h15ZM18 3h1.5A1.5 1.5 0 0 1 21 4.5v15a1.5 1.5 0 0 1-1.5 1.5H18V3Zm-1.5 18h-12A1.5 1.5 0 0 1 3 19.5v-15A1.5 1.5 0 0 1 4.5 3h12v18Z"; 

    protected override string IconGeometry32 { get; }
        = "M16.707 22.707a1 1 0 0 1-1.414 0l-6-6a1 1 0 0 1 0-1.414l6-6a1 1 0 1 1 1.414 1.414L11.414 16l5.293 5.293a1 1 0 0 1 0 1.414Z M26 2a4 4 0 0 1 4 4v20a4 4 0 0 1-4 4H6a4 4 0 0 1-4-4V6a4 4 0 0 1 4-4h20Zm-2 2h2a2 2 0 0 1 2 2v20a2 2 0 0 1-2 2h-2V4Zm-2 24H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h16v24Z";
}