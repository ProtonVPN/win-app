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

public class CircleSlash : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M3.418 4.126a6 6 0 0 0 8.456 8.456L3.418 4.126Zm9.163 7.748L4.126 3.418a6 6 0 0 1 8.456 8.456ZM8 15A7 7 0 1 0 8 1a7 7 0 0 0 0 14Z";

    protected override string IconGeometry20 { get; }
        = "M4.273 5.157a7.5 7.5 0 0 0 10.57 10.57L4.273 5.157Zm11.454 9.686L5.157 4.273a7.5 7.5 0 0 1 10.57 10.57ZM10 18.75a8.75 8.75 0 1 0 0-17.5 8.75 8.75 0 0 0 0 17.5Z"; 

    protected override string IconGeometry24 { get; }
        = "M5.128 6.188a9 9 0 0 0 12.684 12.684L5.128 6.188Zm13.744 11.624L6.188 5.128a9 9 0 0 1 12.684 12.684ZM12 22.5c5.799 0 10.5-4.701 10.5-10.5S17.799 1.5 12 1.5 1.5 6.201 1.5 12 6.201 22.5 12 22.5Z"; 

    protected override string IconGeometry32 { get; }
        = "M6.837 8.251A11.952 11.952 0 0 0 4 16c0 6.627 5.373 12 12 12 2.954 0 5.658-1.067 7.749-2.837L6.837 8.251ZM25.163 23.75 8.251 6.837A11.952 11.952 0 0 1 16 4c6.627 0 12 5.373 12 12 0 2.954-1.067 5.658-2.837 7.749ZM16 30c7.732 0 14-6.268 14-14S23.732 2 16 2 2 8.268 2 16s6.268 14 14 14Z";
}