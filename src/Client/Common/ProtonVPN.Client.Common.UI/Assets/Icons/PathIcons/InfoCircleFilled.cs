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

public class InfoCircleFilled : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8.5 16a7.5 7.5 0 1 0 0-15 7.5 7.5 0 0 0 0 15Zm.8-10.6a.9.9 0 1 1-1.8 0 .9.9 0 0 1 1.8 0ZM7.25 7.5a.5.5 0 0 1 .5-.5H9v4h.5a.5.5 0 0 1 0 1h-2a.5.5 0 0 1 0-1H8V8h-.25a.5.5 0 0 1-.5-.5Z";

    protected override string IconGeometry20 { get; }
        = "M10.625 20a9.375 9.375 0 1 0 0-18.75 9.375 9.375 0 1 0 0 18.75Zm1-13.25a1.125 1.125 0 1 1-2.25 0 1.125 1.125 0 0 1 2.25 0ZM9.062 9.375c0-.345.28-.625.626-.625h1.562v5h.625a.625.625 0 1 1 0 1.25h-2.5a.625.625 0 1 1 0-1.25H10V10h-.313a.625.625 0 0 1-.624-.625Z"; 

    protected override string IconGeometry24 { get; }
        = "M12.75 24C18.963 24 24 18.963 24 12.75S18.963 1.5 12.75 1.5 1.5 6.537 1.5 12.75 6.537 24 12.75 24Zm1.2-15.9a1.35 1.35 0 1 1-2.7 0 1.35 1.35 0 0 1 2.7 0Zm-3.075 3.15a.75.75 0 0 1 .75-.75H13.5v6h.75a.75.75 0 0 1 0 1.5h-3a.75.75 0 0 1 0-1.5H12V12h-.375a.75.75 0 0 1-.75-.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M17 32c8.284 0 15-6.716 15-15 0-8.284-6.716-15-15-15C8.716 2 2 8.716 2 17c0 8.284 6.716 15 15 15Zm1.6-21.2a1.8 1.8 0 1 1-3.6 0 1.8 1.8 0 0 1 3.6 0ZM14.5 15a1 1 0 0 1 1-1H18v8h1a1 1 0 1 1 0 2h-4a1 1 0 1 1 0-2h1v-6h-.5a1 1 0 0 1-1-1Z";
}