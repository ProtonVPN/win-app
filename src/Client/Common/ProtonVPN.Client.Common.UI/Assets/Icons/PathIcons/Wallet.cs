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

public class Wallet : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M1 4a2 2 0 0 1 2-2h10.5A1.5 1.5 0 0 1 15 3.5v10a1.5 1.5 0 0 1-1.5 1.5H3a2 2 0 0 1-2-2V4Zm13-.5V5H3a1 1 0 0 1 0-2h10.5a.5.5 0 0 1 .5.5ZM3 6a1.99 1.99 0 0 1-1-.268V13a1 1 0 0 0 1 1h10.5a.5.5 0 0 0 .5-.5V6H3Zm8 4a.5.5 0 1 1 1 0 .5.5 0 0 1-1 0Zm.5-1.5a1.5 1.5 0 1 0 0 3 1.5 1.5 0 0 0 0-3Z";

    protected override string IconGeometry20 { get; }
        = "M1.25 5a2.5 2.5 0 0 1 2.5-2.5h13.125c1.035 0 1.875.84 1.875 1.875v12.5c0 1.035-.84 1.875-1.875 1.875H3.75a2.5 2.5 0 0 1-2.5-2.5V5Zm16.25-.625V6.25H3.75a1.25 1.25 0 1 1 0-2.5h13.125c.345 0 .625.28.625.625ZM3.75 7.5c-.455 0-.882-.122-1.25-.334v9.084c0 .69.56 1.25 1.25 1.25h13.125c.345 0 .625-.28.625-.625V7.5H3.75Zm10 5a.625.625 0 1 1 1.25 0 .625.625 0 0 1-1.25 0Zm.625-1.875a1.875 1.875 0 1 0 0 3.75 1.875 1.875 0 0 0 0-3.75Z"; 

    protected override string IconGeometry24 { get; }
        = "M1.5 6a3 3 0 0 1 3-3h15.75a2.25 2.25 0 0 1 2.25 2.25v15a2.25 2.25 0 0 1-2.25 2.25H4.5a3 3 0 0 1-3-3V6ZM21 5.25V7.5H4.5a1.5 1.5 0 1 1 0-3h15.75a.75.75 0 0 1 .75.75ZM4.5 9A2.986 2.986 0 0 1 3 8.599V19.5A1.5 1.5 0 0 0 4.5 21h15.75a.75.75 0 0 0 .75-.75V9H4.5Zm12 6a.75.75 0 1 1 1.5 0 .75.75 0 0 1-1.5 0Zm.75-2.25a2.25 2.25 0 1 0 0 4.5 2.25 2.25 0 0 0 0-4.5Z"; 

    protected override string IconGeometry32 { get; }
        = "M2 8a4 4 0 0 1 4-4h21a3 3 0 0 1 3 3v20a3 3 0 0 1-3 3H6a4 4 0 0 1-4-4V8Zm26-1v3H6a2 2 0 1 1 0-4h21a1 1 0 0 1 1 1ZM6 12a3.982 3.982 0 0 1-2-.535V26a2 2 0 0 0 2 2h21a1 1 0 0 0 1-1V12H6Zm16 8a1 1 0 1 1 2 0 1 1 0 0 1-2 0Zm1-3a3 3 0 1 0 0 6 3 3 0 0 0 0-6Z";
}