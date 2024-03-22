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

public class PaintRoller : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M2 3a1 1 0 0 1 1-1h8a1 1 0 0 1 1 1v2a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V3Zm1-2a2 2 0 0 0-2 2v2a2 2 0 0 0 2 2h8a2 2 0 0 0 2-2V4h.5a.5.5 0 0 1 .5.5v3a.5.5 0 0 1-.5.5h-5A1.5 1.5 0 0 0 7 9.5v.5a1 1 0 0 0-1 1v4a1 1 0 0 0 1 1h1a1 1 0 0 0 1-1v-4a1 1 0 0 0-1-1v-.5a.5.5 0 0 1 .5-.5h5A1.5 1.5 0 0 0 15 7.5v-3A1.5 1.5 0 0 0 13.5 3H13a2 2 0 0 0-2-2H3Zm4 10v4h1v-4H7Z";

    protected override string IconGeometry20 { get; }
        = "M2.5 3.75c0-.69.56-1.25 1.25-1.25h10.625c.69 0 1.25.56 1.25 1.25v2.5c0 .69-.56 1.25-1.25 1.25H3.75c-.69 0-1.25-.56-1.25-1.25v-2.5Zm1.25-2.5a2.5 2.5 0 0 0-2.5 2.5v2.5a2.5 2.5 0 0 0 2.5 2.5h10.625a2.5 2.5 0 0 0 2.5-2.5V5c.345 0 .625.28.625.625V8.75c0 .345-.28.625-.625.625H11.25c-1.036 0-1.875.84-1.875 1.875v.625c-.69 0-1.25.56-1.25 1.25V17.5c0 .69.56 1.25 1.25 1.25h1.25c.69 0 1.25-.56 1.25-1.25v-4.375c0-.69-.56-1.25-1.25-1.25v-.625c0-.345.28-.625.625-.625h5.625c1.035 0 1.875-.84 1.875-1.875V5.625c0-1.036-.84-1.875-1.875-1.875a2.5 2.5 0 0 0-2.5-2.5H3.75ZM10 13.125h-.625V17.5h1.25v-4.375H10Z"; 

    protected override string IconGeometry24 { get; }
        = "M3 4.5A1.5 1.5 0 0 1 4.5 3h12.75a1.5 1.5 0 0 1 1.5 1.5v3a1.5 1.5 0 0 1-1.5 1.5H4.5A1.5 1.5 0 0 1 3 7.5v-3Zm1.5-3a3 3 0 0 0-3 3v3a3 3 0 0 0 3 3h12.75a3 3 0 0 0 3-3V6a.75.75 0 0 1 .75.75v3.75a.75.75 0 0 1-.75.75H13.5a2.25 2.25 0 0 0-2.25 2.25v.75a1.5 1.5 0 0 0-1.5 1.5V21a1.5 1.5 0 0 0 1.5 1.5h1.5a1.5 1.5 0 0 0 1.5-1.5v-5.25a1.5 1.5 0 0 0-1.5-1.5v-.75a.75.75 0 0 1 .75-.75h6.75a2.25 2.25 0 0 0 2.25-2.25V6.75a2.25 2.25 0 0 0-2.25-2.25 3 3 0 0 0-3-3H4.5ZM12 15.75h-.75V21h1.5v-5.25H12Z"; 

    protected override string IconGeometry32 { get; }
        = "M4 6a2 2 0 0 1 2-2h17a2 2 0 0 1 2 2v4a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6Zm2-4a4 4 0 0 0-4 4v4a4 4 0 0 0 4 4h17a4 4 0 0 0 4-4V8a1 1 0 0 1 1 1v5a1 1 0 0 1-1 1h-9a3 3 0 0 0-3 3v1a2 2 0 0 0-2 2v7a2 2 0 0 0 2 2h2a2 2 0 0 0 2-2v-7a2 2 0 0 0-2-2v-1a1 1 0 0 1 1-1h9a3 3 0 0 0 3-3V9a3 3 0 0 0-3-3 4 4 0 0 0-4-4H6Zm10 19h-1v7h2v-7h-1Z";
}