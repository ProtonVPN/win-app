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

public class FolderArrowUp : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M1 4a2 2 0 0 1 2-2h3.028c.388 0 .77.09 1.118.264l1.155.578A1.5 1.5 0 0 0 8.972 3H13a2 2 0 0 1 2 2v7a2 2 0 0 1-2 2h-3v-1h3a1 1 0 0 0 1-1V5a1 1 0 0 0-1-1H8.972a2.5 2.5 0 0 1-1.118-.264L6.7 3.158A1.5 1.5 0 0 0 6.028 3H3a1 1 0 0 0-1 1v8a1 1 0 0 0 1 1h4v1H3a2 2 0 0 1-2-2V4Z M8.854 6.646a.5.5 0 0 0-.708 0l-2 2a.5.5 0 1 0 .708.708L8 8.207V14h1V8.207l1.146 1.147a.5.5 0 0 0 .708-.708l-2-2Z";

    protected override string IconGeometry20 { get; }
        = "M3.75 2.5A2.5 2.5 0 0 0 1.25 5v10a2.5 2.5 0 0 0 2.5 2.5h4.375v-1.25H3.75c-.69 0-1.25-.56-1.25-1.25V5c0-.69.56-1.25 1.25-1.25H7.5c.27 0 .534.088.75.25l1.833 1.375c.217.162.48.25.75.25h5.417c.69 0 1.25.56 1.25 1.25V15c0 .69-.56 1.25-1.25 1.25h-4.375v1.25h4.375a2.5 2.5 0 0 0 2.5-2.5V6.875a2.5 2.5 0 0 0-2.5-2.5h-5.417L9 3a2.5 2.5 0 0 0-1.5-.5H3.75Zm3.308 9.192a.625.625 0 0 1 0-.884l2.323-2.323a.875.875 0 0 1 1.238 0l2.323 2.323a.625.625 0 1 1-.884.884l-1.433-1.433V17.5h-1.25v-7.241l-1.433 1.433a.625.625 0 0 1-.884 0Z"; 

    protected override string IconGeometry24 { get; }
        = "M4.5 3a3 3 0 0 0-3 3v12a3 3 0 0 0 3 3h5.25v-1.5H4.5A1.5 1.5 0 0 1 3 18V6a1.5 1.5 0 0 1 1.5-1.5H9c.325 0 .64.105.9.3l2.2 1.65c.26.195.575.3.9.3h6.5a1.5 1.5 0 0 1 1.5 1.5V18a1.5 1.5 0 0 1-1.5 1.5h-5.25V21h5.25a3 3 0 0 0 3-3V8.25a3 3 0 0 0-3-3H13L10.8 3.6A3 3 0 0 0 9 3H4.5Zm3.97 11.03a.75.75 0 0 1 0-1.06l2.787-2.788a1.05 1.05 0 0 1 1.486 0l2.787 2.788a.75.75 0 1 1-1.06 1.06l-1.72-1.72V21h-1.5v-8.69l-1.72 1.72a.75.75 0 0 1-1.06 0Z"; 

    protected override string IconGeometry32 { get; }
        = "M6 4a4 4 0 0 0-4 4v16a4 4 0 0 0 4 4h7v-2H6a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6a2 2 0 0 1 1.2.4l2.933 2.2a2 2 0 0 0 1.2.4H26a2 2 0 0 1 2 2v13a2 2 0 0 1-2 2h-7v2h7a4 4 0 0 0 4-4V11a4 4 0 0 0-4-4h-8.667L14.4 4.8A4 4 0 0 0 12 4H6Zm5.293 14.707a1 1 0 0 1 0-1.414l3.717-3.717a1.4 1.4 0 0 1 1.98 0l3.717 3.717a1 1 0 0 1-1.414 1.414L17 16.414V28h-2V16.414l-2.293 2.293a1 1 0 0 1-1.414 0Z";
}