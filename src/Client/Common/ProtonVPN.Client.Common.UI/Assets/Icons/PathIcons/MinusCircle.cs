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

public class MinusCircle : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M6 8a.5.5 0 0 0 0 1h5a.5.5 0 0 0 0-1H6Z M8.5 1a7.5 7.5 0 1 0 0 15 7.5 7.5 0 0 0 0-15ZM2 8.5a6.5 6.5 0 1 1 13 0 6.5 6.5 0 0 1-13 0Z";

    protected override string IconGeometry20 { get; }
        = "M10.625 20a9.375 9.375 0 1 0 0-18.75 9.375 9.375 0 1 0 0 18.75Zm3.349-10.045H7.277a.67.67 0 1 0 0 1.34h6.697a.67.67 0 1 0 0-1.34Z"; 

    protected override string IconGeometry24 { get; }
        = "M12.75 24C18.963 24 24 18.963 24 12.75S18.963 1.5 12.75 1.5 1.5 6.537 1.5 12.75 6.537 24 12.75 24Zm4.018-12.054H8.733a.804.804 0 1 0 0 1.608h8.035a.804.804 0 0 0 0-1.608Z"; 

    protected override string IconGeometry32 { get; }
        = "M17 32c8.284 0 15-6.716 15-15 0-8.284-6.716-15-15-15C8.716 2 2 8.716 2 17c0 8.284 6.716 15 15 15Zm5.358-16.071H11.644a1.071 1.071 0 0 0 0 2.143h10.714a1.071 1.071 0 1 0 0-2.143Z";
}