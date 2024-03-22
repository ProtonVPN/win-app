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

public class CreditCard : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M3 4h10a1 1 0 0 1 1 1v1H2V5a1 1 0 0 1 1-1ZM1 8V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v6a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8Zm13 0v3a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V8h12Z";

    protected override string IconGeometry20 { get; }
        = "M3.75 5h12.5c.69 0 1.25.56 1.25 1.25V7.5h-15V6.25C2.5 5.56 3.06 5 3.75 5Zm-2.5 5V6.25a2.5 2.5 0 0 1 2.5-2.5h12.5a2.5 2.5 0 0 1 2.5 2.5v7.5a2.5 2.5 0 0 1-2.5 2.5H3.75a2.5 2.5 0 0 1-2.5-2.5V10Zm16.25 0v3.75c0 .69-.56 1.25-1.25 1.25H3.75c-.69 0-1.25-.56-1.25-1.25V10h15Z"; 

    protected override string IconGeometry24 { get; }
        = "M4.5 6h15A1.5 1.5 0 0 1 21 7.5V9H3V7.5A1.5 1.5 0 0 1 4.5 6Zm-3 6V7.5a3 3 0 0 1 3-3h15a3 3 0 0 1 3 3v9a3 3 0 0 1-3 3h-15a3 3 0 0 1-3-3V12ZM21 12v4.5a1.5 1.5 0 0 1-1.5 1.5h-15A1.5 1.5 0 0 1 3 16.5V12h18Z"; 

    protected override string IconGeometry32 { get; }
        = "M6 8h20a2 2 0 0 1 2 2v2H4v-2a2 2 0 0 1 2-2Zm-4 8v-6a4 4 0 0 1 4-4h20a4 4 0 0 1 4 4v12a4 4 0 0 1-4 4H6a4 4 0 0 1-4-4v-6Zm26 0v6a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2v-6h24Z";
}