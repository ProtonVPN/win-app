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

public class Folder : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M14 12V5.5a1 1 0 0 0-1-1H8.567a1.2 1.2 0 0 1-.72-.24L6.433 3.2a1 1 0 0 0-.6-.2H3a1 1 0 0 0-1 1v8a1 1 0 0 0 1 1h10a1 1 0 0 0 1-1ZM3 2a2 2 0 0 0-2 2v8a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V5.5a2 2 0 0 0-2-2H8.567a.2.2 0 0 1-.12-.04L7.033 2.4a2 2 0 0 0-1.2-.4H3Z";

    protected override string IconGeometry20 { get; }
        = "M17.5 15V6.875c0-.69-.56-1.25-1.25-1.25h-5.542a1.5 1.5 0 0 1-.9-.3L8.042 4a1.25 1.25 0 0 0-.75-.25H3.75c-.69 0-1.25.56-1.25 1.25v10c0 .69.56 1.25 1.25 1.25h12.5c.69 0 1.25-.56 1.25-1.25ZM3.75 2.5A2.5 2.5 0 0 0 1.25 5v10a2.5 2.5 0 0 0 2.5 2.5h12.5a2.5 2.5 0 0 0 2.5-2.5V6.875a2.5 2.5 0 0 0-2.5-2.5h-5.542a.25.25 0 0 1-.15-.05L8.792 3a2.5 2.5 0 0 0-1.5-.5H3.75Z"; 

    protected override string IconGeometry24 { get; }
        = "M21 18V8.25a1.5 1.5 0 0 0-1.5-1.5h-6.65a1.8 1.8 0 0 1-1.08-.36L9.65 4.8a1.5 1.5 0 0 0-.9-.3H4.5A1.5 1.5 0 0 0 3 6v12a1.5 1.5 0 0 0 1.5 1.5h15A1.5 1.5 0 0 0 21 18ZM4.5 3a3 3 0 0 0-3 3v12a3 3 0 0 0 3 3h15a3 3 0 0 0 3-3V8.25a3 3 0 0 0-3-3h-6.65a.3.3 0 0 1-.18-.06L10.55 3.6a3 3 0 0 0-1.8-.6H4.5Z"; 

    protected override string IconGeometry32 { get; }
        = "M28 24V11a2 2 0 0 0-2-2h-8.867a2.4 2.4 0 0 1-1.44-.48L12.867 6.4a2 2 0 0 0-1.2-.4H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h20a2 2 0 0 0 2-2ZM6 4a4 4 0 0 0-4 4v16a4 4 0 0 0 4 4h20a4 4 0 0 0 4-4V11a4 4 0 0 0-4-4h-8.867a.4.4 0 0 1-.24-.08L14.067 4.8a4 4 0 0 0-2.4-.8H6Z";
}