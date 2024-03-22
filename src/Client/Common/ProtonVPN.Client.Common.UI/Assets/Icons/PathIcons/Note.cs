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

public class Note : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M3 12a1 1 0 0 0 1 1h5v-2.5A1.5 1.5 0 0 1 10.5 9H13V4a1 1 0 0 0-1-1H4a1 1 0 0 0-1 1v8Zm9.75-2H10.5a.5.5 0 0 0-.5.5v2.25a1.5 1.5 0 0 0 .232-.19l2.329-2.328a1.5 1.5 0 0 0 .19-.232ZM4 14a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v5.172a2.5 2.5 0 0 1-.732 1.767l-2.329 2.329A2.5 2.5 0 0 1 9.172 14H4Z";

    protected override string IconGeometry20 { get; }
        = "M3.75 15c0 .69.56 1.25 1.25 1.25h6.25v-3.125c0-1.036.84-1.875 1.875-1.875h3.125V5c0-.69-.56-1.25-1.25-1.25H5c-.69 0-1.25.56-1.25 1.25v10Zm12.188-2.5h-2.813a.625.625 0 0 0-.625.625v2.813c.104-.069.201-.148.29-.237l2.91-2.91c.09-.09.17-.187.238-.291ZM5 17.5A2.5 2.5 0 0 1 2.5 15V5A2.5 2.5 0 0 1 5 2.5h10A2.5 2.5 0 0 1 17.5 5v6.464c0 .83-.33 1.624-.915 2.21l-2.91 2.91a3.125 3.125 0 0 1-2.21.916H5Z"; 

    protected override string IconGeometry24 { get; }
        = "M4.5 18A1.5 1.5 0 0 0 6 19.5h7.5v-3.75a2.25 2.25 0 0 1 2.25-2.25h3.75V6A1.5 1.5 0 0 0 18 4.5H6A1.5 1.5 0 0 0 4.5 6v12Zm14.626-3H15.75a.75.75 0 0 0-.75.75v3.376c.124-.083.241-.178.348-.285l3.493-3.493c.107-.107.202-.224.285-.348ZM6 21a3 3 0 0 1-3-3V6a3 3 0 0 1 3-3h12a3 3 0 0 1 3 3v7.757a3.75 3.75 0 0 1-1.098 2.652l-3.493 3.493A3.75 3.75 0 0 1 13.757 21H6Z"; 

    protected override string IconGeometry32 { get; }
        = "M6 24a2 2 0 0 0 2 2h10v-5a3 3 0 0 1 3-3h5V8a2 2 0 0 0-2-2H8a2 2 0 0 0-2 2v16Zm19.501-4H21a1 1 0 0 0-1 1v4.501c.166-.11.322-.237.465-.38l4.656-4.656c.143-.143.27-.3.38-.465ZM8 28a4 4 0 0 1-4-4V8a4 4 0 0 1 4-4h16a4 4 0 0 1 4 4v10.343a5 5 0 0 1-1.465 3.536l-4.656 4.656A5 5 0 0 1 18.343 28H8Z";
}