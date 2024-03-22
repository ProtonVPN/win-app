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

public class PlusCircleFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8.5 16a7.5 7.5 0 1 0 0-15 7.5 7.5 0 0 0 0 15ZM9 5.5a.5.5 0 0 0-1 0V8H5.5a.5.5 0 0 0 0 1H8v2.5a.5.5 0 0 0 1 0V9h2.5a.5.5 0 0 0 0-1H9V5.5Z";

    protected override string IconGeometry20 { get; }
        = "M10.625 20a9.375 9.375 0 1 0 0-18.75 9.375 9.375 0 1 0 0 18.75Zm.625-13.125a.625.625 0 1 0-1.25 0V10H6.875a.625.625 0 1 0 0 1.25H10v3.125a.625.625 0 1 0 1.25 0V11.25h3.125a.625.625 0 1 0 0-1.25H11.25V6.875Z"; 

    protected override string IconGeometry24 { get; }
        = "M12.75 24C18.963 24 24 18.963 24 12.75S18.963 1.5 12.75 1.5 1.5 6.537 1.5 12.75 6.537 24 12.75 24Zm.75-15.75a.75.75 0 0 0-1.5 0V12H8.25a.75.75 0 0 0 0 1.5H12v3.75a.75.75 0 0 0 1.5 0V13.5h3.75a.75.75 0 0 0 0-1.5H13.5V8.25Z"; 

    protected override string IconGeometry32 { get; }
        = "M17 32c8.284 0 15-6.716 15-15 0-8.284-6.716-15-15-15C8.716 2 2 8.716 2 17c0 8.284 6.716 15 15 15Zm1-21a1 1 0 1 0-2 0v5h-5a1 1 0 1 0 0 2h5v5a1 1 0 1 0 2 0v-5h5a1 1 0 1 0 0-2h-5v-5Z";
}