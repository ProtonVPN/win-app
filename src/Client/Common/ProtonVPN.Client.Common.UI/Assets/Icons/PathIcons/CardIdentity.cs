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

public class CardIdentity : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M13 4H3a1 1 0 0 0-1 1v6a1 1 0 0 0 1 1h10a1 1 0 0 0 1-1V5a1 1 0 0 0-1-1ZM3 3a2 2 0 0 0-2 2v6a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V5a2 2 0 0 0-2-2H3Zm2.5 4a.5.5 0 1 0 0-1 .5.5 0 0 0 0 1Zm0 1a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3ZM4 10a.5.5 0 0 1 .5-.5h2a.5.5 0 0 1 .5.5v.5a.5.5 0 0 0 1 0V10a1.5 1.5 0 0 0-1.5-1.5h-2A1.5 1.5 0 0 0 3 10v.5a.5.5 0 0 0 1 0V10Zm5-3a.5.5 0 0 1 .5-.5H12a.5.5 0 0 1 0 1H9.5A.5.5 0 0 1 9 7Zm.5 1.5a.5.5 0 0 0 0 1H12a.5.5 0 0 0 0-1H9.5Z";

    protected override string IconGeometry20 { get; }
        = "M16.25 5H3.75c-.69 0-1.25.56-1.25 1.25v7.5c0 .69.56 1.25 1.25 1.25h12.5c.69 0 1.25-.56 1.25-1.25v-7.5c0-.69-.56-1.25-1.25-1.25ZM3.75 3.75a2.5 2.5 0 0 0-2.5 2.5v7.5a2.5 2.5 0 0 0 2.5 2.5h12.5a2.5 2.5 0 0 0 2.5-2.5v-7.5a2.5 2.5 0 0 0-2.5-2.5H3.75Zm3.125 5a.625.625 0 1 0 0-1.25.625.625 0 0 0 0 1.25Zm0 1.25a1.875 1.875 0 1 0 0-3.75 1.875 1.875 0 0 0 0 3.75ZM5 12.5c0-.345.28-.625.625-.625h2.5c.345 0 .625.28.625.625v.625a.625.625 0 1 0 1.25 0V12.5c0-1.036-.84-1.875-1.875-1.875h-2.5c-1.036 0-1.875.84-1.875 1.875v.625a.625.625 0 1 0 1.25 0V12.5Zm6.25-3.75c0-.345.28-.625.625-.625H15a.625.625 0 1 1 0 1.25h-3.125a.625.625 0 0 1-.625-.625Zm.625 1.875a.625.625 0 1 0 0 1.25H15a.625.625 0 1 0 0-1.25h-3.125Z"; 

    protected override string IconGeometry24 { get; }
        = "M19.5 6h-15A1.5 1.5 0 0 0 3 7.5v9A1.5 1.5 0 0 0 4.5 18h15a1.5 1.5 0 0 0 1.5-1.5v-9A1.5 1.5 0 0 0 19.5 6Zm-15-1.5a3 3 0 0 0-3 3v9a3 3 0 0 0 3 3h15a3 3 0 0 0 3-3v-9a3 3 0 0 0-3-3h-15Zm3.75 6a.75.75 0 1 0 0-1.5.75.75 0 0 0 0 1.5Zm0 1.5a2.25 2.25 0 1 0 0-4.5 2.25 2.25 0 0 0 0 4.5ZM6 15a.75.75 0 0 1 .75-.75h3a.75.75 0 0 1 .75.75v.75a.75.75 0 0 0 1.5 0V15a2.25 2.25 0 0 0-2.25-2.25h-3A2.25 2.25 0 0 0 4.5 15v.75a.75.75 0 0 0 1.5 0V15Zm7.5-4.5a.75.75 0 0 1 .75-.75H18a.75.75 0 0 1 0 1.5h-3.75a.75.75 0 0 1-.75-.75Zm.75 2.25a.75.75 0 0 0 0 1.5H18a.75.75 0 0 0 0-1.5h-3.75Z"; 

    protected override string IconGeometry32 { get; }
        = "M26 8H6a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h20a2 2 0 0 0 2-2V10a2 2 0 0 0-2-2ZM6 6a4 4 0 0 0-4 4v12a4 4 0 0 0 4 4h20a4 4 0 0 0 4-4V10a4 4 0 0 0-4-4H6Zm5 8a1 1 0 1 0 0-2 1 1 0 0 0 0 2Zm0 2a3 3 0 1 0 0-6 3 3 0 0 0 0 6Zm-3 4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v1a1 1 0 1 0 2 0v-1a3 3 0 0 0-3-3H9a3 3 0 0 0-3 3v1a1 1 0 1 0 2 0v-1Zm10-6a1 1 0 0 1 1-1h5a1 1 0 1 1 0 2h-5a1 1 0 0 1-1-1Zm1 3a1 1 0 1 0 0 2h5a1 1 0 1 0 0-2h-5Z";
}