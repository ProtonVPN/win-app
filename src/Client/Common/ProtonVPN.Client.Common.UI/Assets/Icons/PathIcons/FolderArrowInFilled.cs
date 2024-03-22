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

public class FolderArrowInFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M1 4a2 2 0 0 1 2-2h2.528c.388 0 .77.09 1.118.264l1.155.578A1.5 1.5 0 0 0 8.472 3H13a2 2 0 0 1 2 2v7a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V9h6.793l-1.147 1.146a.5.5 0 0 0 .708.708l1.858-1.859a.7.7 0 0 0 0-.99L7.354 6.146a.5.5 0 1 0-.708.708L7.793 8H1V4Z";

    protected override string IconGeometry20 { get; }
        = "M1.25 5a2.5 2.5 0 0 1 2.5-2.5h3.542a2.5 2.5 0 0 1 1.5.5l1.766 1.325a.25.25 0 0 0 .15.05h5.542a2.5 2.5 0 0 1 2.5 2.5V15a2.5 2.5 0 0 1-2.5 2.5H3.75a2.5 2.5 0 0 1-2.5-2.5v-3.75h7.866l-1.433 1.433a.625.625 0 1 0 .884.884l2.323-2.323a.875.875 0 0 0 0-1.238L8.567 7.683a.625.625 0 1 0-.884.884L9.116 10H1.25V5Z"; 

    protected override string IconGeometry24 { get; }
        = "M1.5 6a3 3 0 0 1 3-3h4.25a3 3 0 0 1 1.8.6l2.12 1.59a.3.3 0 0 0 .18.06h6.65a3 3 0 0 1 3 3V18a3 3 0 0 1-3 3h-15a3 3 0 0 1-3-3v-4.5h9.44l-1.72 1.72a.75.75 0 1 0 1.06 1.06l2.788-2.788a1.05 1.05 0 0 0 0-1.485L10.28 9.22a.75.75 0 1 0-1.06 1.06L10.94 12H1.5V6Z"; 

    protected override string IconGeometry32 { get; }
        = "M2 8a4 4 0 0 1 4-4h5.667a4 4 0 0 1 2.4.8l2.826 2.12a.4.4 0 0 0 .24.08H26a4 4 0 0 1 4 4v13a4 4 0 0 1-4 4H6a4 4 0 0 1-4-4v-6h12.586l-2.293 2.293a1 1 0 0 0 1.414 1.414l3.717-3.717a1.4 1.4 0 0 0 0-1.98l-3.717-3.717a1 1 0 0 0-1.414 1.414L14.586 16H2V8Z";
}