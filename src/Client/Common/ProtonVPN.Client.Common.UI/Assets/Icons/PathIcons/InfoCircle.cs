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

public class InfoCircle : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7.25 7.5a.5.5 0 0 1 .5-.5H9v4h.5a.5.5 0 0 1 0 1h-2a.5.5 0 0 1 0-1H8V8h-.25a.5.5 0 0 1-.5-.5ZM8.4 6.3a.9.9 0 1 0 0-1.8.9.9 0 0 0 0 1.8Z M1 8.5a7.5 7.5 0 1 1 15 0 7.5 7.5 0 0 1-15 0ZM8.5 2a6.5 6.5 0 1 0 0 13 6.5 6.5 0 0 0 0-13Z";

    protected override string IconGeometry20 { get; }
        = "M9.063 9.375c0-.345.28-.625.624-.625h1.563v5h.625a.625.625 0 1 1 0 1.25h-2.5a.625.625 0 1 1 0-1.25H10V10h-.313a.625.625 0 0 1-.624-.625Zm1.437-1.5a1.125 1.125 0 1 0 0-2.25 1.125 1.125 0 0 0 0 2.25Z M1.25 10.625a9.375 9.375 0 1 1 18.75 0 9.375 9.375 0 0 1-18.75 0ZM10.625 2.5a8.125 8.125 0 1 0 0 16.25 8.125 8.125 0 0 0 0-16.25Z"; 

    protected override string IconGeometry24 { get; }
        = "M10.875 11.25a.75.75 0 0 1 .75-.75H13.5v6h.75a.75.75 0 0 1 0 1.5h-3a.75.75 0 0 1 0-1.5H12V12h-.375a.75.75 0 0 1-.75-.75Zm1.725-1.8a1.35 1.35 0 1 0 0-2.7 1.35 1.35 0 0 0 0 2.7Z M1.5 12.75C1.5 6.537 6.537 1.5 12.75 1.5S24 6.537 24 12.75 18.963 24 12.75 24 1.5 18.963 1.5 12.75ZM12.75 3C7.365 3 3 7.365 3 12.75s4.365 9.75 9.75 9.75 9.75-4.365 9.75-9.75S18.135 3 12.75 3Z"; 

    protected override string IconGeometry32 { get; }
        = "M14.5 15a1 1 0 0 1 1-1H18v8h1a1 1 0 1 1 0 2h-4a1 1 0 1 1 0-2h1v-6h-.5a1 1 0 0 1-1-1Zm2.3-2.4a1.8 1.8 0 1 0 0-3.6 1.8 1.8 0 0 0 0 3.6Z M2 17C2 8.716 8.716 2 17 2c8.284 0 15 6.716 15 15 0 8.284-6.716 15-15 15-8.284 0-15-6.716-15-15ZM17 4C9.82 4 4 9.82 4 17s5.82 13 13 13 13-5.82 13-13S24.18 4 17 4Z";
}