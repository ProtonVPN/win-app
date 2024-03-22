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

public class FolderFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M3 2a2 2 0 0 0-2 2v8a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V5a2 2 0 0 0-2-2H8.472a1.5 1.5 0 0 1-.67-.158l-1.156-.578A2.5 2.5 0 0 0 5.528 2H3Z";

    protected override string IconGeometry20 { get; }
        = "M1.25 5a2.5 2.5 0 0 1 2.5-2.5h3.542a2.5 2.5 0 0 1 1.5.5l1.766 1.325a.25.25 0 0 0 .15.05h5.542a2.5 2.5 0 0 1 2.5 2.5V15a2.5 2.5 0 0 1-2.5 2.5H3.75a2.5 2.5 0 0 1-2.5-2.5V5Z"; 

    protected override string IconGeometry24 { get; }
        = "M1.5 6a3 3 0 0 1 3-3h4.25a3 3 0 0 1 1.8.6l2.12 1.59a.3.3 0 0 0 .18.06h6.65a3 3 0 0 1 3 3V18a3 3 0 0 1-3 3h-15a3 3 0 0 1-3-3V6Z"; 

    protected override string IconGeometry32 { get; }
        = "M2 8a4 4 0 0 1 4-4h5.667a4 4 0 0 1 2.4.8l2.826 2.12a.4.4 0 0 0 .24.08H26a4 4 0 0 1 4 4v13a4 4 0 0 1-4 4H6a4 4 0 0 1-4-4V8Z";
}