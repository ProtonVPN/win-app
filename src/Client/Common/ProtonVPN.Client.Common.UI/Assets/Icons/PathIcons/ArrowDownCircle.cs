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

public class ArrowDownCircle : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M8 10.58 6.354 8.935a.5.5 0 1 0-.708.707L8.005 12a.7.7 0 0 0 .99 0l2.359-2.359a.5.5 0 0 0-.708-.707L9 10.581V4.5a.5.5 0 0 0-1 0v6.08Z M8.5 15a6.5 6.5 0 1 0 0-13 6.5 6.5 0 0 0 0 13Zm0 1a7.5 7.5 0 1 0 0-15 7.5 7.5 0 0 0 0 15Z";

    protected override string IconGeometry20 { get; }
        = "m10 13.226-2.058-2.058a.625.625 0 1 0-.884.884L10.006 15a.875.875 0 0 0 1.238 0l2.948-2.948a.625.625 0 0 0-.884-.884l-2.058 2.058V5.625a.625.625 0 1 0-1.25 0v7.601Z M10.625 18.75a8.125 8.125 0 1 0 0-16.25 8.125 8.125 0 0 0 0 16.25Zm0 1.25a9.375 9.375 0 1 0 0-18.75 9.375 9.375 0 1 0 0 18.75Z"; 

    protected override string IconGeometry24 { get; }
        = "m12 15.871-2.47-2.47a.75.75 0 0 0-1.06 1.061L12.007 18c.41.41 1.075.41 1.486 0l3.537-3.538a.75.75 0 1 0-1.06-1.06l-2.47 2.47V6.75a.75.75 0 0 0-1.5 0v9.121Z M12.75 22.5c5.385 0 9.75-4.365 9.75-9.75S18.135 3 12.75 3 3 7.365 3 12.75s4.365 9.75 9.75 9.75Zm0 1.5C18.963 24 24 18.963 24 12.75S18.963 1.5 12.75 1.5 1.5 6.537 1.5 12.75 6.537 24 12.75 24Z"; 

    protected override string IconGeometry32 { get; }
        = "m16 21.162-3.293-3.293a1 1 0 0 0-1.414 1.414L16.01 24a1.4 1.4 0 0 0 1.98 0l4.717-4.717a1 1 0 0 0-1.414-1.414L18 21.162V9a1 1 0 1 0-2 0v12.162Z M17 30c7.18 0 13-5.82 13-13S24.18 4 17 4 4 9.82 4 17s5.82 13 13 13Zm0 2c8.284 0 15-6.716 15-15 0-8.284-6.716-15-15-15C8.716 2 2 8.716 2 17c0 8.284 6.716 15 15 15Z";
}