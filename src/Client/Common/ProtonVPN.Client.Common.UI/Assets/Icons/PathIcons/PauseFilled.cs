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

public class PauseFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M4 4a1 1 0 0 1 1-1h1a1 1 0 0 1 1 1v8c0 .5-.5 1-1 1H5c-.5 0-1-.448-1-1V4Zm5 0c0-.5.5-1 1-1h1c.5 0 1 .5 1 1v8c0 .5-.5 1-1 1h-1c-.5 0-1-.5-1-1V4Z";

    protected override string IconGeometry20 { get; }
        = "M5 4.857C5 4.384 5.448 4 6 4h1c.552 0 1 .384 1 .857v10.286C8 15.57 7.5 16 7 16H6c-.5 0-1-.384-1-.857V4.857Zm7 0C12 4.43 12.5 4 13 4h1c.5 0 1 .429 1 .857v10.286c0 .428-.5.857-1 .857h-1c-.5 0-1-.429-1-.857V4.857Z"; 

    protected override string IconGeometry24 { get; }
        = "M6 6.5C6 5.727 6.672 5 7.5 5h1c.828 0 1.5.727 1.5 1.5v11c0 .773-.672 1.5-1.5 1.5h-1c-.828 0-1.5-.727-1.5-1.5v-11Zm8 0c0-.773.672-1.5 1.5-1.5h1c.828 0 1.5.727 1.5 1.5v11c0 .773-.672 1.5-1.5 1.5h-1c-.828 0-1.5-.727-1.5-1.5v-11Z"; 

    protected override string IconGeometry32 { get; }
        = "M8 8a2 2 0 0 1 2-2h2a2 2 0 0 1 2 2v16a2 2 0 0 1-2 2h-2a2 2 0 0 1-2-2V8Zm10 0a2 2 0 0 1 2-2h2a2 2 0 0 1 2 2v16a2 2 0 0 1-2 2h-2a2 2 0 0 1-2-2V8Z";
}