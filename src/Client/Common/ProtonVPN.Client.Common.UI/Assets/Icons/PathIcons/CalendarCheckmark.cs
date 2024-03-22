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

public class CalendarCheckmark : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M3 2.17V1.5a.5.5 0 0 1 1 0V2h8v-.5a.5.5 0 0 1 1 0v.67c1.165.413 2 1.524 2 2.83v7a3 3 0 0 1-3 3H4a3 3 0 0 1-3-3V5c0-1.306.835-2.417 2-2.83ZM12 3H4a2 2 0 0 0-2 2h12a2 2 0 0 0-2-2ZM2 12V6h12v6a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2Zm9.354-3.646a.5.5 0 0 0-.708-.708L7.5 10.793 5.854 9.146a.5.5 0 1 0-.708.708l1.859 1.858a.7.7 0 0 0 .99 0l3.359-3.358Z";

    protected override string IconGeometry20 { get; }
        = "M3.75 2.713v-.838a.625.625 0 1 1 1.25 0V2.5h10v-.625a.625.625 0 1 1 1.25 0v.838a3.752 3.752 0 0 1 2.5 3.537V15A3.75 3.75 0 0 1 15 18.75H5A3.75 3.75 0 0 1 1.25 15V6.25a3.752 3.752 0 0 1 2.5-3.537ZM15 3.75H5a2.5 2.5 0 0 0-2.5 2.5h15a2.5 2.5 0 0 0-2.5-2.5ZM2.5 15V7.5h15V15a2.5 2.5 0 0 1-2.5 2.5H5A2.5 2.5 0 0 1 2.5 15Zm11.692-4.558a.625.625 0 1 0-.884-.884l-3.933 3.933-2.058-2.058a.625.625 0 1 0-.884.884l2.323 2.323a.875.875 0 0 0 1.238 0l4.198-4.198Z"; 

    protected override string IconGeometry24 { get; }
        = "M4.5 3.256V2.25a.75.75 0 0 1 1.5 0V3h12v-.75a.75.75 0 0 1 1.5 0v1.006c1.748.618 3 2.285 3 4.244V18a4.5 4.5 0 0 1-4.5 4.5H6A4.5 4.5 0 0 1 1.5 18V7.5a4.502 4.502 0 0 1 3-4.244ZM18 4.5H6a3 3 0 0 0-3 3h18a3 3 0 0 0-3-3ZM3 18V9h18v9a3 3 0 0 1-3 3H6a3 3 0 0 1-3-3Zm14.03-5.47a.75.75 0 1 0-1.06-1.06l-4.72 4.72-2.47-2.47a.75.75 0 0 0-1.06 1.06l2.788 2.788c.41.41 1.074.41 1.485 0l5.037-5.038Z"; 

    protected override string IconGeometry32 { get; }
        = "M6 4.341V3a1 1 0 0 1 2 0v1h16V3a1 1 0 1 1 2 0v1.341c2.33.824 4 3.047 4 5.659v14a6 6 0 0 1-6 6H8a6 6 0 0 1-6-6V10a6.002 6.002 0 0 1 4-5.659ZM24 6H8a4 4 0 0 0-4 4h24a4 4 0 0 0-4-4ZM4 24V12h24v12a4 4 0 0 1-4 4H8a4 4 0 0 1-4-4Zm18.707-7.293a1 1 0 0 0-1.414-1.414L15 21.586l-3.293-3.293a1 1 0 0 0-1.414 1.414l3.717 3.717a1.4 1.4 0 0 0 1.98 0l6.717-6.717Z";
}