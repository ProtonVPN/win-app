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

public class ChevronSquareRight : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M11 14V2H3a1 1 0 0 0-1 1v10a1 1 0 0 0 1 1h8Zm-8 1a2 2 0 0 1-2-2V3a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v10a2 2 0 0 1-2 2H3Zm9-13h1a1 1 0 0 1 1 1v10a1 1 0 0 1-1 1h-1V2Z M4.646 4.646a.5.5 0 0 1 .708 0l3 3a.5.5 0 0 1 0 .708l-3 3a.5.5 0 0 1-.708-.708L7.293 8 4.646 5.354a.5.5 0 0 1 0-.708Z";

    protected override string IconGeometry20 { get; }
        = "M13.75 17.5v-15h-10c-.69 0-1.25.56-1.25 1.25v12.5c0 .69.56 1.25 1.25 1.25h10Zm-10 1.25a2.5 2.5 0 0 1-2.5-2.5V3.75a2.5 2.5 0 0 1 2.5-2.5h12.5a2.5 2.5 0 0 1 2.5 2.5v12.5a2.5 2.5 0 0 1-2.5 2.5H3.75ZM15 2.5h1.25c.69 0 1.25.56 1.25 1.25v12.5c0 .69-.56 1.25-1.25 1.25H15v-15Z M5.808 5.808a.625.625 0 0 1 .884 0l3.75 3.75a.625.625 0 0 1 0 .884l-3.75 3.75a.625.625 0 1 1-.884-.884L9.116 10 5.808 6.692a.625.625 0 0 1 0-.884Z"; 

    protected override string IconGeometry24 { get; }
        = "M16.5 21V3h-12A1.5 1.5 0 0 0 3 4.5v15A1.5 1.5 0 0 0 4.5 21h12Zm-12 1.5a3 3 0 0 1-3-3v-15a3 3 0 0 1 3-3h15a3 3 0 0 1 3 3v15a3 3 0 0 1-3 3h-15ZM18 3h1.5A1.5 1.5 0 0 1 21 4.5v15a1.5 1.5 0 0 1-1.5 1.5H18V3Z M6.97 6.97a.75.75 0 0 1 1.06 0l4.5 4.5a.75.75 0 0 1 0 1.06l-4.5 4.5a.75.75 0 0 1-1.06-1.06L10.94 12 6.97 8.03a.75.75 0 0 1 0-1.06Z"; 

    protected override string IconGeometry32 { get; }
        = "M22 28V4H6a2 2 0 0 0-2 2v20a2 2 0 0 0 2 2h16ZM6 30a4 4 0 0 1-4-4V6a4 4 0 0 1 4-4h20a4 4 0 0 1 4 4v20a4 4 0 0 1-4 4H6ZM24 4h2a2 2 0 0 1 2 2v20a2 2 0 0 1-2 2h-2V4Z M9.293 9.293a1 1 0 0 1 1.414 0l6 6a1 1 0 0 1 0 1.414l-6 6a1 1 0 0 1-1.414-1.414L14.586 16l-5.293-5.293a1 1 0 0 1 0-1.414Z";
}