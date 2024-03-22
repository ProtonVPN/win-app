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

public class ServersFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M3 2a2 2 0 0 0-2 2v2a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V4a2 2 0 0 0-2-2H3Zm0 7a2 2 0 0 0-2 2v2a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2v-2a2 2 0 0 0-2-2H3Zm6.75-3.5a.75.75 0 1 1 1.5 0 .75.75 0 0 1-1.5 0Zm2 0a.75.75 0 1 1 1.5 0 .75.75 0 0 1-1.5 0Zm-2 7a.75.75 0 1 1 1.5 0 .75.75 0 0 1-1.5 0Zm2 0a.75.75 0 1 1 1.5 0 .75.75 0 0 1-1.5 0Z";

    protected override string IconGeometry20 { get; }
        = "M3.75 1.25a2.5 2.5 0 0 0-2.5 2.5v3.125a2.5 2.5 0 0 0 2.5 2.5h12.5a2.5 2.5 0 0 0 2.5-2.5V3.75a2.5 2.5 0 0 0-2.5-2.5H3.75Zm11.875 5a.625.625 0 1 0 0-1.25.625.625 0 0 0 0 1.25Zm-2.5 0a.625.625 0 1 0 0-1.25.625.625 0 0 0 0 1.25ZM3.75 10.625a2.5 2.5 0 0 0-2.5 2.5v3.125a2.5 2.5 0 0 0 2.5 2.5h12.5a2.5 2.5 0 0 0 2.5-2.5v-3.125a2.5 2.5 0 0 0-2.5-2.5H3.75ZM16.25 15A.625.625 0 1 1 15 15a.625.625 0 0 1 1.25 0Zm-2.5 0a.625.625 0 1 1-1.25 0 .625.625 0 0 1 1.25 0Z"; 

    protected override string IconGeometry24 { get; }
        = "M1.5 4.5a3 3 0 0 1 3-3h15a3 3 0 0 1 3 3v3.75a3 3 0 0 1-3 3h-15a3 3 0 0 1-3-3V4.5Zm14.25 1.25a1 1 0 1 0 0 2 1 1 0 0 0 0-2Zm3 0a1 1 0 1 0 0 2 1 1 0 0 0 0-2Zm-17.25 10a3 3 0 0 1 3-3h15a3 3 0 0 1 3 3v3.75a3 3 0 0 1-3 3h-15a3 3 0 0 1-3-3v-3.75ZM15.75 17a1 1 0 1 0 0 2 1 1 0 0 0 0-2Zm3 0a1 1 0 1 0 0 2 1 1 0 0 0 0-2Z"; 

    protected override string IconGeometry32 { get; }
        = "M6 2a4 4 0 0 0-4 4v5a4 4 0 0 0 4 4h20a4 4 0 0 0 4-4V6a4 4 0 0 0-4-4H6Zm0 15a4 4 0 0 0-4 4v5a4 4 0 0 0 4 4h20a4 4 0 0 0 4-4v-5a4 4 0 0 0-4-4H6Zm13.5-8a1.5 1.5 0 1 1 3 0 1.5 1.5 0 0 1-3 0Zm4 0a1.5 1.5 0 1 1 3 0 1.5 1.5 0 0 1-3 0Zm-4 15a1.5 1.5 0 1 1 3 0 1.5 1.5 0 0 1-3 0Zm4 0a1.5 1.5 0 1 1 3 0 1.5 1.5 0 0 1-3 0Z";
}