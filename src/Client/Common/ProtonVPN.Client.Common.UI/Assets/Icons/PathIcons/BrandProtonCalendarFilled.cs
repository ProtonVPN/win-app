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

public class BrandProtonCalendarFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M2.5 3h11a.5.5 0 0 1 .5.5V8h-2V5.5A1.5 1.5 0 0 0 10.5 4H2v-.5a.5.5 0 0 1 .5-.5ZM1 3.5v9A1.5 1.5 0 0 0 2.5 14H10v-3.5A1.5 1.5 0 0 1 11.5 9H15V3.5A1.5 1.5 0 0 0 13.5 2h-11A1.5 1.5 0 0 0 1 3.5ZM8 13v-.879a1 1 0 0 1 .293-.707L9 10.707V13H8Z";

    protected override string IconGeometry20 { get; }
        = "M3.125 3.75h13.75c.345 0 .625.28.625.625V10H15V6.875C15 5.839 14.16 5 13.125 5H2.5v-.625c0-.345.28-.625.625-.625Zm-1.875.625v11.25c0 1.035.84 1.875 1.875 1.875H12.5v-4.375c0-1.036.84-1.875 1.875-1.875h4.375V4.375c0-1.036-.84-1.875-1.875-1.875H3.125c-1.036 0-1.875.84-1.875 1.875ZM10 16.25v-1.098c0-.332.132-.65.366-.884l.884-.884v2.866H10Z"; 

    protected override string IconGeometry24 { get; }
        = "M3.75 4.5h16.5a.75.75 0 0 1 .75.75V12h-3V8.25A2.25 2.25 0 0 0 15.75 6H3v-.75a.75.75 0 0 1 .75-.75Zm-2.25.75v13.5A2.25 2.25 0 0 0 3.75 21H15v-5.25a2.25 2.25 0 0 1 2.25-2.25h5.25V5.25A2.25 2.25 0 0 0 20.25 3H3.75A2.25 2.25 0 0 0 1.5 5.25ZM12 19.5v-1.318c0-.398.158-.78.44-1.06l1.06-1.061V19.5H12Z"; 

    protected override string IconGeometry32 { get; }
        = "M5 6h22a1 1 0 0 1 1 1v9h-4v-5a3 3 0 0 0-3-3H4V7a1 1 0 0 1 1-1ZM2 7v18a3 3 0 0 0 3 3h15v-7a3 3 0 0 1 3-3h7V7a3 3 0 0 0-3-3H5a3 3 0 0 0-3 3Zm14 19v-1.757a2 2 0 0 1 .586-1.415L18 21.414V26h-2Z";
}