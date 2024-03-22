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

public class Placeholder : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M5.707 2 2 5.707 1.293 5 5 1.293 5.707 2Zm3 0L2 8.707 1.293 8 8 1.293 8.707 2Zm3 0L2 11.707 1.293 11 11 1.293l.707.707Zm3 0L2 14.707 1.293 14 14 1.293l.707.707Zm0 3L5 14.707 4.293 14 14 4.293l.707.707Zm0 3L8 14.707 7.293 14 14 7.293l.707.707Zm0 3L11 14.707 10.293 14 14 10.293l.707.707Z";

    protected override string IconGeometry20 { get; }
        = "M6.707 2 2 6.707 1.293 6 6 1.293 6.707 2Zm4 0L2 10.707 1.293 10 10 1.293l.707.707Zm4 0L2 14.707 1.293 14 14 1.293l.707.707Zm4 0L2 18.707 1.293 18 18 1.293l.707.707Zm0 4L6 18.707 5.293 18 18 5.293l.707.707Zm0 4L10 18.707 9.293 18 18 9.293l.707.707Zm0 4L14 18.707 13.293 18 18 13.293l.707.707Z"; 

    protected override string IconGeometry24 { get; }
        = "M5.707 3 3 5.707 2.293 5 5 2.293 5.707 3Zm4 0L3 9.707 2.293 9 9 2.293 9.707 3Zm4 0L3 13.707 2.293 13 13 2.293l.707.707Zm4 0L3 17.707 2.293 17 17 2.293l.707.707Zm4 0L3 21.707 2.293 21 21 2.293l.707.707Zm0 4L7 21.707 6.293 21 21 6.293l.707.707Zm0 4L11 21.707 10.293 21 21 10.293l.707.707Zm0 4L15 21.707 14.293 21 21 14.293l.707.707Zm0 4L19 21.707 18.293 21 21 18.293l.707.707Z"; 

    protected override string IconGeometry32 { get; }
        = "M9.707 3 3 9.707 2.293 9 9 2.293 9.707 3Zm5 0L3 14.707 2.293 14 14 2.293l.707.707Zm5 0L3 19.707 2.293 19 19 2.293l.707.707Zm5 0L3 24.707 2.293 24 24 2.293l.707.707Zm5 0L3 29.707 2.293 29 29 2.293l.707.707Zm0 5L8 29.707 7.293 29 29 7.293l.707.707Zm0 5L13 29.707 12.293 29 29 12.293l.707.707Zm0 5L18 29.707 17.293 29 29 17.293l.707.707Zm0 5L23 29.707 22.293 29 29 22.293l.707.707Z";
}