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

public class File : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M13 6v7a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V3a1 1 0 0 1 1-1h5v2.5A1.5 1.5 0 0 0 10.5 6H13Zm-.414-1L10 2.414V4.5a.5.5 0 0 0 .5.5h2.086ZM2 3a2 2 0 0 1 2-2h5.172a2 2 0 0 1 1.414.586l2.828 2.828A2 2 0 0 1 14 5.828V13a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V3Z";

    protected override string IconGeometry20 { get; }
        = "M16.25 7.5v8.75c0 .69-.56 1.25-1.25 1.25H5c-.69 0-1.25-.56-1.25-1.25V3.75c0-.69.56-1.25 1.25-1.25h6.25v3.125c0 1.036.84 1.875 1.875 1.875h3.125Zm-.518-1.25L12.5 3.018v2.607c0 .345.28.625.625.625h2.607ZM2.5 3.75A2.5 2.5 0 0 1 5 1.25h6.464a2.5 2.5 0 0 1 1.768.732l3.536 3.536a2.5 2.5 0 0 1 .732 1.768v8.964a2.5 2.5 0 0 1-2.5 2.5H5a2.5 2.5 0 0 1-2.5-2.5V3.75Z"; 

    protected override string IconGeometry24 { get; }
        = "M19.5 9v10.5A1.5 1.5 0 0 1 18 21H6a1.5 1.5 0 0 1-1.5-1.5v-15A1.5 1.5 0 0 1 6 3h7.5v3.75A2.25 2.25 0 0 0 15.75 9h3.75Zm-.621-1.5L15 3.621V6.75c0 .414.336.75.75.75h3.129ZM3 4.5a3 3 0 0 1 3-3h7.757a3 3 0 0 1 2.122.879L20.12 6.62A3 3 0 0 1 21 8.743V19.5a3 3 0 0 1-3 3H6a3 3 0 0 1-3-3v-15Z"; 

    protected override string IconGeometry32 { get; }
        = "M26 12v14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h10v5a3 3 0 0 0 3 3h5Zm-.828-2L20 4.828V9a1 1 0 0 0 1 1h4.172ZM4 6a4 4 0 0 1 4-4h10.343a4 4 0 0 1 2.829 1.172l5.656 5.656A4 4 0 0 1 28 11.657V26a4 4 0 0 1-4 4H8a4 4 0 0 1-4-4V6Z";
}