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

public class TrashClock : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M12 10a.5.5 0 0 0-1 0v1.5a.5.5 0 0 0 .146.354l1 1a.5.5 0 0 0 .708-.708L12 11.293V10Z M5 0a.5.5 0 0 0-.447.276L3.69 2H.5a.5.5 0 0 0 0 1h1.272l.435 9.568A1.5 1.5 0 0 0 3.706 14H7.75v-.012a4.5 4.5 0 1 0 3.525-6.983l.2-4.005H12.5a.5.5 0 0 0 0-1H9.309L8.447.276A.5.5 0 0 0 8 0H5Zm2 11.5c0 .526.09 1.03.256 1.5h-3.55a.5.5 0 0 1-.5-.477L2.773 3h7.701l-.208 4.171A4.502 4.502 0 0 0 7 11.5ZM8.191 2l-.5-1H5.309l-.5 1h3.382ZM11.5 8a3.5 3.5 0 1 0 0 7 3.5 3.5 0 0 0 0-7Z";
}