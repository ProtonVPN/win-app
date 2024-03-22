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

public class TrashCross : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M6.053 1.276A.5.5 0 0 1 6.5 1h3a.5.5 0 0 1 .447.276L10.81 3H14a.5.5 0 0 1 0 1h-1.022l-.435 9.568A1.5 1.5 0 0 1 11.044 15H4.956a1.5 1.5 0 0 1-1.499-1.432L3.022 4H2a.5.5 0 0 1 0-1h3.191l.862-1.724ZM9.19 2l.5 1H6.309l.5-1h2.382Zm2.786 2H4.023l.433 9.523a.5.5 0 0 0 .5.477h6.088a.5.5 0 0 0 .5-.477L11.977 4ZM6.354 6.646a.5.5 0 1 0-.708.708L7.293 9l-1.647 1.646a.5.5 0 0 0 .708.708L8 9.707l1.646 1.647a.5.5 0 0 0 .708-.708L8.707 9l1.647-1.646a.5.5 0 0 0-.708-.708L8 8.293 6.354 6.646Z";

    protected override string IconGeometry20 { get; }
        = "M7.566 1.595a.625.625 0 0 1 .559-.345h3.75c.237 0 .453.134.559.345l1.077 2.155H17.5a.625.625 0 1 1 0 1.25h-1.278l-.543 11.96a1.875 1.875 0 0 1-1.873 1.79H6.194a1.875 1.875 0 0 1-1.873-1.79L3.778 5H2.5a.625.625 0 1 1 0-1.25h3.989l1.077-2.155Zm3.923.905.625 1.25H7.886l.625-1.25h2.978ZM14.97 5H5.029l.541 11.903c.015.334.29.597.624.597h7.612a.625.625 0 0 0 .624-.597L14.97 5ZM7.94 8.308a.625.625 0 1 0-.883.884l2.058 2.058-2.058 2.058a.625.625 0 1 0 .884.884L10 12.134l2.058 2.058a.625.625 0 1 0 .884-.884l-2.058-2.058 2.058-2.058a.625.625 0 1 0-.884-.884L10 10.366 7.942 8.308Z"; 

    protected override string IconGeometry24 { get; }
        = "M9.08 1.915a.75.75 0 0 1 .67-.415h4.5a.75.75 0 0 1 .67.415L16.214 4.5H21A.75.75 0 0 1 21 6h-1.533l-.653 14.352a2.25 2.25 0 0 1-2.247 2.148H7.433a2.25 2.25 0 0 1-2.247-2.148L4.533 6H3a.75.75 0 0 1 0-1.5h4.786L9.08 1.915ZM13.786 3l.75 1.5H9.463l.75-1.5h3.572Zm4.178 3H6.035l.65 14.284a.75.75 0 0 0 .748.716h9.134a.75.75 0 0 0 .749-.716L17.966 6ZM9.53 9.97a.75.75 0 0 0-1.06 1.06l2.47 2.47-2.47 2.47a.75.75 0 1 0 1.06 1.06L12 14.56l2.47 2.47a.75.75 0 1 0 1.06-1.06l-2.47-2.47 2.47-2.47a.75.75 0 1 0-1.06-1.06L12 12.44 9.53 9.97Z"; 

    protected override string IconGeometry32 { get; }
        = "M12.106 2.553A1 1 0 0 1 13 2h6a1 1 0 0 1 .894.553L21.618 6H28a1 1 0 1 1 0 2h-2.044l-.87 19.136A3 3 0 0 1 22.089 30H9.91a3 3 0 0 1-2.997-2.864L6.044 8H4a1 1 0 0 1 0-2h6.382l1.724-3.447ZM18.382 4l1 2h-6.764l1-2h4.764Zm5.571 4H8.046l.866 19.045a1 1 0 0 0 1 .955h12.177a1 1 0 0 0 .999-.955L23.953 8Zm-11.246 5.293a1 1 0 0 0-1.414 1.414L14.586 18l-3.293 3.293a1 1 0 0 0 1.414 1.414L16 19.414l3.293 3.293a1 1 0 0 0 1.414-1.414L17.414 18l3.293-3.293a1 1 0 0 0-1.414-1.414L16 16.586l-3.293-3.293Z";
}