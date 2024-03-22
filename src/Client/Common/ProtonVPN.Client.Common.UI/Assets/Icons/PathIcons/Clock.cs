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

public class Clock : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M7.5 13a5.5 5.5 0 1 0 0-11 5.5 5.5 0 0 0 0 11Zm0 1a6.5 6.5 0 1 0 0-13 6.5 6.5 0 0 0 0 13Z M9.854 9.854a.5.5 0 0 1-.708 0l-2-2A.5.5 0 0 1 7 7.5V4a.5.5 0 0 1 1 0v3.293l1.854 1.853a.5.5 0 0 1 0 .708Z";

    protected override string IconGeometry20 { get; }
        = "M17.5 10a7.5 7.5 0 1 1-15 0 7.5 7.5 0 0 1 15 0Zm1.25 0a8.75 8.75 0 1 1-17.5 0 8.75 8.75 0 0 1 17.5 0Zm-6.067 3.567a.625.625 0 1 0 .884-.884l-2.942-2.942V5a.625.625 0 1 0-1.25 0v5c0 .166.066.325.183.442l3.125 3.125Z"; 

    protected override string IconGeometry24 { get; }
        = "M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Zm1.5 0c0 5.799-4.701 10.5-10.5 10.5S1.5 17.799 1.5 12 6.201 1.5 12 1.5 22.5 6.201 22.5 12Zm-7.28 4.28a.75.75 0 1 0 1.06-1.06l-3.53-3.53V6a.75.75 0 0 0-1.5 0v6c0 .199.079.39.22.53l3.75 3.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M28 16c0 6.627-5.373 12-12 12S4 22.627 4 16 9.373 4 16 4s12 5.373 12 12Zm2 0c0 7.732-6.268 14-14 14S2 23.732 2 16 8.268 2 16 2s14 6.268 14 14Zm-9.707 5.707a1 1 0 0 0 1.414-1.414L17 15.586V8a1 1 0 1 0-2 0v8a1 1 0 0 0 .293.707l5 5Z";
}