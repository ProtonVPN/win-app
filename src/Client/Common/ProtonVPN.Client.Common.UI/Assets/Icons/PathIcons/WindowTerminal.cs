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

public class WindowTerminal : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M3 3h10a1 1 0 0 1 1 1H2a1 1 0 0 1 1-1ZM1 5V4a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V5Zm1 0h12v7a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V5Zm6.5 6H12a.5.5 0 0 0 0-1H8.5a.5.5 0 0 0 0 1ZM4.146 6.646a.5.5 0 0 1 .708 0l1.858 1.859a.7.7 0 0 1 0 .99l-1.858 1.859a.5.5 0 0 1-.708-.708L5.793 9 4.146 7.354a.5.5 0 0 1 0-.708Z";

    protected override string IconGeometry20 { get; }
        = "M3.75 3.75h12.5c.69 0 1.25.56 1.25 1.25v.625h-15V5c0-.69.56-1.25 1.25-1.25Zm-2.5 3.125V5a2.5 2.5 0 0 1 2.5-2.5h12.5a2.5 2.5 0 0 1 2.5 2.5v10a2.5 2.5 0 0 1-2.5 2.5H3.75a2.5 2.5 0 0 1-2.5-2.5V6.875Zm1.25 0h15V15c0 .69-.56 1.25-1.25 1.25H3.75c-.69 0-1.25-.56-1.25-1.25V6.875Zm8.125 7.5H15a.625.625 0 1 0 0-1.25h-4.375a.625.625 0 1 0 0 1.25ZM5.183 8.308a.625.625 0 0 1 .884 0l2.323 2.323a.875.875 0 0 1 0 1.238l-2.323 2.323a.625.625 0 1 1-.884-.884l2.058-2.058-2.058-2.058a.625.625 0 0 1 0-.884Z"; 

    protected override string IconGeometry24 { get; }
        = "M4.5 4.5h15A1.5 1.5 0 0 1 21 6v.75H3V6a1.5 1.5 0 0 1 1.5-1.5Zm-3 3.75V6a3 3 0 0 1 3-3h15a3 3 0 0 1 3 3v12a3 3 0 0 1-3 3h-15a3 3 0 0 1-3-3V8.25Zm1.5 0h18V18a1.5 1.5 0 0 1-1.5 1.5h-15A1.5 1.5 0 0 1 3 18V8.25Zm9.75 9H18a.75.75 0 0 0 0-1.5h-5.25a.75.75 0 0 0 0 1.5ZM6.22 9.97a.75.75 0 0 1 1.06 0l2.788 2.787c.41.41.41 1.075 0 1.486L7.28 17.03a.75.75 0 0 1-1.06-1.06l2.47-2.47-2.47-2.47a.75.75 0 0 1 0-1.06Z"; 

    protected override string IconGeometry32 { get; }
        = "M6 6h20a2 2 0 0 1 2 2v1H4V8a2 2 0 0 1 2-2Zm-4 5V8a4 4 0 0 1 4-4h20a4 4 0 0 1 4 4v16a4 4 0 0 1-4 4H6a4 4 0 0 1-4-4V11Zm2 0h24v13a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V11Zm13 12h7a1 1 0 1 0 0-2h-7a1 1 0 1 0 0 2Zm-8.707-9.707a1 1 0 0 1 1.414 0l3.717 3.717a1.4 1.4 0 0 1 0 1.98l-3.717 3.717a1 1 0 0 1-1.414-1.414L11.586 18l-3.293-3.293a1 1 0 0 1 0-1.414Z";
}