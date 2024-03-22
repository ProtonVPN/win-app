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

public class Squares : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M6.818 1A1.818 1.818 0 0 0 5 2.818V4h1V2.818C6 2.366 6.366 2 6.818 2H13a1 1 0 0 1 1 1v6a1 1 0 0 1-1 1h-1v1h1a2 2 0 0 0 2-2V3a2 2 0 0 0-2-2H6.818ZM2 7a1 1 0 0 1 1-1h6a1 1 0 0 1 1 1v6a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V7ZM1 7a2 2 0 0 1 2-2h6a2 2 0 0 1 2 2v6a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V7Z";

    protected override string IconGeometry20 { get; }
        = "M8.523 1.25A2.273 2.273 0 0 0 6.25 3.523V5H7.5V3.523c0-.565.458-1.023 1.023-1.023h7.727c.69 0 1.25.56 1.25 1.25v7.5c0 .69-.56 1.25-1.25 1.25H15v1.25h1.25a2.5 2.5 0 0 0 2.5-2.5v-7.5a2.5 2.5 0 0 0-2.5-2.5H8.523ZM2.5 8.75c0-.69.56-1.25 1.25-1.25h7.5c.69 0 1.25.56 1.25 1.25v7.5c0 .69-.56 1.25-1.25 1.25h-7.5c-.69 0-1.25-.56-1.25-1.25v-7.5Zm-1.25 0a2.5 2.5 0 0 1 2.5-2.5h7.5a2.5 2.5 0 0 1 2.5 2.5v7.5a2.5 2.5 0 0 1-2.5 2.5h-7.5a2.5 2.5 0 0 1-2.5-2.5v-7.5Z"; 

    protected override string IconGeometry24 { get; }
        = "M10.227 1.5A2.727 2.727 0 0 0 7.5 4.227V6H9V4.227C9 3.55 9.55 3 10.227 3H19.5A1.5 1.5 0 0 1 21 4.5v9a1.5 1.5 0 0 1-1.5 1.5H18v1.5h1.5a3 3 0 0 0 3-3v-9a3 3 0 0 0-3-3h-9.273ZM3 10.5A1.5 1.5 0 0 1 4.5 9h9a1.5 1.5 0 0 1 1.5 1.5v9a1.5 1.5 0 0 1-1.5 1.5h-9A1.5 1.5 0 0 1 3 19.5v-9Zm-1.5 0a3 3 0 0 1 3-3h9a3 3 0 0 1 3 3v9a3 3 0 0 1-3 3h-9a3 3 0 0 1-3-3v-9Z"; 

    protected override string IconGeometry32 { get; }
        = "M13.636 2A3.636 3.636 0 0 0 10 5.636V8h2V5.636C12 4.733 12.733 4 13.636 4H26a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2h-2v2h2a4 4 0 0 0 4-4V6a4 4 0 0 0-4-4H13.636ZM4 14a2 2 0 0 1 2-2h12a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V14Zm-2 0a4 4 0 0 1 4-4h12a4 4 0 0 1 4 4v12a4 4 0 0 1-4 4H6a4 4 0 0 1-4-4V14Z";
}