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

public class BrandProton : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M3 1h6.5a4.5 4.5 0 1 1 0 9h-3a.5.5 0 0 0-.5.5V15H3v-5a3 3 0 0 1 3-3h3.5a1.5 1.5 0 0 0 0-3H6v2H3V1Zm1 1v3h1V3h4.5a2.5 2.5 0 0 1 0 5H6a2 2 0 0 0-2 2v4h1v-3.5A1.5 1.5 0 0 1 6.5 9h3a3.5 3.5 0 1 0 0-7H4Z";

    protected override string IconGeometry20 { get; }
        = "M3.75 1.25h8.125a5.625 5.625 0 0 1 0 11.25h-3.75a.625.625 0 0 0-.625.625v5.625H3.75V12.5A3.75 3.75 0 0 1 7.5 8.75h4.375a1.875 1.875 0 0 0 0-3.75H7.5v2.5H3.75V1.25ZM5 2.5v3.75h1.25v-2.5h5.625a3.125 3.125 0 1 1 0 6.25H7.5A2.5 2.5 0 0 0 5 12.5v5h1.25v-4.375c0-1.036.84-1.875 1.875-1.875h3.75a4.375 4.375 0 0 0 0-8.75H5Z"; 

    protected override string IconGeometry24 { get; }
        = "M4.5 1.5h9.75a6.75 6.75 0 0 1 0 13.5h-4.5a.75.75 0 0 0-.75.75v6.75H4.5V15A4.5 4.5 0 0 1 9 10.5h5.25a2.25 2.25 0 0 0 0-4.5H9v3H4.5V1.5ZM6 3v4.5h1.5v-3h6.75a3.75 3.75 0 1 1 0 7.5H9a3 3 0 0 0-3 3v6h1.5v-5.25a2.25 2.25 0 0 1 2.25-2.25h4.5a5.25 5.25 0 1 0 0-10.5H6Z"; 

    protected override string IconGeometry32 { get; }
        = "M6 2h13a9 9 0 1 1 0 18h-6a1 1 0 0 0-1 1v9H6V20a6 6 0 0 1 6-6h7a3 3 0 1 0 0-6h-7v4H6V2Zm2 2v6h2V6h9a5 5 0 0 1 0 10h-7a4 4 0 0 0-4 4v8h2v-7a3 3 0 0 1 3-3h6a7 7 0 1 0 0-14H8Z";
}