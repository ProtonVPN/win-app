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
    protected override string IconGeometry { get; }
        = "M2.5 3h11a.5.5 0 0 1 .5.5V8h-2V5.5A1.5 1.5 0 0 0 10.5 4H2v-.5a.5.5 0 0 1 .5-.5ZM1 3.5v9A1.5 1.5 0 0 0 2.5 14H10v-3.5A1.5 1.5 0 0 1 11.5 9H15V3.5A1.5 1.5 0 0 0 13.5 2h-11A1.5 1.5 0 0 0 1 3.5ZM8 13v-.879a1 1 0 0 1 .293-.707L9 10.707V13H8Z";
}