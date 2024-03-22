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

public class Trash : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M6.5 1a.5.5 0 0 0-.447.276L5.19 3H2a.5.5 0 0 0 0 1h1.022l.435 9.568A1.5 1.5 0 0 0 4.956 15h6.088a1.5 1.5 0 0 0 1.499-1.432L12.978 4H14a.5.5 0 0 0 0-1h-3.191l-.862-1.724A.5.5 0 0 0 9.5 1h-3Zm3.191 2-.5-1H6.809l-.5 1h3.382ZM4.023 4h7.954l-.433 9.523a.5.5 0 0 1-.5.477H4.956a.5.5 0 0 1-.5-.477L4.023 4ZM7 6a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0V6Zm3 0a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0V6Z";

    protected override string IconGeometry20 { get; }
        = "M8.125 1.25a.625.625 0 0 0-.559.345L6.489 3.75H2.5A.625.625 0 1 0 2.5 5h1.278l.543 11.96a1.875 1.875 0 0 0 1.873 1.79h7.612a1.875 1.875 0 0 0 1.873-1.79L16.222 5H17.5a.625.625 0 1 0 0-1.25h-3.989l-1.077-2.155a.625.625 0 0 0-.559-.345h-3.75Zm3.989 2.5-.625-1.25H8.51l-.625 1.25h4.228ZM5.029 5h9.942l-.541 11.903a.625.625 0 0 1-.624.597H6.194a.625.625 0 0 1-.624-.597L5.03 5ZM8.75 7.5a.625.625 0 1 0-1.25 0V15a.625.625 0 1 0 1.25 0V7.5Zm3.75 0a.625.625 0 1 0-1.25 0V15a.625.625 0 1 0 1.25 0V7.5Z"; 

    protected override string IconGeometry24 { get; }
        = "M9.75 1.5a.75.75 0 0 0-.67.415L7.785 4.5H3A.75.75 0 0 0 3 6h1.533l.653 14.352A2.25 2.25 0 0 0 7.433 22.5h9.134a2.25 2.25 0 0 0 2.247-2.148L19.467 6H21a.75.75 0 0 0 0-1.5h-4.787l-1.292-2.585a.75.75 0 0 0-.671-.415h-4.5Zm4.787 3-.75-1.5h-3.574l-.75 1.5h5.074ZM6.034 6h11.93l-.65 14.284a.75.75 0 0 1-.748.716H7.433a.75.75 0 0 1-.749-.716L6.034 6ZM10.5 9A.75.75 0 0 0 9 9v9a.75.75 0 0 0 1.5 0V9ZM15 9a.75.75 0 0 0-1.5 0v9a.75.75 0 0 0 1.5 0V9Z"; 

    protected override string IconGeometry32 { get; }
        = "M13 2a1 1 0 0 0-.894.553L10.382 6H4a1 1 0 0 0 0 2h2.044l.87 19.136A3 3 0 0 0 9.911 30H22.09a3 3 0 0 0 2.997-2.864L25.956 8H28a1 1 0 1 0 0-2h-6.382l-1.724-3.447A1 1 0 0 0 19 2h-6Zm6.382 4-1-2h-4.764l-1 2h6.764ZM8.046 8h15.907l-.865 19.045a1 1 0 0 1-1 .955H9.912a1 1 0 0 1-.999-.955L8.046 8ZM14 12a1 1 0 1 0-2 0v12a1 1 0 1 0 2 0V12Zm6 0a1 1 0 1 0-2 0v12a1 1 0 1 0 2 0V12Z";
}