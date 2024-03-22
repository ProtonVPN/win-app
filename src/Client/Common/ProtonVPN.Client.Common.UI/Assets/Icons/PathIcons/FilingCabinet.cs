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

public class FilingCabinet : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M4 2h8a1 1 0 0 1 1 1v4.5H3V3a1 1 0 0 1 1-1ZM2 8V3a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v10a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V8Zm11 .5V13a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V8.5h10ZM11 5a1 1 0 0 1-1 1H6a1 1 0 0 1-1-1v-.5a.5.5 0 0 1 1 0V5h4v-.5a.5.5 0 0 1 1 0V5Zm-1 7a1 1 0 0 0 1-1v-.5a.5.5 0 0 0-1 0v.5H6v-.5a.5.5 0 0 0-1 0v.5a1 1 0 0 0 1 1h4Z";

    protected override string IconGeometry20 { get; }
        = "M5 2.5h10c.69 0 1.25.56 1.25 1.25v5.625H3.75V3.75c0-.69.56-1.25 1.25-1.25ZM2.5 10V3.75A2.5 2.5 0 0 1 5 1.25h10a2.5 2.5 0 0 1 2.5 2.5v12.5a2.5 2.5 0 0 1-2.5 2.5H5a2.5 2.5 0 0 1-2.5-2.5V10Zm13.75.625v5.625c0 .69-.56 1.25-1.25 1.25H5c-.69 0-1.25-.56-1.25-1.25v-5.625h12.5Zm-2.5-4.375c0 .69-.56 1.25-1.25 1.25h-5c-.69 0-1.25-.56-1.25-1.25v-.625a.625.625 0 1 1 1.25 0v.625h5v-.625a.625.625 0 1 1 1.25 0v.625ZM12.5 15c.69 0 1.25-.56 1.25-1.25v-.625a.625.625 0 1 0-1.25 0v.625h-5v-.625a.625.625 0 1 0-1.25 0v.625c0 .69.56 1.25 1.25 1.25h5Z"; 

    protected override string IconGeometry24 { get; }
        = "M6 3h12a1.5 1.5 0 0 1 1.5 1.5v6.75h-15V4.5A1.5 1.5 0 0 1 6 3Zm-3 9V4.5a3 3 0 0 1 3-3h12a3 3 0 0 1 3 3v15a3 3 0 0 1-3 3H6a3 3 0 0 1-3-3V12Zm16.5.75v6.75A1.5 1.5 0 0 1 18 21H6a1.5 1.5 0 0 1-1.5-1.5v-6.75h15Zm-3-5.25A1.5 1.5 0 0 1 15 9H9a1.5 1.5 0 0 1-1.5-1.5v-.75a.75.75 0 0 1 1.5 0v.75h6v-.75a.75.75 0 0 1 1.5 0v.75ZM15 18a1.5 1.5 0 0 0 1.5-1.5v-.75a.75.75 0 0 0-1.5 0v.75H9v-.75a.75.75 0 0 0-1.5 0v.75A1.5 1.5 0 0 0 9 18h6Z"; 

    protected override string IconGeometry32 { get; }
        = "M8 4h16a2 2 0 0 1 2 2v9H6V6a2 2 0 0 1 2-2ZM4 16V6a4 4 0 0 1 4-4h16a4 4 0 0 1 4 4v20a4 4 0 0 1-4 4H8a4 4 0 0 1-4-4V16Zm22 1v9a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2v-9h20Zm-4-7a2 2 0 0 1-2 2h-8a2 2 0 0 1-2-2V9a1 1 0 1 1 2 0v1h8V9a1 1 0 1 1 2 0v1Zm-2 14a2 2 0 0 0 2-2v-1a1 1 0 1 0-2 0v1h-8v-1a1 1 0 1 0-2 0v1a2 2 0 0 0 2 2h8Z";
}