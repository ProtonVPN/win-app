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

public class Pause : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M5 4h2v8H5V4ZM4 4a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v8a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1V4Zm6 0h2v8h-2V4ZM9 4a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v8a1 1 0 0 1-1 1h-2a1 1 0 0 1-1-1V4Z";

    protected override string IconGeometry20 { get; }
        = "M5.625 5h2.5v10h-2.5V5Zm-1.25 0c0-.69.56-1.25 1.25-1.25h2.5c.69 0 1.25.56 1.25 1.25v10c0 .69-.56 1.25-1.25 1.25h-2.5c-.69 0-1.25-.56-1.25-1.25V5Zm7.5 0h2.5v10h-2.5V5Zm-1.25 0c0-.69.56-1.25 1.25-1.25h2.5c.69 0 1.25.56 1.25 1.25v10c0 .69-.56 1.25-1.25 1.25h-2.5c-.69 0-1.25-.56-1.25-1.25V5Z"; 

    protected override string IconGeometry24 { get; }
        = "M6.75 6h3v12h-3V6Zm-1.5 0a1.5 1.5 0 0 1 1.5-1.5h3a1.5 1.5 0 0 1 1.5 1.5v12a1.5 1.5 0 0 1-1.5 1.5h-3a1.5 1.5 0 0 1-1.5-1.5V6Zm9 0h3v12h-3V6Zm-1.5 0a1.5 1.5 0 0 1 1.5-1.5h3a1.5 1.5 0 0 1 1.5 1.5v12a1.5 1.5 0 0 1-1.5 1.5h-3a1.5 1.5 0 0 1-1.5-1.5V6Z"; 

    protected override string IconGeometry32 { get; }
        = "M9 8h4v16H9V8ZM7 8a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v16a2 2 0 0 1-2 2H9a2 2 0 0 1-2-2V8Zm12 0h4v16h-4V8Zm-2 0a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v16a2 2 0 0 1-2 2h-4a2 2 0 0 1-2-2V8Z";
}