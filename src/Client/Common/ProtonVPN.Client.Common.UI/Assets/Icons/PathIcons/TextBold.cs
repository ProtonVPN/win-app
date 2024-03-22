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

public class TextBold : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M3 1a1 1 0 0 0 0 2v10a1 1 0 1 0 0 2h7a4 4 0 0 0 2.063-7.428A4 4 0 0 0 9 1H3Zm6 6a2 2 0 1 0 0-4H5v4h4ZM5 9v4h5a2 2 0 1 0 0-4H5Z";

    protected override string IconGeometry20 { get; }
        = "M3.75 1.25a1.25 1.25 0 1 0 0 2.5v12.5a1.25 1.25 0 1 0 0 2.5h8.75a5 5 0 0 0 2.579-9.284A5 5 0 0 0 11.25 1.25h-7.5Zm7.5 7.5a2.5 2.5 0 0 0 0-5h-5v5h5Zm-5 2.5v5h6.25a2.5 2.5 0 0 0 0-5H6.25Z"; 

    protected override string IconGeometry24 { get; }
        = "M4.5 1.5a1.5 1.5 0 1 0 0 3v15a1.5 1.5 0 0 0 0 3H15a6 6 0 0 0 3.095-11.141A6 6 0 0 0 13.5 1.5h-9Zm9 9a3 3 0 1 0 0-6h-6v6h6Zm-6 3v6H15a3 3 0 1 0 0-6H7.5Z"; 

    protected override string IconGeometry32 { get; }
        = "M6 2a2 2 0 1 0 0 4v20a2 2 0 1 0 0 4h14a8 8 0 0 0 4.126-14.855A8 8 0 0 0 18 2H6Zm12 12a4 4 0 0 0 0-8h-8v8h8Zm-8 4v8h10a4 4 0 0 0 0-8H10Z";
}