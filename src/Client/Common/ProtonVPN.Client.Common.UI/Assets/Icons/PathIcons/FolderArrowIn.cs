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

public class FolderArrowIn : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7.793 9H1V8h6.793L6.646 6.854a.5.5 0 1 1 .708-.708l1.858 1.859a.7.7 0 0 1 0 .99l-1.858 1.859a.5.5 0 0 1-.708-.708L7.793 9Z M3 2a2 2 0 0 0-2 2v3h1V4a1 1 0 0 1 1-1h2.528a1.5 1.5 0 0 1 .67.158l1.156.578A2.5 2.5 0 0 0 8.472 4H13a1 1 0 0 1 1 1v7a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1v-2H1v2a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V5a2 2 0 0 0-2-2H8.472a1.5 1.5 0 0 1-.67-.158l-1.156-.578A2.5 2.5 0 0 0 5.528 2H3Z";

    protected override string IconGeometry20 { get; }
        = "M1.25 5a2.5 2.5 0 0 1 2.5-2.5H7.5A2.5 2.5 0 0 1 9 3l1.833 1.375h5.417a2.5 2.5 0 0 1 2.5 2.5V15a2.5 2.5 0 0 1-2.5 2.5H3.75a2.5 2.5 0 0 1-2.5-2.5v-2.5H2.5V15c0 .69.56 1.25 1.25 1.25h12.5c.69 0 1.25-.56 1.25-1.25V6.875c0-.69-.56-1.25-1.25-1.25h-5.417a1.25 1.25 0 0 1-.75-.25L8.25 4a1.25 1.25 0 0 0-.75-.25H3.75c-.69 0-1.25.56-1.25 1.25v3.75H1.25V5Zm8.491 6.25H1.25V10h8.491L8.308 8.567a.625.625 0 1 1 .884-.884l2.323 2.323a.875.875 0 0 1 0 1.238l-2.323 2.323a.625.625 0 1 1-.884-.884l1.433-1.433Z"; 

    protected override string IconGeometry24 { get; }
        = "M1.5 6a3 3 0 0 1 3-3H9a3 3 0 0 1 1.8.6L13 5.25h6.5a3 3 0 0 1 3 3V18a3 3 0 0 1-3 3h-15a3 3 0 0 1-3-3v-3H3v3a1.5 1.5 0 0 0 1.5 1.5h15A1.5 1.5 0 0 0 21 18V8.25a1.5 1.5 0 0 0-1.5-1.5H13a1.5 1.5 0 0 1-.9-.3L9.9 4.8a1.5 1.5 0 0 0-.9-.3H4.5A1.5 1.5 0 0 0 3 6v4.5H1.5V6Zm10.19 7.5H1.5V12h10.19l-1.72-1.72a.75.75 0 1 1 1.06-1.06l2.788 2.787c.41.41.41 1.075 0 1.486L11.03 16.28a.75.75 0 1 1-1.06-1.06l1.72-1.72Z"; 

    protected override string IconGeometry32 { get; }
        = "M2 8a4 4 0 0 1 4-4h6a4 4 0 0 1 2.4.8L17.333 7H26a4 4 0 0 1 4 4v13a4 4 0 0 1-4 4H6a4 4 0 0 1-4-4v-4h2v4a2 2 0 0 0 2 2h20a2 2 0 0 0 2-2V11a2 2 0 0 0-2-2h-8.667a2 2 0 0 1-1.2-.4L13.2 6.4A2 2 0 0 0 12 6H6a2 2 0 0 0-2 2v6H2V8Zm13.586 10H2v-2h13.586l-2.293-2.293a1 1 0 0 1 1.414-1.414l3.717 3.717a1.4 1.4 0 0 1 0 1.98l-3.717 3.717a1 1 0 0 1-1.414-1.414L15.586 18Z";
}