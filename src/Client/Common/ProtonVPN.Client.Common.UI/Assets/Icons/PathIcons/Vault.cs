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

public class Vault : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M3 3h12v10H3V3ZM2 3a1 1 0 0 1 1-1h12a1 1 0 0 1 1 1v10a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1h-.5a.5.5 0 0 1 0-1H2V4h-.5a.5.5 0 0 1 0-1H2Zm9 7a2 2 0 1 0 0-4 2 2 0 0 0 0 4Zm0 1a3 3 0 1 0 0-6 3 3 0 0 0 0 6Zm1-3a1 1 0 1 1-2 0 1 1 0 0 1 2 0Zm-6-.134a1 1 0 1 0-1 0V9.5a.5.5 0 0 0 1 0V7.866Z";

    protected override string IconGeometry20 { get; }
        = "M3.75 3.75H17.5v12.5H3.75V3.75Zm-1.25 0c0-.69.56-1.25 1.25-1.25H17.5c.69 0 1.25.56 1.25 1.25v12.5c0 .69-.56 1.25-1.25 1.25H3.75c-.69 0-1.25-.56-1.25-1.25h-.625a.625.625 0 1 1 0-1.25H2.5V5h-.625a.625.625 0 1 1 0-1.25H2.5Zm10 8.75a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5Zm0 1.25a3.75 3.75 0 1 0 0-7.5 3.75 3.75 0 0 0 0 7.5ZM13.75 10a1.25 1.25 0 1 1-2.5 0 1.25 1.25 0 0 1 2.5 0Zm-6.875-.167a1.25 1.25 0 1 0-1.25 0v2.042a.625.625 0 1 0 1.25 0V9.833Z"; 

    protected override string IconGeometry24 { get; }
        = "M4.5 4.5H21v15H4.5v-15ZM3 4.5A1.5 1.5 0 0 1 4.5 3H21a1.5 1.5 0 0 1 1.5 1.5v15A1.5 1.5 0 0 1 21 21H4.5A1.5 1.5 0 0 1 3 19.5h-.75a.75.75 0 0 1 0-1.5H3V6h-.75a.75.75 0 0 1 0-1.5H3ZM15 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6Zm0 1.5a4.5 4.5 0 1 0 0-9 4.5 4.5 0 0 0 0 9Zm1.5-4.5a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0Zm-8.25-.2a1.5 1.5 0 1 0-1.5 0v2.45a.75.75 0 0 0 1.5 0V11.8Z"; 

    protected override string IconGeometry32 { get; }
        = "M6 6h22v20H6V6ZM4 6a2 2 0 0 1 2-2h22a2 2 0 0 1 2 2v20a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2H3a1 1 0 1 1 0-2h1V8H3a1 1 0 0 1 0-2h1Zm16 14a4 4 0 1 0 0-8 4 4 0 0 0 0 8Zm0 2a6 6 0 1 0 0-12 6 6 0 0 0 0 12Zm2-6a2 2 0 1 1-4 0 2 2 0 0 1 4 0Zm-11-.268A2 2 0 0 0 10 12a2 2 0 0 0-1 3.732V19a1 1 0 1 0 2 0v-3.268Z";
}