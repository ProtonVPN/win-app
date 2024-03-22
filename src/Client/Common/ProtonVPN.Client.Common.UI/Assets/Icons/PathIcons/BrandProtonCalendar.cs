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

public class BrandProtonCalendar : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M1 3.5A1.5 1.5 0 0 1 2.5 2h11A1.5 1.5 0 0 1 15 3.5V9h-3.5a1.5 1.5 0 0 0-1.5 1.5V14H2.5A1.5 1.5 0 0 1 1 12.5v-9ZM13.5 3h-11a.5.5 0 0 0-.5.5V4h8.5A1.5 1.5 0 0 1 12 5.5V8h2V3.5a.5.5 0 0 0-.5-.5ZM11 8.05V5.5a.5.5 0 0 0-.5-.5H2v7.5a.5.5 0 0 0 .5.5H7v-.704a2 2 0 0 1 .586-1.414l2.06-2.06A2.495 2.495 0 0 1 11 8.05Zm-3 4.246V13h1v-2.118l-.707.707a1 1 0 0 0-.293.707Z";

    protected override string IconGeometry20 { get; }
        = "M2 5.3A2.3 2.3 0 0 1 4.3 3h11.4A2.3 2.3 0 0 1 18 5.3V11h-4.5a1.5 1.5 0 0 0-1.5 1.5V17H4.3A2.3 2.3 0 0 1 2 14.7V5.3ZM4.3 4A1.3 1.3 0 0 0 3 5.3V6h9a2 2 0 0 1 2 2v2h3V5.3A1.3 1.3 0 0 0 15.7 4H4.3Zm8.7 6.05V8a1 1 0 0 0-1-1H3v7.7A1.3 1.3 0 0 0 4.3 16H8v-1.043a2.5 2.5 0 0 1 .931-1.947l2.655-2.14.009.011A2.498 2.498 0 0 1 13 10.05Zm-2 2.577L9.559 13.79A1.5 1.5 0 0 0 9 14.957V16h2v-3.373Z"; 

    protected override string IconGeometry24 { get; }
        = ""; 

    protected override string IconGeometry32 { get; }
        = "";
}