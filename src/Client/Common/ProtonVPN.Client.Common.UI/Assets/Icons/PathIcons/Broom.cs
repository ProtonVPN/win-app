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

public class Broom : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M13 15H5.2a.2.2 0 0 1-.2-.2v-1.267a.01.01 0 0 0-.018-.005l-.004.005-.546.82-.299.447-.074.11a.2.2 0 0 1-.166.09H2V8a2 2 0 0 1 2-2h2V2a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v4h2a2 2 0 0 1 2 2v7h-1Zm0-1H6v-.467c0-1-1.296-1.392-1.85-.56L3.465 14H3v-4h10v4Zm0-5H3V8a1 1 0 0 1 1-1h3V2h2v5h3a1 1 0 0 1 1 1v1Z";

    protected override string IconGeometry20 { get; }
        = "M16.25 18.75H6.5a.25.25 0 0 1-.25-.25v-1.584c0-.012-.016-.017-.023-.007l-.005.007-.683 1.025-.372.559-.093.139a.249.249 0 0 1-.208.111H2.5V10A2.5 2.5 0 0 1 5 7.5h2.5v-5c0-.69.56-1.25 1.25-1.25h2.5c.69 0 1.25.56 1.25 1.25v5H15a2.5 2.5 0 0 1 2.5 2.5v8.75h-1.25Zm0-1.25H7.5v-.584c0-1.249-1.62-1.74-2.313-.7L4.331 17.5H3.75v-5h12.5v5Zm0-6.25H3.75V10c0-.69.56-1.25 1.25-1.25h3.75V2.5h2.5v6.25H15c.69 0 1.25.56 1.25 1.25v1.25Z"; 

    protected override string IconGeometry24 { get; }
        = "M19.5 22.5H7.8a.3.3 0 0 1-.3-.3v-1.9c0-.015-.02-.021-.027-.009l-.006.008-.82 1.23-.447.671-.11.166a.298.298 0 0 1-.25.134H3V12a3 3 0 0 1 3-3h3V3a1.5 1.5 0 0 1 1.5-1.5h3A1.5 1.5 0 0 1 15 3v6h3a3 3 0 0 1 3 3v10.5h-1.5Zm0-1.5H9v-.7c0-1.5-1.944-2.088-2.776-.84L5.197 21H4.5v-6h15v6Zm0-7.5h-15V12A1.5 1.5 0 0 1 6 10.5h4.5V3h3v7.5H18a1.5 1.5 0 0 1 1.5 1.5v1.5Z"; 

    protected override string IconGeometry32 { get; }
        = "M26 30H10.4a.4.4 0 0 1-.4-.4v-2.534c0-.02-.026-.027-.037-.011l-.007.011-1.093 1.64-.596.894-.148.222a.4.4 0 0 1-.333.178H4V16a4 4 0 0 1 4-4h4V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v8h4a4 4 0 0 1 4 4v14h-2Zm0-2H12v-.934c0-1.999-2.592-2.783-3.7-1.12L6.93 28H6v-8h20v8Zm0-10H6v-2a2 2 0 0 1 2-2h6V4h4v10h6a2 2 0 0 1 2 2v2Z";
}