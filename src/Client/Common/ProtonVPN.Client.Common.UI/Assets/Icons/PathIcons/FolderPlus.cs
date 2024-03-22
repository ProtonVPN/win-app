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

public class FolderPlus : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M0 4.05a2 2 0 0 1 2-2h2.528c.388 0 .77.09 1.118.264L6.8 2.89a1.5 1.5 0 0 0 .671.159H13a2 2 0 0 1 2 2v7a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2v-8Zm2-1a1 1 0 0 0-1 1v8a1 1 0 0 0 1 1h11a1 1 0 0 0 1-1v-7a1 1 0 0 0-1-1H7.472a2.5 2.5 0 0 1-1.118-.264L5.2 3.208a1.5 1.5 0 0 0-.671-.158H2Z M7.5 6a.5.5 0 0 1 .5.5V8h1.5a.5.5 0 0 1 0 1H8v1.5a.5.5 0 0 1-1 0V9H5.5a.5.5 0 1 1 0-1H7V6.5a.5.5 0 0 1 .5-.5Z";

    protected override string IconGeometry20 { get; }
        = "M17.5 15V6.875c0-.69-.56-1.25-1.25-1.25h-5.542a1.5 1.5 0 0 1-.9-.3L8.042 4a1.25 1.25 0 0 0-.75-.25H3.75c-.69 0-1.25.56-1.25 1.25v10c0 .69.56 1.25 1.25 1.25h12.5c.69 0 1.25-.56 1.25-1.25ZM3.75 2.5A2.5 2.5 0 0 0 1.25 5v10a2.5 2.5 0 0 0 2.5 2.5h12.5a2.5 2.5 0 0 0 2.5-2.5V6.875a2.5 2.5 0 0 0-2.5-2.5h-5.542a.25.25 0 0 1-.15-.05L8.792 3a2.5 2.5 0 0 0-1.5-.5H3.75Zm6.25 5c.345 0 .625.28.625.625V10H12.5a.625.625 0 1 1 0 1.25h-1.875v1.875a.625.625 0 1 1-1.25 0V11.25H7.5a.625.625 0 1 1 0-1.25h1.875V8.125c0-.345.28-.625.625-.625Z"; 

    protected override string IconGeometry24 { get; }
        = "M21 18V8.25a1.5 1.5 0 0 0-1.5-1.5h-6.65a1.8 1.8 0 0 1-1.08-.36L9.65 4.8a1.5 1.5 0 0 0-.9-.3H4.5A1.5 1.5 0 0 0 3 6v12a1.5 1.5 0 0 0 1.5 1.5h15A1.5 1.5 0 0 0 21 18ZM4.5 3a3 3 0 0 0-3 3v12a3 3 0 0 0 3 3h15a3 3 0 0 0 3-3V8.25a3 3 0 0 0-3-3h-6.65a.3.3 0 0 1-.18-.06L10.55 3.6a3 3 0 0 0-1.8-.6H4.5ZM12 9a.75.75 0 0 1 .75.75V12H15a.75.75 0 0 1 0 1.5h-2.25v2.25a.75.75 0 0 1-1.5 0V13.5H9A.75.75 0 0 1 9 12h2.25V9.75A.75.75 0 0 1 12 9Z"; 

    protected override string IconGeometry32 { get; }
        = "M28 24V11a2 2 0 0 0-2-2h-8.867a2.4 2.4 0 0 1-1.44-.48L12.867 6.4a2 2 0 0 0-1.2-.4H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h20a2 2 0 0 0 2-2ZM6 4a4 4 0 0 0-4 4v16a4 4 0 0 0 4 4h20a4 4 0 0 0 4-4V11a4 4 0 0 0-4-4h-8.867a.4.4 0 0 1-.24-.08L14.067 4.8a4 4 0 0 0-2.4-.8H6Zm10 8a1 1 0 0 1 1 1v3h3a1 1 0 1 1 0 2h-3v3a1 1 0 1 1-2 0v-3h-3a1 1 0 1 1 0-2h3v-3a1 1 0 0 1 1-1Z";
}