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

public class Camera : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M4.5 4h.618l.276-.553L6.118 2h3.764l.724 1.447.276.553H13a1 1 0 0 1 1 1v7a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V5a1 1 0 0 1 1-1h1.5ZM3 3h1.5l.724-1.447A1 1 0 0 1 6.118 1h3.764a1 1 0 0 1 .894.553L11.5 3H13a2 2 0 0 1 2 2v7a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2Zm7.5 5a2.5 2.5 0 1 1-5 0 2.5 2.5 0 0 1 5 0Zm1 0a3.5 3.5 0 1 1-7 0 3.5 3.5 0 0 1 7 0Z";

    protected override string IconGeometry20 { get; }
        = "M5.625 5h.773l.345-.691.905-1.809h4.704l.905 1.809.345.691h2.648c.69 0 1.25.56 1.25 1.25V15c0 .69-.56 1.25-1.25 1.25H3.75c-.69 0-1.25-.56-1.25-1.25V6.25C2.5 5.56 3.06 5 3.75 5h1.875ZM3.75 3.75h1.875l.905-1.809a1.25 1.25 0 0 1 1.118-.691h4.704c.474 0 .907.268 1.118.691l.905 1.809h1.875a2.5 2.5 0 0 1 2.5 2.5V15a2.5 2.5 0 0 1-2.5 2.5H3.75a2.5 2.5 0 0 1-2.5-2.5V6.25a2.5 2.5 0 0 1 2.5-2.5ZM13.125 10a3.125 3.125 0 1 1-6.25 0 3.125 3.125 0 0 1 6.25 0Zm1.25 0a4.375 4.375 0 1 1-8.75 0 4.375 4.375 0 0 1 8.75 0Z"; 

    protected override string IconGeometry24 { get; }
        = "M6.75 6h.927l.415-.83L9.177 3h5.646l1.085 2.17.415.83H19.5A1.5 1.5 0 0 1 21 7.5V18a1.5 1.5 0 0 1-1.5 1.5h-15A1.5 1.5 0 0 1 3 18V7.5A1.5 1.5 0 0 1 4.5 6h2.25ZM4.5 4.5h2.25l1.085-2.17a1.5 1.5 0 0 1 1.342-.83h5.646a1.5 1.5 0 0 1 1.342.83L17.25 4.5h2.25a3 3 0 0 1 3 3V18a3 3 0 0 1-3 3h-15a3 3 0 0 1-3-3V7.5a3 3 0 0 1 3-3ZM15.75 12a3.75 3.75 0 1 1-7.5 0 3.75 3.75 0 0 1 7.5 0Zm1.5 0a5.25 5.25 0 1 1-10.5 0 5.25 5.25 0 0 1 10.5 0Z"; 

    protected override string IconGeometry32 { get; }
        = "M9 8h1.236l.553-1.106L12.236 4h7.528l1.447 2.894L21.764 8H26a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V10a2 2 0 0 1 2-2h3ZM6 6h3l1.447-2.894A2 2 0 0 1 12.237 2h7.527a2 2 0 0 1 1.789 1.106L23 6h3a4 4 0 0 1 4 4v14a4 4 0 0 1-4 4H6a4 4 0 0 1-4-4V10a4 4 0 0 1 4-4Zm15 10a5 5 0 1 1-10 0 5 5 0 0 1 10 0Zm2 0a7 7 0 1 1-14 0 7 7 0 0 1 14 0Z";
}